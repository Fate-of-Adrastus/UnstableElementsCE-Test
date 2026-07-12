using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;

namespace UnstableElements;

public class Solitaire{

	// TODO: just run hookgen with private methods on please
	private static Hook hookJournalEntryRender, hookSolitaireStateGetter, hookSolitaireStateSetter;

	private static Texture sigmarSprite, sigmarHoverSprite;
	private static HexIndex[] indicies = new DynamicData(typeof(SolitaireScreen)).Get<HexIndex[]>("field_3867");
	
	// current solitaire state
	public static SolitaireState UeSolitaireState;

	// element placement
	public static List<AtomType> Cardinals = new(){
		AtomTypes.salt, // salt
		AtomTypes.air, // air
		AtomTypes.earth, // earth
		AtomTypes.fire, // fire
		AtomTypes.water  // water
	};
	public static List<AtomType> Metals = new(){
		AtomTypes.silver, // silver
		AtomTypes.copper, // copper
		AtomTypes.iron, // iron
		AtomTypes.tin, // tin
		AtomTypes.lead  // lead
	};
	public static AtomType Quicksilver = AtomTypes.quicksilver;
	public static AtomType Gold = AtomTypes.gold;

	internal static void Load(){
		On.SolitaireFileManager.GetRandomFromFile += OnGenerateSolitaireBoard;
		
		hookJournalEntryRender = new Hook(
			typeof(JournalScreen).GetMethod("RenderPuzzleSelect", BindingFlags.Instance | BindingFlags.NonPublic),
			typeof(Solitaire).GetMethod("OnJournalEntryRender", BindingFlags.Static | BindingFlags.NonPublic)
		);
		hookSolitaireStateGetter = new Hook(
			typeof(SolitaireScreen).GetMethod("GetState", BindingFlags.Instance | BindingFlags.NonPublic),
			typeof(Solitaire).GetMethod("OnSolitaireScreenGetState", BindingFlags.Static | BindingFlags.NonPublic)
		);
		hookSolitaireStateSetter = new Hook(
			typeof(SolitaireScreen).GetMethod("SetState", BindingFlags.Instance | BindingFlags.NonPublic),
			typeof(Solitaire).GetMethod("OnSolitaireScreenSetState", BindingFlags.Static | BindingFlags.NonPublic)
		);

		sigmarSprite = AssetLoaderHelper.LoadTexture("UeJournal/sigmar");
		sigmarHoverSprite = AssetLoaderHelper.LoadTexture("UeJournal/sigmar_hover");
	}

	internal static void Unload(){
		On.SolitaireFileManager.GetRandomFromFile -= OnGenerateSolitaireBoard;
		
		hookJournalEntryRender?.Dispose();
		hookSolitaireStateGetter?.Dispose();
		hookSolitaireStateSetter?.Dispose();
	}

	private static SolitaireGameState GenerateSolitaireBoard(){
		SolitaireGameState state = new(){
			atoms = { // gold in the centre
				[new HexIndex(5, 0)] = Gold
			}
		};
		
		// generate via a series of valid moves
		// go for marbles + metals
		Random rng = new(7893);
		int curMetal = 0;
		int[] cardinalsPlaced = new int[Cardinals.Count];
		int aethers = 0;
		while(true){
			List<AtomType> choices = new(6);
			// could choose a cardinal that we don't have enough of
			for(var idx = 0; idx < cardinalsPlaced.Length; idx++)
				if(cardinalsPlaced[idx] < 6)
					choices.Add(Cardinals[idx]);
			// could choose a metal, if we have any left
			if(curMetal < Metals.Count)
				choices.Add(Metals[curMetal]);
			// could choose aether; higher priority to hopefully give them enough space
			if(aethers < 6){
				choices.Add(Atoms.Aether);
				choices.Add(Atoms.Aether);
			}

			if(choices.Count == 0)
				break; // we're done!

			AtomType next = rng.GetElement(choices);
			if(next == Atoms.Aether){
				HexIndex pos = RandomFree(state, null, rng, threshold: 6);
				state.atoms[pos] = next;
				aethers++;
			}else{
				HexIndex pos = RandomFree(state, null, rng);
				HexIndex pos2 = RandomFree(state, pos, rng);
				if(Cardinals.Contains(next)){
					state.atoms[pos] = next;
					state.atoms[pos2] = next;
					cardinalsPlaced[Cardinals.IndexOf(next)] += 2;
				}else{
					state.atoms[pos] = next;
					state.atoms[pos2] = Quicksilver;
					curMetal++;
				}
			}
		}

		return state;
	}

