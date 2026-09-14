using MonoMod.Utils;
using Quintessential;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnstableElements;

internal static class Parts{
	
	public static PartType Irradiation, Volatility, Tranquility, Sublimation;

	public static Texture IrradiationBase = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/irradiation_base");
	public static Texture IrradiationGoldSymbol = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/gold_symbol");
	public static Texture IrradiationMetalBowl = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/irradiation_bowl");

	public static Texture VolatilitySymbol = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/volatility_symbol");
	public static Texture VolatilityBowl = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/volatility_bowl");

	public static Texture TranquilityBase = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/tranquility_base");
	public static Texture TranquilityQuicksilverSymbol = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/quicksilver_symbol");
	public static Texture TranquilityMetalBowl = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/tranquility_bowl");
	public static Texture TranquilityProjectors = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/tranquility_projectors");
	public static Texture TranquilityZoneHex = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/tranquility_zone_hex");
	public static Color TranquilityZoneColor = new(255 / 255f, 251 / 255f, 199 / 255f, 255 / 255f);

	public static Texture SublimationBelowIris = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/sublimation_below_iris");
	public static Texture SublimationAboveIris = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/sublimation_above_iris");
	public static Texture SublimationQuintessenceSymbol = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/quintessence_symbol");
	public static Texture[] SublimationAetherIris = new Texture[16];
	public static Texture[] SublimationSaltIris = new Texture[16];

	public static readonly HashSet<HexIndex> TranquilityHexes = new();
	
	internal static readonly List<Func<Sim, HashSet<HexIndex>>> OtherStableHexesCallbacks = new();
	public static readonly HashSet<HexIndex> OtherStableHexes = new();

	private static readonly string TranquilityPowerId = "UnstableElements:tranquility_powered";
    private static readonly string SublimationMoleculeStorage = "UnstableElements:sublimation_molecules";

    private static readonly HashSet<HexIndex> TranquilityOffsets = new(){
		new(1, -1),
		new(1, -2), new(2, -2),
		new(1, -3), new(2, -3), new(3, -3),
		new(1, -4), new(2, -4), new(3, -4), new(4, -4)
	};

