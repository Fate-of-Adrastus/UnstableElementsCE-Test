using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;

namespace UnstableElements;

internal static class Atoms{
	
	public static AtomType Aether, Uranium;

	private static readonly List<AtomType> UraniumIsotopes = new(), SlowShakingIso = new(), FastShakingIso = new();

	private static readonly AtomTypeEq AtomComparator = new();
	private static readonly Random UraniumShakeCounter = new(85934);
	private static ILHook SimValidationHook;
	private static Hook AetherBlockerHook;

	public static void AddAtomTypes(){
		// Aether atom type
		Aether = new(){
            byteId = 64,
            defaultName = Translations.WithAllLanguages("Aether"),
            elementalName = Translations.Translate("Elemental Aether"),
            name = Translations.Translate("Aether"),
            symbol = AssetLoaderHelper.LoadTexture("textures/atoms/leppa/UnstableElements/aether_symbol"),
            shadow = AssetLoaderHelper.LoadTexture("textures/atoms/leppa/UnstableElements/aether_shadow")
		};
        PrismaticAtomTextures aetherColours = new(){
            base1 = Assets.textures.field_81.field_613.field_627,
            colors = AssetLoaderHelper.LoadTexture("textures/atoms/leppa/UnstableElements/aether_colors"),
            mask = Assets.textures.field_81.field_613.field_629,
            rimlight = Assets.textures.field_81.field_613.field_630
		};
		Aether.prismaticTextures = aetherColours;
		Aether.isPrismatic = true;
		Aether.QuintAtomType = "UnstableElements:aether";

		QApi.AddAtomType(Aether);

		// Uranium atom types
		for(int phase = 0; phase < 3; phase++)
			for(int turn = 0; turn < 3; turn++){
				AtomType isotope = new(){
                    byteId = 65,
                    defaultName = Translations.WithAllLanguages("Uranium"),
                    elementalName = Translations.Translate("Elemental Uranium"),
                    name = Translations.Translate("Uranium"),
                    symbol = AssetLoaderHelper.LoadTexture($"textures/atoms/leppa/UnstableElements/uranium_symbol_{phase}"),
                    shadow = Assets.textures.field_81.field_599,
                    metallicTextures = new(){
                        diffuse = Assets.textures.field_81.field_577,
                        lightramp = AssetLoaderHelper.LoadTexture($"textures/atoms/leppa/UnstableElements/uranium_lightramp_{phase}"),
						rimlight = Assets.textures.field_81.field_601
					},
                    isMetallic = true
				};
				if(phase == 0 && turn == 0){
					isotope.QuintAtomType = "UnstableElements:uranium";
					Uranium = isotope;
				}else
					isotope.QuintAtomType = $"UnstableElements:uranium:{phase}_{turn}";
				UraniumIsotopes.Add(isotope);
				if(phase == 1)
					SlowShakingIso.Add(isotope);
				else if(phase == 2)
					FastShakingIso.Add(isotope);
			}
		
		QApi.AddAtomType(Uranium);

		// Aether self-destruction
		QApi.RunAfterCycle((sim, first) => {
			if(!first){
				List<Molecule> toRemove = new();
				var molecules = sim.molecules;
				foreach(var molecule in molecules){
					bool hasAether = false, hasNonAether = false;
					foreach(KeyValuePair<HexIndex, Atom> atom in molecule.GetAtoms())
						if(atom.Value.atomType.Equals(Aether)){
							if(!IsHexStabilized(atom.Key))
								hasAether = true;
						}else
							hasNonAether = true;

					if(hasAether && !hasNonAether)
						toRemove.Add(molecule);
				}

				foreach(var it in toRemove){
					foreach(KeyValuePair<HexIndex, Atom> atom in it.GetAtoms()){
						var seb = sim.solutionEditor;
						seb.field_3936.Add(new GlyphEffect(seb, (EffectTimescaleType)1, HexGrid.standardGrid.ToWorldCoords(atom.Key) + new Vector2(80f, 0.0f), Assets.textures.field_90.field_240 /* or 42? */, 30f, Vector2.Zero, 0.0f));
					}

					molecules.Remove(it);
				}
			}
		});

		// Uranium heating
		QApi.RunAfterCycle((sim, first) => {
			if(first) return;
			
			var seb = sim.solutionEditor;
			var molecules = sim.molecules;
			foreach(var molecule in molecules){
				// atoms of initial uranium only decay if the molecule containing them is grabbed
				bool grabbed = sim.simulationDict.Values.Any(state => state.heldMolecule == molecule);
				foreach(KeyValuePair<HexIndex, Atom> atom in molecule.GetAtoms())
					if(!IsHexStabilized(atom.Key))
						for(var idx = 0; idx < UraniumIsotopes.Count; idx++)
							if(atom.Value.atomType.QuintAtomType == UraniumIsotopes[idx].QuintAtomType){
								if(idx == UraniumIsotopes.Count - 1)
									DoUraniumDecay(molecule, atom.Value, atom.Key, seb);
								else if(idx > 0 || grabbed)
									atom.Value.atomType = UraniumIsotopes[idx + 1];
								break;
							}
			}
		});

		// Uranium visuals (shaking, heating)
		On.Editor.RenderAtom += OnAtomRender;
		// Molecule editor warning for pure-aether atoms
		//On.MoleculeEditorScreen.RenderFrame += OnMoleculeEditorRender;
		// Shaking uranium validation
        SimValidationHook = new(typeof(Sim).GetMethod("IsSameMolecule", BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(Molecule), typeof(Molecule) }, null), ModSimValidate);
		// Blocking unstable pure-aether inputs
		AetherBlockerHook = new(typeof(Sim).GetMethod("HasOverlap", BindingFlags.NonPublic | BindingFlags.Instance), CheckInputProduction);
	}

	public static void Unload(){
		On.Editor.RenderAtom -= OnAtomRender;
		//On.MoleculeEditorScreen.RenderFrame -= OnMoleculeEditorRender;
		SimValidationHook.Dispose();
		AetherBlockerHook.Dispose();
	}

	public static void DoUraniumDecay(Molecule m, Atom u, HexIndex pos, SolutionEditorBase seb){
		AtomType from = u.atomType;
		m.ReplaceAtom(AtomTypes.lead, pos);
		u.transmutationEffect = new TransmutationEffect(seb, (TransmutationEffectRenderMode)1, from, Assets.textures.field_81.field_614, 30f);
	}

	public static bool IsUraniumState(AtomType type) => UraniumIsotopes.Contains(type, AtomComparator);

	private static void OnAtomRender(On.Editor.orig_RenderAtom orig, AtomType type, Vector2 position, float param_4582, float param_4583, float param_4584, float param_4585, float param_4586, float param_4587, Texture overrideShadow, Texture maskM, bool param_4590){
		if(SlowShakingIso.Contains(type, AtomComparator))
			position += new Vector2(UraniumShakeCounter.GetFloat(-4,4) / 4f, UraniumShakeCounter.GetFloat(-4,4) / 4f);
		if(FastShakingIso.Contains(type, AtomComparator))
			position += new Vector2(UraniumShakeCounter.GetFloat(-4, 4) / 2f, UraniumShakeCounter.GetFloat(-4,4) / 2f);
		orig(type, position, param_4582, param_4583, param_4584, param_4585, param_4586, param_4587, overrideShadow, maskM, param_4590);
	}

	private static void ModSimValidate(ILContext il){
		ILCursor cursor = new(il);
		while(cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdfld("Atom", "atomType"))){
			cursor.Remove();
			cursor.EmitDelegate<Func<Atom, AtomType>>(u => {
				AtomType type = u.atomType;
				return IsUraniumState(type) ? Uranium : type;
			});
		}
	}

	private static void OnMoleculeEditorRender(On.MoleculeEditorScreen.orig_RenderFrame orig, MoleculeEditorScreen self, float deltaTime) {
		orig(self, deltaTime);
		DynamicData selfData = new(self);
		// if there's no existing error...
		if(!selfData.Get<Maybe<LocString>>("errorMessage").HasValue()){
			Molecule m = selfData.Get<Molecule>("molecule");
			// and there are only a nonzero amount of Aether atoms...
			if(m.GetAtoms().Count > 0 && m.GetAtoms().Values.Select(u => u.atomType).All(u => u.Equals(Aether))){
				// display a warning
				Vector2 sizeM = new Vector2(1516f, 922f);
				Vector2 centreM = (InputManager.screenSize / 2 - sizeM / 2 + new Vector2(-2f, -11f)).Rounded();
                UIUtils.RenderScreenTitle("WARNING: Pure-aether molecules require a Glyph of Tranquility to handle.", centreM + new Vector2(471f, 107f), 922, false, false);
			}
		}
	}

	private static bool IsHexStabilized(HexIndex h) => Parts.TranquilityHexes.Contains(h) || Parts.OtherStableHexes.Contains(h);

	public delegate bool orig_HasOverlap(Sim self, Molecule toCheck, HashSet<HexIndex> moleculeFootprint);

	public static bool CheckInputProduction(orig_HasOverlap orig, Sim self, Molecule toCheck, HashSet<HexIndex> moleculeFootprint){
		bool blocked = orig(self, toCheck, moleculeFootprint);
		if(!blocked) // if its not blocked by collisions, but is made of Aether and not stabilized, block it
			if(toCheck.GetAtoms().Values.Any() && toCheck.GetAtoms().Values.Select(u => u.atomType).All(u => u.QuintAtomType?.Equals(Aether.QuintAtomType) ?? false))
				if(!toCheck.GetAtoms().Keys.All(IsHexStabilized))
					return true;
		return blocked;
	}

	// TODO: fix properly in quintessential
	private class AtomTypeEq : IEqualityComparer<AtomType>{
		public bool Equals(AtomType x, AtomType y){
			return x.QuintAtomType == y.QuintAtomType;
		}

		public int GetHashCode(AtomType obj){
			return obj.QuintAtomType.GetHashCode();
		}
	}
}