	private static HexIndex RandomFree(SolitaireGameState current, HexIndex? exclude, Random rng, int threshold = 3){
		if(exclude != null){
			current = current.Clone();
			current.atoms[exclude.Value] = AtomTypes.salt;
		}

		return rng.ChooseOrElse(indicies.Where(v => IsValidPlacement(v, current, threshold)).ToList(), new HexIndex(0, 0));
	}
	
	private static bool IsValidPlacement(HexIndex pos, SolitaireGameState self, int threshold){
		if(self.atoms.ContainsKey(pos))
			return false;

		int currentBlanks = 0;
		int maxBlanks = 0;
		for(int index = 0; index < 2; ++index){
			foreach(HexIndex adjacentOffset in HexIndex.AdjacentOffsets)
				if(self.atoms.ContainsKey(pos + adjacentOffset))
					currentBlanks = 0;
				else{
					++currentBlanks;
					maxBlanks = Math.Max(maxBlanks, currentBlanks);
				}
		}

		return maxBlanks >= threshold;
	}
	
	private static SolitaireGameState OnGenerateSolitaireBoard(On.SolitaireFileManager.orig_GetRandomFromFile orig, SolitaireType type){
		return SolitaireExt.IsCurrentSolitaireUe() ? GenerateSolitaireBoard() : orig(type);
	}
	
	private delegate void orig_method_1040(JournalScreen self, Puzzle puzzle, Vector2 pos, bool big);
	private static void OnJournalEntryRender(orig_method_1040 orig, JournalScreen self, Puzzle puzzle, Vector2 pos, bool big){
		if(puzzle.puzzleId == "QuickIron"){
			Texture puzzleBg = big ? Assets.textures.field_88.field_894 : Assets.textures.field_88.field_895;
			Texture tick = true /* TODO: count wins */ ? Assets.textures.field_96.field_879 : Assets.textures.field_96.field_882;
			Texture divider = big ? Assets.textures.field_88.field_892 : Assets.textures.field_88.field_893;
			Bounds2 bounds = Bounds2.WithSize(pos, puzzleBg.size.ToVector2());
			bool hover = bounds.Contains(Input.MousePos());
			TextureRenderer.RenderText("Shattered Garden", pos + new Vector2(9, -19), Assets.fonts.crimson_15, class_181.field_1718, 0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
			UI.DrawTexture(tick, pos + new Vector2(puzzleBg.size.X - 27, -23f));
			UI.DrawTexture(puzzleBg, pos);
			UI.DrawTexture(divider, pos + new Vector2(7f, -34f));
			UI.DrawTexture(hover ? sigmarHoverSprite : sigmarSprite, bounds.Min + new Vector2(13f, 13f));
			if(hover && Input.IsLeftClickPressed()){
				var solitaireScreen = new SolitaireScreen((SolitaireType)1);
				solitaireScreen.SetUe(true);
				UI.OpenScreen(solitaireScreen);
				Assets.sounds.field_1821.method_28(1f);
			}
		}else
			orig(self, puzzle, pos, big);
	}

	private static SolitaireState OnSolitaireScreenGetState(Func<SolitaireScreen, SolitaireState> orig, SolitaireScreen self)
		=> self.IsUe() ? UeSolitaireState : orig(self);

	private static void OnSolitaireScreenSetState(Action<SolitaireScreen, SolitaireState> orig, SolitaireScreen self, SolitaireState next){
		if(self.IsUe())
			UeSolitaireState = next;
		else orig(self, next);
	}
}

internal static class SolitaireExt{

	private const string ueTag = "UnstableElements";
	
	internal static bool IsUe(this SolitaireScreen screen)
		=> new DynamicData(screen).TryGet(ueTag, out bool? t) && t == true;

	internal static void SetUe(this SolitaireScreen screen, bool value)
		=> new DynamicData(screen).Set(ueTag, value);

	internal static bool IsCurrentSolitaireUe()
		=> GameLogic.instance.GetLastScreen() is SolitaireScreen screen && screen.IsUe();
}

internal static class RandomExt{

	public static T ChooseOrElse<T>(this Random rng, List<T> from, T fallback) => from.Count > 0 ? rng.GetElement(from) : fallback;
}