	public static void AddPartTypes() {
		for (int i = 0; i < 16; i++) {
			SublimationAetherIris[i] = AssetLoaderHelper.LoadTexture($"textures/parts/leppa/UnstableElements/iris_full_aether.array/iris_full_00{i + 1:D2}");
			SublimationSaltIris[i] = AssetLoaderHelper.LoadTexture($"textures/parts/leppa/UnstableElements/iris_full_salt.array/iris_full_00{i + 1:D2}");
		}

		Irradiation = new() {
			cost = 25,
			isFullHexCover = true,
			glowTexture = Assets.textures.select.tetra_glow,
			strokeTexture = Assets.textures.select.tetra_stroke,
			baseTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/irradiation"),
			hoverTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/irradiation_hovered"),
			glyphHexes = new HexIndex[]{
				new(0, 0),
				new(-1, 1),
				new(1, 0),
				new(0, -1)
			},
			permissionCategory = PuzzlePermissions.None,
			CustomPermissionCheck = perms => perms.Contains(UnstableElements.Instance.GetIdentifier("irradiation"))
		};

		Volatility = new() {
			cost = 10,
			isFullHexCover = true,
			glowTexture = Assets.textures.select.single_glow,
			strokeTexture = Assets.textures.select.single_stroke,
			baseTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/volatility"),
			hoverTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/volatility_hovered"),
			glyphHexes = new HexIndex[]{
				new(0, 0)
			},
			permissionCategory = PuzzlePermissions.None,
			CustomPermissionCheck = perms => perms.Contains(UnstableElements.Instance.GetIdentifier("volatility"))
		};

		Tranquility = new() {
			cost = 40,
			isFullHexCover = true,
			glowTexture = Assets.textures.select.triple_glow,
			strokeTexture = Assets.textures.select.triple_stroke,
			baseTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/tranquility"),
			hoverTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/tranquility_hovered"),
			glyphHexes = new HexIndex[]{
				new(0, 0),
				new(1, 0),
				new(0, 1)
			},
			permissionCategory = PuzzlePermissions.None,
			CustomPermissionCheck = perms => perms.Contains(UnstableElements.Instance.GetIdentifier("tranquility"))
		};

		Sublimation = new() {
			cost = 10,
			isFullHexCover = true,
			glowTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/sublimation_glow"),
			strokeTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/sublimation_stroke"),
			baseTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/sublimation"),
			hoverTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/sublimation_hovered"),
			glyphHexes = new HexIndex[]{
				new(0, 0),
				new(0, 1),
				new(1, 1),
				new(0, -1),
				new(-1, -1)
			},
			permissionCategory = PuzzlePermissions.None,
			CustomPermissionCheck = perms => perms.Contains(UnstableElements.Instance.GetIdentifier("sublimation"))
		};

		UnstableElements.Instance.AddPartType(Irradiation, "irradiation", (part, pos, editor, renderer) => {
			Vector2 vector2 = new(83f, 119f);
			renderer.RenderBase(IrradiationBase, new Vector2(0.0f, -1f), vector2, 0.0f);
			foreach (HexIndex idx in part.GetType().glyphHexes) {
				if (idx is { Q: 0, R: 0 }) {
					renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, idx, 0);
					renderer.RenderRotating(IrradiationMetalBowl, idx, Vector2.Zero);
					renderer.RenderUpright(IrradiationGoldSymbol, idx, Vector2.Zero);
				} else {
					renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, idx, 0);
					renderer.RenderShadowCircle(Assets.textures.parts.projection_glyph.quicksilver_input, idx, 0);
					// should be 272?
					renderer.RenderUpright(Assets.textures.parts.projection_glyph.quicksilver_symbol, idx, Vector2.Zero);
				}
			}

			for (var i = 0; i < part.GetType().glyphHexes.Length; i++) {
				HexIndex hexIndex = part.GetType().glyphHexes[i];
				if (hexIndex != new HexIndex(0, 0)) {
					int index = i - 1;
					float num = new HexRotation(index * 2).ToRadians();
					renderer.RenderBase(Assets.textures.parts.projection_glyph.bond, new Vector2(-30f, 12f), num);
				}
			}
		});
		UnstableElements.Instance.AddPartType(Volatility, "volatility", (part, pos, editor, renderer) => {
			Texture calcinatorBase = Assets.textures.parts.calcinator_base;
			Vector2 centre = (calcinatorBase.size.ToVector2() / 2).Rounded() + new Vector2(0, 1);
			renderer.RenderBase(calcinatorBase, centre);
			renderer.RenderShadowCircle(Assets.textures.parts.animismus.ring_shadow, new HexIndex(0, 0), 3);
			renderer.RenderRotating(VolatilityBowl, new HexIndex(0, 0), Vector2.Zero);
			renderer.RenderBase(VolatilitySymbol, centre);
		});
		UnstableElements.Instance.AddPartType(Tranquility, "tranquility", (part, pos, editor, renderer) => {
			Vector2 vector2 = new(42, 48);
			renderer.RenderBase(TranquilityBase, new Vector2(-1, -1), vector2, 0);
			HexIndex qsSite = new(0, 1);
			renderer.RenderShadowCircle(Assets.textures.parts.bonder_shadow, qsSite, 0);
			renderer.RenderRotating(TranquilityMetalBowl, qsSite, Vector2.Zero);
			renderer.RenderUpright(TranquilityQuicksilverSymbol, qsSite, Vector2.Zero);

			double time = Math.Sin(new DeltaTime(Time.Now().Ticks).InSeconds());
			float pulse = (float)(time / 3 + .66);
			if (editor.GetSimPlayState() != SimPlayState.Stopped && new DynamicData(part).TryGet(TranquilityPowerId, out bool? power) && power == true) {
				Color tint = Color.White;
				tint.A *= pulse;
				DrawForPartWithTint(renderer, TranquilityProjectors, new Vector2(-1, -1), vector2, 0, tint);
			}
		});
		UnstableElements.Instance.AddPartType(Sublimation, "sublimation", (part, pos, editor, renderer) => {
			PartSimState myState = editor.GetSimulation().GetSimState(part);
			Vector2 vector2 = new(330 / 2, 238 / 2);
			var renderInfo = editor.GetIntermState(part, pos);

			// base
			renderer.RenderBase(SublimationBelowIris, new Vector2(-1, -1), vector2, 0);

			// centre hex
			renderer.RenderUpright(SublimationQuintessenceSymbol, new(0, 0), new(3, 3));
			if (myState.isProcessing) // disappearing quintessence for active glyph
				Editor.RenderMolecule(Molecule.CreateMonoatomic(myState.processingAtoms[0]), RelativeToPart(renderInfo, new(0, 0)), new(0, 0), 0.0f, 1f, 1f - editor.GetCycleTime(), 1f, false, null);

            new DynamicData(myState).TryGet(SublimationMoleculeStorage, out Molecule[] molecules);

            // irises
            var animIdx = 15;
            float progress = 0;
            if (myState.isProcessing) {
                animIdx = Utils.Clamp((int)((double)Utils.InterpolateLinear(1f, -1f, editor.GetCycleTime()) * 16), 0, 15);
                progress = editor.GetCycleTime();
            }

            if (progress < 0.5 && molecules != null) { // render under irises
				Editor.RenderMolecule(molecules[0], renderInfo.pos, part.GetHexPos(), 0, 1f, progress, 1f, false, null);
                Editor.RenderMolecule(molecules[1], renderInfo.pos, part.GetHexPos(), 0, 1f, progress, 1f, false, null);
            }

            renderer.RenderUpright(SublimationSaltIris[animIdx], new(0, 1), new(2, 0));
            renderer.RenderUpright(SublimationSaltIris[animIdx], new(0, -1), new(2, 0));
            renderer.RenderUpright(SublimationAetherIris[animIdx], new(1, 1), new(2, 0));
            renderer.RenderUpright(SublimationAetherIris[animIdx], new(-1, -1), new(2, 0));
            if (progress > 0.5 && molecules != null) { // render over irises
				Debugger.Break();
				var a = molecules[0].GetAtoms();
                Editor.RenderMolecule(molecules[0], renderInfo.pos, part.GetHexPos(), 0, 1f, progress, 1f, false, null);
                Editor.RenderMolecule(molecules[1], renderInfo.pos, part.GetHexPos(), 0, 1f, progress, 1f, false, null);
            }


            // top
            renderer.RenderBase(SublimationAboveIris, new Vector2(-1, -1), vector2, 0);
		});

		QApi.AddPartTypeToPanel(Irradiation, PartTypes.triplexBonder);
		QApi.AddPartTypeToPanel(Volatility, PartTypes.triplexBonder);
		QApi.AddPartTypeToPanel(Tranquility, PartTypes.triplexBonder);
		QApi.AddPartTypeToPanel(Sublimation, PartTypes.triplexBonder);

		UnstableElements.Instance.AddPuzzlePermission("irradiation");
		UnstableElements.Instance.AddPuzzlePermission("volatility");
		UnstableElements.Instance.AddPuzzlePermission("tranquility");
		UnstableElements.Instance.AddPuzzlePermission("sublimation");

		QApi.RunAfterCycle((sim, _) => {
			// first thing
			TranquilityHexes.Clear();

			OtherStableHexes.Clear();
			foreach (var cb in OtherStableHexesCallbacks)
				OtherStableHexes.UnionWith(cb(sim));
		});

		UnstableElements.Instance.AddRecipe(new(static (sim, part) => {
			Debugger.Break();
			if (!sim.GetAtomReference(part, new(-1, 1), false, out var qs1) ||
				qs1.inMultiAtomMolecule || qs1.isHeldByArm || qs1.atomType != AtomTypes.quicksilver ||
                !sim.GetAtomReference(part, new(1, 0), false, out var qs2) ||
                qs2.inMultiAtomMolecule || qs2.isHeldByArm || qs2.atomType != AtomTypes.quicksilver ||
                !sim.GetAtomReference(part, new(0, -1), false, out var qs3) ||
                qs3.inMultiAtomMolecule || qs3.isHeldByArm || qs3.atomType != AtomTypes.quicksilver ||
                !sim.GetAtomReference(part, new(0, 0), false, out var gold) ||
                gold.atomType != AtomTypes.gold) return false;

			sim.RecipeInputs[new(0, 0)] = gold;
            sim.RecipeInputs[new(-1, 1)] = qs1;
            sim.RecipeInputs[new(1, 0)] = qs2;
            sim.RecipeInputs[new(0, -1)] = qs3;
			sim.RecipeOutputs[new(0, 0)] = Atoms.Uranium;
            return true;
		}), Irradiation.Id, Irradiation.Id);

        QApi.BindGlyphCylce(Irradiation, new(PartCycleDelegate.CycleExecutionType.Normal, static (sim, part, simState, recipe, isCycleStart) => {
            // look for 3 unheld QSs and free gold
            // if all the atoms exist...
            if (recipe.Predicate.InvokeAndClear(sim, part)) {
				AtomReference gold = sim.RecipeInputs[new(0, 0)] as AtomReference;
                AtomReference qs1 = sim.RecipeInputs[new(-1, 1)] as AtomReference;
                AtomReference qs2 = sim.RecipeInputs[new(1, 0)] as AtomReference;
                AtomReference qs3 = sim.RecipeInputs[new(0, -1)] as AtomReference;
                // transmute the gold and destroy the quicksilver
                gold.molecule.ReplaceAtom(sim.RecipeOutputs[new(0, 0)] as AtomType, gold.pos);
                qs1.molecule.RemoveAtom(qs1.pos);
                qs2.molecule.RemoveAtom(qs2.pos);
                qs3.molecule.RemoveAtom(qs3.pos);
                // show the removal effects for qs
                sim.solutionEditor.consumptionEffects.Add(new ConsumptionEffect(sim.solutionEditor, new AtomReference[] { qs1, qs2, qs3 }));
                // upgrade effect for gold -> uranium
                gold.atom.transmutationEffect = new TransmutationEffect(sim.solutionEditor, (TransmutationEffectRenderMode)1, gold.atomType, Assets.textures.atoms.projection_effect, 30f);
                // glowy effect on central hex
                HexIndex pos = part.GetHexPos();
                Vector2 posAsVec = HexGrid.standardGrid.ToPixelCoords(pos);
                Texture[] glowFrames = Assets.textures.parts.projection_glyph_flash;
                GlyphEffect glowEffect = new(sim.solutionEditor, (EffectTimescaleType)1, posAsVec, glowFrames, 30f, Vector2.Zero, 0);
                sim.solutionEditor.glyphEffects.Add(glowEffect);
                Assets.sounds.glyph_projection.method_28(sim.solutionEditor.method_506());
                return true;
            }
            return false;
        }));

        UnstableElements.Instance.AddRecipe(new(static (sim, part) => {
			if (!sim.GetAtomReference(part, new(0, 0), false, out AtomReference uranium) ||
				!Atoms.IsUraniumState(uranium.atomType)) return false;

			sim.RecipeInputs[new(0, 0)] = uranium;
            sim.RecipeOutputs[new(0, 0)] = AtomTypes.lead;
            return true;
		}), Volatility.Id, Volatility.Id);
		
        QApi.BindGlyphCylce(Volatility, new(PartCycleDelegate.CycleExecutionType.Normal, (sim, part, simState, recipe, isCycleStart) => {
            if (recipe.Predicate.InvokeAndClear(sim, part)){
				AtomReference toDecay = sim.RecipeInputs[new(0, 0)] as AtomReference;
                Atoms.DoDecay(toDecay.molecule, toDecay.atom, toDecay.pos, sim.solutionEditor, sim.RecipeOutputs[new(0, 0)] as AtomType);
                return true;
            }
            return false;
        }));

        UnstableElements.Instance.AddRecipe(new(), Tranquility.Id, Tranquility.Id);

        QApi.BindGlyphCylce(Tranquility, new(PartCycleDelegate.CycleExecutionType.Normal, (sim, part, simState, recipe, isCycleStart) => {
            bool isPowered =
                        sim.FindAtomRelative(part, new(0, 1)).GetOrDefault(out AtomReference qs)
                        && qs.atomType == AtomTypes.quicksilver; // is QS, //  TODO maybe add a tag here
            new DynamicData(part).Set(TranquilityPowerId, isPowered);
            if (isPowered) {
                foreach (var offset in TranquilityOffsets) {
                    var adjusted = part.InFrontBy(offset);
                    TranquilityHexes.Add(adjusted);
                }
            }
            return true;
        }));

        UnstableElements.Instance.AddRecipe(new(static (sim, part) => {
			if (!sim.GetAtomReference(part, new(0, 0), false, out AtomReference quint) ||
				quint.inMultiAtomMolecule || quint.isHeldByArm ||
                quint.atomType != AtomTypes.quintessence ||
				sim.HasAtomAt(part, new(0, 1), true) ||
                sim.HasAtomAt(part, new(1, 1), true) ||
                sim.HasAtomAt(part, new(0, -1), true) ||
                sim.HasAtomAt(part, new(-1, -1), true)) return false;

			sim.RecipeInputs[new(0, 0)] = quint;
			Molecule stabilizedAether = new();
            stabilizedAether.AddAtom(new Atom(Atoms.Aether), part.InFrontBy(new HexIndex(1, 1)));
            stabilizedAether.AddAtom(new Atom(AtomTypes.salt), part.InFrontBy(new HexIndex(0, 1)));
			stabilizedAether.AddBond(BondTypeEnum.Standard, part.InFrontBy(new HexIndex(0, 1)), part.InFrontBy(new HexIndex(1, 1)), MaybeHelper.empty);
            sim.RecipeOutputs[new(1, 1)] = stabilizedAether;
            Molecule stbAetherRot = new();
            stbAetherRot.AddAtom(new Atom(Atoms.Aether), part.InFrontBy(new HexIndex(-1, -1)));
            stbAetherRot.AddAtom(new Atom(AtomTypes.salt), part.InFrontBy(new HexIndex(0, -1)));
            stbAetherRot.AddBond(BondTypeEnum.Standard, part.InFrontBy(new HexIndex(0, -1)), part.InFrontBy(new HexIndex(-1, -1)), MaybeHelper.empty);
            sim.RecipeOutputs[new(-1, -1)] = stbAetherRot;

            return true;
		}), Sublimation.Id, Sublimation.Id);

        QApi.BindGlyphCylce(Sublimation, new(PartCycleDelegate.CycleExecutionType.Normal, (sim, part, simState, recipe, isCycleStart) => {
            // if we're in the accepting phase...
            if (!simState.isProcessing) {
                // if we have an unheld & unbonded quintessence at the centre...
				if (isCycleStart && recipe.Predicate.InvokeAndClear(sim, part)) {
					AtomReference toSplit = sim.RecipeInputs[new(0 ,0)] as AtomReference;
                    // destroy the quintessence
                    toSplit.molecule.RemoveAtom(toSplit.pos);
                    // set this part to be inactive the rest of the cycle
                    simState.isProcessing = true;
                    // play the production sound
                    Assets.sounds.glyph_dispersion.method_28(sim.solutionEditor.method_506());
                    // mark output positions as collidable
                    HexIndex[] outputs = {
                                    new(0, 1),
                                    new(1, 1),
                                    new(0, -1),
                                    new(-1, -1)
                                };
                    List<Sim.Collider> collisions = sim.additionalCollisions;
                    foreach (var hex in outputs) {
                        Vector2 vector2 = HexGrid.standardGrid.ToPixelCoords(part.InFrontBy(hex), Vector2.Zero);
                        Sim.Collider collision = new() {
                            type = 0,
                            center = vector2,
                            radius =  15
                        };
                        collisions.Add(collision);
                    }
					Molecule[] molecules = new Molecule[] {
						sim.RecipeOutputs[new(1 ,1)] as Molecule,
						sim.RecipeOutputs[new(-1, -1)] as Molecule
					};
					simState.processingAtoms = new AtomType[] { toSplit.atomType };
                    new DynamicData(simState).Set(SublimationMoleculeStorage, molecules);
                    return true;
                }
            } else {
				new DynamicData(simState).TryGet(SublimationMoleculeStorage, out Molecule[] molecules);
                // otherwise, we're in the producing phase
                sim.molecules.Add(molecules[0]);
                sim.molecules.Add(molecules[1]);
                // state is reset automatically
                return true;
            }
            return false;
        }));




        UnstableElements.Instance.AddRecipe(new(static (sim, part) => {
            if (!sim.GetAtomReference(part, new(0, 0), false, out AtomReference quint) ||
                quint.inMultiAtomMolecule || quint.isHeldByArm ||
                !Atoms.IsUraniumState(quint.atomType) ||
                sim.HasAtomAt(part, new(0, 1), true) ||
                sim.HasAtomAt(part, new(1, 1), true) ||
                sim.HasAtomAt(part, new(0, -1), true) ||
                sim.HasAtomAt(part, new(-1, -1), true)) return false;

            sim.RecipeInputs[new(0, 0)] = quint;
            Molecule stabilizedAether = new();
            stabilizedAether.AddAtom(new Atom(AtomTypes.lead), part.InFrontBy(new HexIndex(1, 1)));
            stabilizedAether.AddAtom(new Atom(AtomTypes.quicksilver), part.InFrontBy(new HexIndex(0, 1)));
            stabilizedAether.AddBond(BondTypeEnum.Standard, part.InFrontBy(new HexIndex(0, 1)), part.InFrontBy(new HexIndex(1, 1)), MaybeHelper.empty);
            sim.RecipeOutputs[new(1, 1)] = stabilizedAether;
            Molecule stbAetherRot = new();
            stbAetherRot.AddAtom(new Atom(AtomTypes.gold), part.InFrontBy(new HexIndex(-1, -1)));
            stbAetherRot.AddAtom(new Atom(AtomTypes.quicksilver), part.InFrontBy(new HexIndex(0, -1)));
            stbAetherRot.AddBond(BondTypeEnum.Standard, part.InFrontBy(new HexIndex(0, -1)), part.InFrontBy(new HexIndex(-1, -1)), MaybeHelper.empty);
            sim.RecipeOutputs[new(-1, -1)] = stbAetherRot;

            return true;
        }), Sublimation.Id + "_other", Sublimation.Id);
    }

	private static void DrawForPartWithTint(PartRenderer renderer, Texture tex, Vector2 offset, Vector2 size, float rotation, Color c){
		Matrix4 tf = Matrix4.GetTranslation((renderer.partPos + offset).ToVector3(0)) * Matrix4.RotXY(renderer.partRotation + rotation) * Matrix4.GetTranslation(-size.ToVector3(0)) * Matrix4.GetScale(tex.size.ToVector3(0));
        TextureRenderer.Render(tex, c, tf);
	}

	private static Vector2 RelativeToPart(IntermediatePartState partRenderInfo, HexIndex pos) => (partRenderInfo.pos + HexGrid.standardGrid.ToPixelCoords(-pos));//.Rotated(partRenderInfo.rotation);
}
