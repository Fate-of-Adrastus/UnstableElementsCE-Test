using MonoMod.Utils;
using Quintessential;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Quintessential.PartCycleDelegate;

namespace UnstableElements;

internal static class Parts {
	
	public static PartType Irradiation, Volatility, Tranquility, Sublimation;

	public static void AddPartTypes() {
		for (int i = 0; i < 16; i++) {
            SublimationGlyph.SublimationAetherIris[i] = AssetLoaderHelper.LoadTexture($"textures/parts/leppa/UnstableElements/iris_full_aether.array/iris_full_00{i + 1:D2}");
            SublimationGlyph.SublimationSaltIris[i] = AssetLoaderHelper.LoadTexture($"textures/parts/leppa/UnstableElements/iris_full_salt.array/iris_full_00{i + 1:D2}");
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

		UnstableElements.Instance.AddPartType(Irradiation, "irradiation", IrradiationGlyph.Render);
		UnstableElements.Instance.AddPartType(Volatility, "volatility", VolatilityGlyph.Render);
		UnstableElements.Instance.AddPartType(Tranquility, "tranquility", TranquilityGlyph.Render);
		UnstableElements.Instance.AddPartType(Sublimation, "sublimation", SublimationGlyph.Render);

		QApi.AddPartTypeToPanel(Irradiation, PartTypes.triplexBonder);
		QApi.AddPartTypeToPanel(Volatility, PartTypes.triplexBonder);
		QApi.AddPartTypeToPanel(Tranquility, PartTypes.triplexBonder);
		QApi.AddPartTypeToPanel(Sublimation, PartTypes.triplexBonder);

		UnstableElements.Instance.AddPuzzlePermission("irradiation");
		UnstableElements.Instance.AddPuzzlePermission("volatility");
		UnstableElements.Instance.AddPuzzlePermission("tranquility");
		UnstableElements.Instance.AddPuzzlePermission("sublimation");

        QApi.BindGlyphCylce(Irradiation, new(PartCycleExecutionType.Normal, IrradiationGlyph.OnCycle));
        QApi.BindGlyphCylce(Volatility, new(PartCycleExecutionType.Normal, VolatilityGlyph.OnCycle));
        QApi.BindGlyphCylce(Tranquility, new(PartCycleExecutionType.Normal, TranquilityGlyph.OnCycle));
        QApi.BindGlyphCylce(Sublimation, new(PartCycleExecutionType.Normal, SublimationGlyph.OnCycle));

        UnstableElements.Instance.AddRecipe(new(IrradiationGlyph.BasicIrradiation), Irradiation.Id, Irradiation.Id);
        UnstableElements.Instance.AddRecipe(new(VolatilityGlyph.BasicVolatility), Volatility.Id, Volatility.Id);
        UnstableElements.Instance.AddRecipe(new(), Tranquility.Id, Tranquility.Id);
        UnstableElements.Instance.AddRecipe(new(SublimationGlyph.BasicSublimation), Sublimation.Id, Sublimation.Id);

		QApi.AddCycleEvent(new(
			CycleEvent.CycleEventExecutionType.BeforeEarlyGlyphs | CycleEvent.CycleEventExecutionType.BeforeLateGlyphs,
			TranquilityGlyph.ClearTranquility
		));



		// Dumb test recipe with uranium
		//UnstableElements.Instance.AddRecipe(new(static (sim, part) => {
		//	if (!sim.GetAtomReference(part, new(0, 0), false, out AtomReference quint) ||
		//		quint.inMultiAtomMolecule || quint.isHeldByArm ||
		//		!Atoms.IsUraniumState(quint.atomType) ||
		//		sim.HasAtomAt(part, new(0, 1), true) ||
		//		sim.HasAtomAt(part, new(1, 1), true) ||
		//		sim.HasAtomAt(part, new(0, -1), true) ||
		//		sim.HasAtomAt(part, new(-1, -1), true)) return false;

		//	sim.RecipeInputs[new(0, 0)] = quint;
		//	Molecule stabilizedAether = new();
		//	stabilizedAether.AddAtom(new Atom(AtomTypes.lead), part.InFrontBy(new HexIndex(1, 1)));
		//	stabilizedAether.AddAtom(new Atom(AtomTypes.quicksilver), part.InFrontBy(new HexIndex(0, 1)));
		//	stabilizedAether.AddBond(BondTypeEnum.Standard, part.InFrontBy(new HexIndex(0, 1)), part.InFrontBy(new HexIndex(1, 1)), MaybeHelper.empty);
		//	sim.RecipeOutputs[new(1, 1)] = stabilizedAether;
		//	Molecule stbAetherRot = new();
		//	stbAetherRot.AddAtom(new Atom(AtomTypes.gold), part.InFrontBy(new HexIndex(-1, -1)));
		//	stbAetherRot.AddAtom(new Atom(AtomTypes.quicksilver), part.InFrontBy(new HexIndex(0, -1)));
		//	stbAetherRot.AddBond(BondTypeEnum.Standard, part.InFrontBy(new HexIndex(0, -1)), part.InFrontBy(new HexIndex(-1, -1)), MaybeHelper.empty);
		//	sim.RecipeOutputs[new(-1, -1)] = stbAetherRot;

		//	return true;
		//}), Sublimation.Id + "_other", Sublimation.Id);
	}
}
