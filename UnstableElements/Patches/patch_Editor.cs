#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System.Linq;
using UnstableElements;

public class patch_Editor {

    public static extern void orig_RenderAtom(AtomType atomType, Vector2 translation, float scaleMultiplier, float opacityMultiplier, float height, float shadowStrength, float shadowOffset, float shadowAngle, Texture shadow, Texture overlayEffect, bool isOutputRender);

    public static void RenderAtom(AtomType atomType, Vector2 translation, float scaleMultiplier, float opacityMultiplier, float height, float shadowStrength, float shadowOffset, float shadowAngle, Texture shadow, Texture overlayEffect, bool isOutputRender) {
        if (Atoms.SlowShakingIso.Contains(atomType))
            translation += new Vector2(Atoms.UraniumShakeCounter.GetFloat(-4, 4) / 4f, Atoms.UraniumShakeCounter.GetFloat(-4, 4) / 4f);
        if (Atoms.FastShakingIso.Contains(atomType))
            translation += new Vector2(Atoms.UraniumShakeCounter.GetFloat(-4, 4) / 2f, Atoms.UraniumShakeCounter.GetFloat(-4, 4) / 2f);
        orig_RenderAtom(atomType, translation, scaleMultiplier, opacityMultiplier, height, shadowStrength, shadowOffset, shadowAngle, shadow, overlayEffect, isOutputRender);
    }
}
