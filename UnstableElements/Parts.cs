using System.Collections.Generic;
using MonoMod.Utils;
using Quintessential;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System;
using System.Linq;

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

	private static readonly HashSet<HexIndex> TranquilityOffsets = new(){
		new(1, -1),
		new(1, -2), new(2, -2),
		new(1, -3), new(2, -3), new(3, -3),
		new(1, -4), new(2, -4), new(3, -4), new(4, -4)
	};

	public static void AddPartTypes(){
		for(int i = 0; i < 16; i++){
			SublimationAetherIris[i] = AssetLoaderHelper.LoadTexture($"textures/parts/leppa/UnstableElements/iris_full_aether.array/iris_full_00{i + 1:D2}");
			SublimationSaltIris[i] = AssetLoaderHelper.LoadTexture($"textures/parts/leppa/UnstableElements/iris_full_salt.array/iris_full_00{i + 1:D2}");
		}

		Irradiation = new(){
			id = "unstable-elements-irradiation", // ID
			name = Translations.Translate("Glyph of Irradiation"), // Name
			description = Translations.Translate("The glyph of irradiation projects an atom of gold into an unstable atom of uranium."), // Description
			cost = 25, // Cost
			isFullHexCover = true, // Is a glyph (?)
			glowTexture = Assets.textures.field_97.field_384, // Shadow/glow
			strokeTexture = Assets.textures.field_97.field_385, // Stroke/outline
			baseTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/irradiation"), // Panel icon
			hoverTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/irradiation_hovered"), // Hovered panel icon
			glyphHexes = new HexIndex[]{
				new(0, 0),
				new(-1, 1),
				new(1, 0),
				new(0, -1)
			}, // Spaces used
			permissionCategory = PuzzlePermissions.None,
			CustomPermissionCheck = perms => perms.Contains("UnstableElements:irradiation")
		};

		Volatility = new(){
			id = "unstable-elements-volatility", // ID
			name = Translations.Translate("Glyph of Volatility"), // Name
			description = Translations.Translate("The glyph of volatility causes an atom of uranium to instantly decay, regardless of its heat."), // Description
			cost = 10, // Cost
			isFullHexCover = true, // Is a glyph (?)
			glowTexture = Assets.textures.field_97.field_382, // Shadow/glow
			strokeTexture = Assets.textures.field_97.field_383, // Stroke/outline
			baseTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/volatility"), // Panel icon
			hoverTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/volatility_hovered"), // Hovered panel icon
			glyphHexes = new HexIndex[]{
				new(0, 0)
			}, // Spaces used
			permissionCategory = PuzzlePermissions.None,
			CustomPermissionCheck = perms => perms.Contains("UnstableElements:volatility")
		};

		Tranquility = new(){
			id = "unstable-elements-tranquility", // ID
			name = Translations.Translate("Glyph of Tranquility"), // Name
			description = Translations.Translate("The glyph of tranquility projects a field that stabilizes uranium and aether atoms, preventing their decays."), // Description
			cost = 40, // Cost
			isFullHexCover = true, // Is a glyph (?)
			glowTexture = Assets.textures.field_97.field_386, // Shadow/glow
			strokeTexture = Assets.textures.field_97.field_387, // Stroke/outline
			baseTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/tranquility"), // Panel icon
			hoverTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/tranquility_hovered"), // Hovered panel icon
			glyphHexes = new HexIndex[]{
				new(0, 0),
				new(1, 0),
				new(0, 1)
			}, // Spaces used
			permissionCategory = PuzzlePermissions.None,
			CustomPermissionCheck = perms => perms.Contains("UnstableElements:tranquility")
		};

		Sublimation = new(){
			id = "unstable-elements-sublimation", // ID
			name = Translations.Translate("Glyph of Sublimation"), // Name
			description = Translations.Translate("The glyph of sublimation splits an atom of quintessence into two molecules of stabilized aether."), // Description
			cost = 10, // Cost
			isFullHexCover = true, // Is a glyph (?)
			glowTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/sublimation_glow"), // Shadow/glow
			strokeTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/sublimation_stroke"), // Stroke/outline
			baseTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/sublimation"), // Panel icon
			hoverTexture = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/sublimation_hovered"), // Hovered panel icon
			glyphHexes = new HexIndex[]{
				new(0, 0),
				new(0, 1),
				new(1, 1),
				new(0, -1),
				new(-1, -1)
			}, // Spaces used
			permissionCategory = PuzzlePermissions.None,
			CustomPermissionCheck = perms => perms.Contains("UnstableElements:sublimation")
		};

		QApi.AddPartType(Irradiation, (part, pos, editor, renderer) => {
			Vector2 vector2 = new(83f, 119f);
			renderer.method_523(IrradiationBase, new Vector2(0.0f, -1f), vector2, 0.0f);
			foreach(HexIndex idx in part.GetType().glyphHexes){
				if(idx is { Q: 0, R: 0 }){
					renderer.method_530(Assets.textures.field_90.field_164 /*bonder_shadow*/, idx, 0);
					renderer.method_528(IrradiationMetalBowl, idx, Vector2.Zero);
					renderer.method_529(IrradiationGoldSymbol, idx, Vector2.Zero);
				}
				else{
					renderer.method_530(Assets.textures.field_90.field_164 /*bonder_shadow*/, idx, 0);
					renderer.method_530(Assets.textures.field_90.field_255.field_293 /*quicksilver_input*/, idx, 0);
					// should be 272?
					renderer.method_529(Assets.textures.field_90.field_255.field_294 /*quicksilver_symbol*/, idx, Vector2.Zero);
				}
			}

			for(var i = 0; i < part.GetType().glyphHexes.Length; i++){
				HexIndex hexIndex = part.GetType().glyphHexes[i];
				if(hexIndex != new HexIndex(0, 0)){
					int index = i - 1;
					float num = new HexRotation(index * 2).ToRadians();
					renderer.method_522(Assets.textures.field_90.field_255.field_289 /*bond*/, new Vector2(-30f, 12f), num);
				}
			}
		});
		QApi.AddPartType(Volatility, (part, pos, editor, renderer) => {
			Texture calcinatorBase = Assets.textures.field_90.field_169;
			Vector2 centre = (calcinatorBase.size.ToVector2() / 2).Rounded() + new Vector2(0, 1);
			renderer.method_521(calcinatorBase, centre);
			renderer.method_530(Assets.textures.field_90.field_228.field_273 /* ring_shadow */, new HexIndex(0, 0), 3);
			renderer.method_528(VolatilityBowl, new HexIndex(0, 0), Vector2.Zero);
			renderer.method_521(VolatilitySymbol, centre);
		});
		QApi.AddPartType(Tranquility, (part, pos, editor, renderer) => {
			Vector2 vector2 = new(42, 48);
			renderer.method_523(TranquilityBase, new Vector2(-1, -1), vector2, 0);
			HexIndex qsSite = new(0, 1);
			renderer.method_530(Assets.textures.field_90.field_164 /*bonder_shadow*/, qsSite, 0);
			renderer.method_528(TranquilityMetalBowl, qsSite, Vector2.Zero);
			renderer.method_529(TranquilityQuicksilverSymbol, qsSite, Vector2.Zero);

			double time = Math.Sin(new DeltaTime(Time.Now().Ticks).InSeconds());
			float pulse = (float)(time / 3 + .66);
			if(editor.GetSimPlayState() != SimPlayState.Stopped && new DynamicData(part).TryGet(TranquilityPowerId, out bool? power) && power == true){
				Color tint = Color.White;
				tint.A *= pulse;
				DrawForPartWithTint(renderer, TranquilityProjectors, new Vector2(-1, -1), vector2, 0, tint);
			}
		});
		QApi.AddPartType(Sublimation, (part, pos, editor, renderer) => {
			PartSimState myState = editor.GetSimulation().GetSimState(part);
			Vector2 vector2 = new(330 / 2, 238 / 2);
			var renderInfo = editor.GetIntermState(part, pos);

			// base
			renderer.method_523(SublimationBelowIris, new Vector2(-1, -1), vector2, 0);

			// centre hex
			renderer.method_529(SublimationQuintessenceSymbol, new(0, 0), new(3, 3));
			if(myState.isProcessing) // disappearing quintessence for active glyph
				Editor.RenderMolecule(Molecule.GetSinglet(AtomTypes.quintessence), RelativeToGlobal(renderInfo, new(0, 0)), new(0, 0), 0.0f, 1f, 1f - editor.GetCycleTime(), 1f, false, null);

			Molecule stabilizedAether = new();
			stabilizedAether.AddAtom(new Atom(Atoms.Aether), new(1, 1));
			stabilizedAether.AddAtom(new Atom(AtomTypes.salt), new(0, 1));
			stabilizedAether.AddBond(BondTypeEnum.Standard, new(0, 1), new(1, 1), MaybeHelper.empty);
			Molecule stbAetherRot = stabilizedAether.CloneAndRotate(HexRotation.R180);

			// irises
			var animIdx = 15;
			float progress = 0;
			if(myState.isProcessing){
				animIdx = Utils.Clamp((int)((double)Utils.InterpolateLinear(1f, -1f, editor.GetCycleTime()) * 16), 0, 15);
				progress = editor.GetCycleTime();
			}

			if(progress < 0.5){ // render under irises
				Editor.RenderMolecule(stabilizedAether, RelativeToGlobal(renderInfo, new(0, 0)), new(0, 0), renderInfo.rotation, 1f, progress, 1f, false, null);
				Editor.RenderMolecule(stbAetherRot, RelativeToGlobal(renderInfo, new(0, 0)), new(0, 0), renderInfo.rotation, 1f, progress, 1f, false, null);
			}

			renderer.method_529(SublimationSaltIris[animIdx], new(0, 1), new(2, 0));
			renderer.method_529(SublimationSaltIris[animIdx], new(0, -1), new(2, 0));
			renderer.method_529(SublimationAetherIris[animIdx], new(1, 1), new(2, 0));
			renderer.method_529(SublimationAetherIris[animIdx], new(-1, -1), new(2, 0));
			if(progress > 0.5){ // render over irises
				Editor.RenderMolecule(stabilizedAether, RelativeToGlobal(renderInfo, new(0, 0)), new(0, 0), renderInfo.rotation, 1f, progress, 1f, false, null);
				Editor.RenderMolecule(stbAetherRot, RelativeToGlobal(renderInfo, new(0, 0)), new(0, 0), renderInfo.rotation, 1f, progress, 1f, false, null);
			}

			// top
			renderer.method_523(SublimationAboveIris, new Vector2(-1, -1), vector2, 0);
		});

		QApi.AddPartTypeToPanel(Irradiation, PartTypes.triplexBonder);
		QApi.AddPartTypeToPanel(Volatility, PartTypes.triplexBonder);
		QApi.AddPartTypeToPanel(Tranquility, PartTypes.triplexBonder);
		QApi.AddPartTypeToPanel(Sublimation, PartTypes.triplexBonder);
		
		QApi.AddPuzzlePermission("UnstableElements:irradiation", "Glyph of Irradiation", "Unstable Elements");
		QApi.AddPuzzlePermission("UnstableElements:volatility", "Glyph of Volatility", "Unstable Elements");
		QApi.AddPuzzlePermission("UnstableElements:tranquility", "Glyph of Tranquility", "Unstable Elements");
		QApi.AddPuzzlePermission("UnstableElements:sublimation", "Glyph of Sublimation", "Unstable Elements");

		QApi.RunAfterCycle((sim, _) => {
			// first thing
			TranquilityHexes.Clear();
			
			OtherStableHexes.Clear();
			foreach(var cb in OtherStableHexesCallbacks)
				OtherStableHexes.UnionWith(cb(sim));
		});

		QApi.RunAfterCycle((sim, first) => {
			var seb = sim.solutionEditor;
			List<Part> allParts = seb.GetSolution().parts;
			var simStates = sim.simulationDict;

			foreach(var part in allParts){
				var type = part.GetType();
				// look for 3 unheld QSs and free gold
				if(type == Irradiation){
					// if all the atoms exist...
					if(sim.FindAtomRelative(part, new HexIndex(0, 0)).GetOrDefault(out AtomReference gold)
					   && sim.FindAtomRelative(part, new(-1, 1)).GetOrDefault(out AtomReference qs1)
					   && sim.FindAtomRelative(part, new(1, 0)).GetOrDefault(out AtomReference qs2)
					   && sim.FindAtomRelative(part, new(0, -1)).GetOrDefault(out AtomReference qs3)){
						// and are the right types...
						if(gold.atomType == AtomTypes.gold
						   && qs1.atomType == AtomTypes.quicksilver
						   && qs2.atomType == AtomTypes.quicksilver
						   && qs3.atomType == AtomTypes.quicksilver){
							// and the quicksilver is not being consumed or held...
							if(!qs1.doesMoleculeExist && !qs1.isHeldByArm
							                   && !qs2.doesMoleculeExist && !qs2.isHeldByArm
							                   && !qs3.doesMoleculeExist && !qs3.isHeldByArm){
								// transmute the gold and destroy the quicksilver
								gold.molecule.ReplaceAtom(Atoms.Uranium, gold.pos);
								qs1.molecule.RemoveAtom(qs1.pos);
								qs2.molecule.RemoveAtom(qs2.pos);
								qs3.molecule.RemoveAtom(qs3.pos);
								// show the removal effects for qs
								seb.consumptionEffects.Add(new ConsumptionEffect(seb, qs1.molecule));
								seb.consumptionEffects.Add(new ConsumptionEffect(seb, qs2.molecule));
								seb.consumptionEffects.Add(new ConsumptionEffect(seb, qs3.molecule));
								// upgrade effect for gold -> uranium
								gold.atom.transmutationEffect = new TransmutationEffect(seb, (TransmutationEffectRenderMode)1, gold.atomType, Assets.textures.field_81.field_614, 30f);
								// glowy effect on central hex
								HexIndex pos = part.GetHexPos();
								Vector2 posAsVec = HexGrid.standardGrid.ToWorldCoords(pos);
								Texture[] glowFrames = Assets.textures.field_90.field_256;
								GlyphEffect glowEffect = new(seb, (EffectTimescaleType)1, posAsVec, glowFrames, 30f, Vector2.Zero, 0);
                                seb.glyphEffects.Add(glowEffect);
								Assets.sounds.field_1844.method_28(seb.method_506());
							}
						}
					}
				}
				else if(type == Volatility){
					if(sim.FindAtomRelative(part, new(0, 0)).GetOrDefault(out AtomReference uranium))
						if(Atoms.IsUraniumState(uranium.atomType))
							Atoms.DoUraniumDecay(uranium.molecule, uranium.atom, uranium.pos, seb);
				}
				else if(type == Tranquility){
					bool isPowered =
						sim.FindAtomRelative(part, new(0, 1)).GetOrDefault(out AtomReference qs)
						&& qs.atomType == AtomTypes.quicksilver; // is QS
					new DynamicData(part).Set(TranquilityPowerId, isPowered);
					if(isPowered){
						foreach(var offset in TranquilityOffsets){
							var adjusted = part.InFrontBy(offset);
							TranquilityHexes.Add(adjusted);
						}
					}
				}
				else if(type == Sublimation){
					var mySimState = simStates[part];
					// if we're in the accepting phase...
					if(!mySimState.isProcessing){
						// if we have an unheld & unbonded quintessence at the centre...
						if(first && sim.FindAtomRelative(part, new(0, 0)).GetOrDefault(out AtomReference quint)
						         && quint.atomType == AtomTypes.quintessence
                                 && !quint.doesMoleculeExist && !quint.isHeldByArm){
							// and no atoms are blocking our outputs...
							if(!sim.FindAtomRelative(part, new(0, 1)).HasValue()
							   && !sim.FindAtomRelative(part, new(1, 1)).HasValue()
							   && !sim.FindAtomRelative(part, new(0, -1)).HasValue()
							   && !sim.FindAtomRelative(part, new(-1, -1)).HasValue()){
								// destroy the quintessence
								quint.molecule.RemoveAtom(quint.pos);
								// set this part to be inactive the rest of the cycle
								mySimState.isProcessing = true;
								// play the production sound
								Assets.sounds.field_1841.method_28(seb.method_506());
								// mark output positions as collidable
								HexIndex[] outputs = {
									new(0, 1),
									new(1, 1),
									new(0, -1),
									new(-1, -1)
								};
								List<Sim.Collider> collisions = sim.additionalCollisions;
								foreach(var hex in outputs){
									Vector2 vector2 = HexGrid.standardGrid.ToWorldCoords(part.InFrontBy(hex), Vector2.Zero);
									Sim.Collider collision = new(){
                                        type = 0,
										center = vector2,
										radius = 15
									};
									collisions.Add(collision);
								}
							}
						}
					}
					else{
						// otherwise, we're in the producing phase
						Molecule stabilizedAether = new();
						stabilizedAether.AddAtom(new Atom(Atoms.Aether), part.InFrontBy(new HexIndex(1, 1)));
						stabilizedAether.AddAtom(new Atom(AtomTypes.salt), part.InFrontBy(new HexIndex(0, 1)));
						stabilizedAether.AddBond(BondTypeEnum.Standard, part.InFrontBy(new HexIndex(0, 1)), part.InFrontBy(new HexIndex(1, 1)), MaybeHelper.empty);
						Molecule stbAetherRot = new();
						stbAetherRot.AddAtom(new Atom(Atoms.Aether), part.InFrontBy(new HexIndex(-1, -1)));
						stbAetherRot.AddAtom(new Atom(AtomTypes.salt), part.InFrontBy(new HexIndex(0, -1)));
						stbAetherRot.AddBond(BondTypeEnum.Standard, part.InFrontBy(new HexIndex(0, -1)), part.InFrontBy(new HexIndex(-1, -1)), MaybeHelper.empty);

						List<Molecule> molecules = sim.molecules;
						molecules.Add(stabilizedAether);
						molecules.Add(stbAetherRot);

						// state is reset automatically
					}
				}
			}
		});

		On.SolutionEditorBase.method_1984 += DrawTranquilityField;
	}

	public static void Unload(){
		On.SolutionEditorBase.method_1984 -= DrawTranquilityField;
	}

	private static void DrawForPartWithTint(PartRenderer renderer, Texture tex, Vector2 offset, Vector2 size, float rotation, Color c){
		Matrix4 tf = Matrix4.GetTranslation((renderer.partPos + offset).ToVector3(0)) * Matrix4.RotXY(renderer.partRotation + rotation) * Matrix4.GetTranslation(-size.ToVector3(0)) * Matrix4.GetScale(tex.size.ToVector3(0));
        TextureRenderer.Render(tex, c, tf);
	}

	private static void DrawTranquilityField(On.SolutionEditorBase.orig_method_1984 orig, SolutionEditorBase self, Vector2 param_5533, Bounds2 param_5534, Bounds2 param_5535, bool param_5536, Maybe<List<Molecule>> param_5537, bool param_5538){
		orig(self, param_5533, param_5534, param_5535, param_5536, param_5537, param_5538);

		if(self.GetSimPlayState() != SimPlayState.Stopped){
			double time = Math.Sin(new DeltaTime(Time.Now().Ticks).InSeconds());
			float pulse = (float)(time / 4 + .75) / 2.4f;

			if(self is SimpleSolutionEditor)
				pulse = 0.25f; // constant brightness in GIFs

			Color tint = TranquilityZoneColor;
			HexGrid conv = HexGrid.standardGrid;
			tint.A *= pulse;
			foreach(var hex in TranquilityHexes){
				Vector2 hexAsVec = conv.ToWorldCoords(hex) + param_5533 - new Vector2(2, 8);
				Matrix4 tf = Matrix4.GetTranslation(hexAsVec.ToVector3(0)) * Matrix4.RotXY(0) * Matrix4.GetTranslation(new Vector3(-40, -40, 0)) * Matrix4.GetScale(TranquilityZoneHex.size.ToVector3(0));
				TextureRenderer.Render(TranquilityZoneHex, tint, tf);
			}
		}
	}

	private static Vector2 RelativeToGlobal(IntermediatePartState partRenderInfo, HexIndex pos) => partRenderInfo.pos + HexGrid.standardGrid.ToWorldCoords(pos).Rotated(partRenderInfo.rotation);
}
