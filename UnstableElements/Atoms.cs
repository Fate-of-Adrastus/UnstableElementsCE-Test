using System;
using System.Collections.Generic;
using System.Linq;
using Quintessential;

namespace UnstableElements;

internal static class Atoms{
	
	public static AtomType Aether, Uranium;

    public static readonly List<AtomType> UraniumIsotopes = new(), SlowShakingIso = new(), FastShakingIso = new();

    public static readonly AtomTypeEq AtomComparator = new();
    public static readonly Random UraniumShakeCounter = new(85934);

	public static void AddAtomTypes(){
		// Aether atom type
		Aether = new(){
            symbol = AssetLoaderHelper.LoadTexture("textures/atoms/leppa/UnstableElements/aether_symbol"),
            shadow = AssetLoaderHelper.LoadTexture("textures/atoms/leppa/UnstableElements/aether_shadow"),
            isPrismatic = true,
            prismaticTextures = new(){
                base1 = Assets.textures.atoms.elements.quintessence_base,
                colors = AssetLoaderHelper.LoadTexture("textures/atoms/leppa/UnstableElements/aether_colors"),
                colorsMask = Assets.textures.atoms.elements.quintessence_mask,
                rimlight = Assets.textures.atoms.elements.quintessence_rimlight
            }
        };
        UnstableElements.Instance.AddAtomType(Aether, "aether");

		// Uranium atom types
		for(int phase = 0; phase < 3; phase++)
			for(int turn = 0; turn < 3; turn++){
				AtomType isotope = new(){
                    symbol = AssetLoaderHelper.LoadTexture($"textures/atoms/leppa/UnstableElements/uranium_symbol_{phase}"),
                    shadow = Assets.textures.atoms.shadow,
                    metallicTextures = new(){
                        diffuse = Assets.textures.atoms.copper_diffuse,
                        lightramp = AssetLoaderHelper.LoadTexture($"textures/atoms/leppa/UnstableElements/uranium_lightramp_{phase}"),
						rimlight = Assets.textures.atoms.silver_rimlight
                    },
                    isMetallic = true
				};
				if(phase == 0 && turn == 0){
                    Uranium = isotope;
				} else {
					isotope.QuintAtomType = UnstableElements.Instance.GetIdentifier($"uranium_{phase}_{turn}");
					isotope.name = Translations.Translate("unstable_elements.atoms.uranium");
					isotope.elementalName = Translations.Translate("unstable_elements.atoms.uranium.elemental");
					isotope.defaultName = Translations.Translate("unstable_elements.atoms.uranium").locDictionary[Language.English];
				}
				UraniumIsotopes.Add(isotope);
				if(phase == 1)
					SlowShakingIso.Add(isotope);
				else if(phase == 2)
					FastShakingIso.Add(isotope);
			}

        UnstableElements.Instance.AddAtomType(Uranium, "uranium");

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
						seb.field_3936.Add(new GlyphEffect(seb, (EffectTimescaleType)1, HexGrid.standardGrid.ToPixelCoords(atom.Key) + new Vector2(80f, 0.0f), Assets.textures.parts.disposal_flash /* or 42? */, 30f, Vector2.Zero, 0.0f));
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
									DoDecay(molecule, atom.Value, atom.Key, seb, AtomTypes.lead);
								else if(idx > 0 || grabbed)
									atom.Value.atomType = UraniumIsotopes[idx + 1];
								break;
							}
			}
		});
	}

	public static void DoDecay(Molecule m, Atom u, HexIndex pos, SolutionEditorBase seb, AtomType result){
		AtomType from = u.atomType;
		m.ReplaceAtom(result, pos);
		u.transmutationEffect = new TransmutationEffect(seb, (TransmutationEffectRenderMode)1, from, Assets.textures.atoms.projection_effect, 30f);
	}

	public static bool IsUraniumState(AtomType type) => UraniumIsotopes.Contains(type, AtomComparator);

    //private static void OnMoleculeEditorRender(On.MoleculeEditorScreen.orig_RenderFrame orig, MoleculeEditorScreen self, float deltaTime) {
    //	orig(self, deltaTime);
    //	DynamicData selfData = new(self);
    //	// if there's no existing error...
    //	if(!selfData.Get<Maybe<LocString>>("errorMessage").HasValue()){
    //		Molecule m = selfData.Get<Molecule>("molecule");
    //		// and there are only a nonzero amount of Aether atoms...
    //		if(m.GetAtoms().Count > 0 && m.GetAtoms().Values.Select(u => u.atomType).All(u => u.Equals(Aether))){
    //			// display a warning
    //			Vector2 sizeM = new Vector2(1516f, 922f);
    //			Vector2 centreM = (InputManager.screenSize / 2 - sizeM / 2 + new Vector2(-2f, -11f)).Rounded();
    //            UIUtils.RenderScreenTitle("WARNING: Pure-aether molecules require a Glyph of Tranquility to handle.", centreM + new Vector2(471f, 107f), 922, false, false);
    //		}
    //	}
    //}

    public static bool IsHexStabilized(HexIndex h) => Parts.TranquilityHexes.Contains(h) || Parts.OtherStableHexes.Contains(h);


    // TODO: fix properly in quintessential
    public class AtomTypeEq : IEqualityComparer<AtomType>{
		public bool Equals(AtomType x, AtomType y){
			return x.QuintAtomType == y.QuintAtomType;
		}

		public int GetHashCode(AtomType obj){
			return obj.QuintAtomType.GetHashCode();
		}
	}
}