using MonoMod.Utils;
using Quintessential;
using System;
using System.Collections.Generic;
using System.Text;
using static Quintessential.CycleEvent;

namespace UnstableElements;

internal static class TranquilityGlyph {
    public static Texture TranquilityBase = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/tranquility_base");
    public static Texture TranquilityQuicksilverSymbol = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/quicksilver_symbol");
    public static Texture TranquilityMetalBowl = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/tranquility_bowl");
    public static Texture TranquilityProjectors = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/tranquility_projectors");
    public static Texture TranquilityZoneHex = AssetLoaderHelper.LoadTexture("textures/parts/leppa/UnstableElements/tranquility_zone_hex");
    public static Color TranquilityZoneColor = new(255 / 255f, 251 / 255f, 199 / 255f, 255 / 255f);
    private static readonly string TranquilityPowerId = "UnstableElements:tranquility_powered";
    private static readonly HashSet<HexIndex> TranquilityOffsets = new(){
        new(1, -1),
        new(1, -2), new(2, -2),
        new(1, -3), new(2, -3), new(3, -3),
        new(1, -4), new(2, -4), new(3, -4), new(4, -4)
    };


    public static readonly HashSet<HexIndex> TranquilityHexes = new();


    public static void Render(Part part, Vector2 pos, SolutionEditorBase editor, PartRenderer renderer) {
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
    }

    public static bool OnCycle(Sim sim, Part part, PartSimState simState, GlyphRecipe recipe, bool isCycleStart) {
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
    }

    public static void ClearTranquility(Sim sim, CycleEventExecutionType executionType) {
        // first thing
        TranquilityHexes.Clear();

        UeApi.OtherStableHexes.Clear();
        foreach (var cb in UeApi.OtherStableHexesCallbacks)
            UeApi.OtherStableHexes.UnionWith(cb(sim));
    }

    private static void DrawForPartWithTint(PartRenderer renderer, Texture tex, Vector2 offset, Vector2 size, float rotation, Color c) {
        Matrix4 tf = Matrix4.GetTranslation((renderer.partPos + offset).ToVector3(0)) * Matrix4.RotXY(renderer.partRotation + rotation) * Matrix4.GetTranslation(-size.ToVector3(0)) * Matrix4.GetScale(tex.size.ToVector3(0));
        TextureRenderer.Render(tex, c, tf);
    }

}
