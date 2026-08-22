using System;
using System.Collections.Generic;
using System.Linq;
using MonoMod.Utils;

namespace UnstableElements;

public class Solitaire{
	public static Texture sigmarSprite, sigmarHoverSprite;
	private static HexIndex[] indicies = new DynamicData(typeof(SolitaireScreen)).Get<HexIndex[]>("orderedBoardHexes");
	
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

		sigmarSprite = AssetLoaderHelper.LoadTexture("UeJournal/sigmar");
		sigmarHoverSprite = AssetLoaderHelper.LoadTexture("UeJournal/sigmar_hover");
	}
	
	public static SolitaireGameState GenerateSolitaireBoard(){
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
}

internal static class SolitaireExt{

	private const string ueTag = "UnstableElements";
	
	internal static bool IsUe(this SolitaireScreen screen)
		=> new DynamicData(screen).TryGet(ueTag, out bool? t) && t == true;

	internal static void SetUe(this SolitaireScreen screen, bool value)
		=> new DynamicData(screen).Set(ueTag, value);

	internal static bool IsCurrentSolitaireUe()
		=> GameLogic.instance.GetCurrentScreen() is SolitaireScreen screen && screen.IsUe();
}

internal static class RandomExt{

	public static T ChooseOrElse<T>(this Random rng, List<T> from, T fallback) => from.Count > 0 ? rng.GetElement(from) : fallback;
}