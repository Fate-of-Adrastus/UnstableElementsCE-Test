using Quintessential;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UnstableElements;

internal static class IrradiationGlyph {
    public static Texture IrradiationBase = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/irradiation_base");
    public static Texture IrradiationGoldSymbol = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/gold_symbol");
    public static Texture IrradiationMetalBowl = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/irradiation_bowl");

    public static void Render(Part part, Vector2 pos, SolutionEditorBase editor, PartRenderer renderer) {
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
    }

    public static bool OnCycle(Sim sim, Part part, PartSimState simState, GlyphRecipe recipe, bool isCycleStart) {
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
    }

    public static bool BasicIrradiation(Sim sim, Part part) {
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
    }
}
