using MonoMod.Utils;
using Quintessential;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace UnstableElements;

internal static class SublimationGlyph {
    public static Texture SublimationBelowIris = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/sublimation_below_iris");
    public static Texture SublimationAboveIris = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/sublimation_above_iris");
    public static Texture SublimationQuintessenceSymbol = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/quintessence_symbol");
    public static Texture[] SublimationAetherIris = new Texture[16];
    public static Texture[] SublimationSaltIris = new Texture[16];
    private static readonly string SublimationMoleculeStorage = "UnstableElements:sublimation_molecules";

    public static void Render(Part part, Vector2 pos, SolutionEditorBase editor, PartRenderer renderer) {
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

        if (0 < progress && progress < 0.5 && molecules != null) { // render under irises
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
    }

    public static bool OnCycle(Sim sim, Part part, PartSimState simState, GlyphRecipe recipe, bool isCycleStart) {
        // if we're in the accepting phase...
        if (!simState.isProcessing) {
            // if we have an unheld & unbonded quintessence at the centre...
            if (isCycleStart && recipe.Predicate.InvokeAndClear(sim, part)) {
                AtomReference toSplit = sim.RecipeInputs[new(0, 0)] as AtomReference;
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
                        radius = 15
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
    }

    public static bool BasicSublimation(Sim sim, Part part) {
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
    }

    private static Vector2 RelativeToPart(IntermediatePartState partRenderInfo, HexIndex pos) => (partRenderInfo.pos + HexGrid.standardGrid.ToPixelCoords(-pos));
}
