using Quintessential;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnstableElements;

internal static class VolatilityGlyph {
    public static Texture VolatilitySymbol = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/volatility_symbol");
    public static Texture VolatilityBowl = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/volatility_bowl");

    public static void Render(Part part, Vector2 pos, SolutionEditorBase editor, PartRenderer renderer) {
        Texture calcinatorBase = Assets.textures.parts.calcinator_base;
        Vector2 centre = (calcinatorBase.size.ToVector2() / 2).Rounded() + new Vector2(0, 1);
        renderer.RenderBase(calcinatorBase, centre);
        renderer.RenderShadowCircle(Assets.textures.parts.animismus.ring_shadow, new HexIndex(0, 0), 3);
        renderer.RenderRotating(VolatilityBowl, new HexIndex(0, 0), Vector2.Zero);
        renderer.RenderBase(VolatilitySymbol, centre);
    }

    public static bool OnCycle(Sim sim, Part part, PartSimState simState, GlyphRecipe recipe, bool isCycleStart) {
        if (recipe.Predicate.InvokeAndClear(sim, part)) {
            AtomReference toDecay = sim.RecipeInputs[new(0, 0)] as AtomReference;
            Atoms.DoDecay(toDecay.molecule, toDecay.atom, toDecay.pos, sim.solutionEditor, sim.RecipeOutputs[new(0, 0)] as AtomType);
            return true;
        }
        return false;
    }

    public static bool BasicVolatility(Sim sim, Part part) {
        if (!sim.GetAtomReference(part, new(0, 0), false, out AtomReference uranium) ||
            !Atoms.IsUraniumState(uranium.atomType)) return false;

        sim.RecipeInputs[new(0, 0)] = uranium;
        sim.RecipeOutputs[new(0, 0)] = AtomTypes.lead;
        return true;
    }
}
