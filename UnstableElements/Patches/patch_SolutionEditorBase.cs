#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.Linq;
using UnstableElements;

public abstract class patch_SolutionEditorBase : SolutionEditorBase {

    public extern void orig_method_1984(Vector2 param_5533, Bounds2 param_5534, Bounds2 param_5535, bool param_5536, Maybe<List<Molecule>> param_5537, bool param_5538);
    public void method_1984(Vector2 param_5533, Bounds2 param_5534, Bounds2 param_5535, bool param_5536, Maybe<List<Molecule>> param_5537, bool param_5538) {
        orig_method_1984(param_5533, param_5534, param_5535, param_5536, param_5537, param_5538);

        if (this.GetSimPlayState() != SimPlayState.Stopped) {
            double time = Math.Sin(new DeltaTime(Time.Now().Ticks).InSeconds());
            float pulse = (float)(time / 4 + .75) / 2.4f;

            if (this is SimpleSolutionEditor)
                pulse = 0.25f; // constant brightness in GIFs

            Color tint = Parts.TranquilityZoneColor;
            HexGrid conv = HexGrid.standardGrid;
            tint.A *= pulse;
            foreach (var hex in Parts.TranquilityHexes) {
                Vector2 hexAsVec = conv.ToPixelCoords(hex) + param_5533 - new Vector2(2, 8);
                Matrix4 tf = Matrix4.GetTranslation(hexAsVec.ToVector3(0)) * Matrix4.RotXY(0) * Matrix4.GetTranslation(new Vector3(-40, -40, 0)) * Matrix4.GetScale(Parts.TranquilityZoneHex.size.ToVector3(0));
                TextureRenderer.Render(Parts.TranquilityZoneHex, tint, tf);
            }
        }
    }
}
