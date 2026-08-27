#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Quintessential;
using UnstableElements;

public class patch_JournalScreen : JournalScreen {
    public patch_JournalScreen(bool showDifficultyWarningIfNeeded) : base(showDifficultyWarningIfNeeded) {}

    private extern void orig_RenderPuzzleSelect(Puzzle puzzle, Vector2 pos, bool isLarge);
    private void RenderPuzzleSelect(Puzzle puzzle, Vector2 pos, bool isLarge) {
        if (puzzle.puzzleId == "QuickIron") {
            Texture puzzleBg = isLarge ? Assets.textures.journal.puzzle_large : Assets.textures.journal.puzzle_small;
            Texture tick = true /* TODO: count wins */ ? Assets.textures.puzzle_select.list_checked : Assets.textures.puzzle_select.list_unchecked;
            Texture divider = isLarge ? Assets.textures.journal.divider_large : Assets.textures.journal.divider_small;
            Bounds2 bounds = Bounds2.WithSize(pos, puzzleBg.size.ToVector2());
            bool hover = bounds.Contains(InputManager.MousePos());
            TextureRenderer.RenderText("Shattered Garden", pos + new Vector2(9, -19), Assets.fonts.crimson_15, class_181.field_1718, 0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
            UI.DrawTexture(tick, pos + new Vector2(puzzleBg.size.X - 27, -23f));
            UI.DrawTexture(puzzleBg, pos);
            UI.DrawTexture(divider, pos + new Vector2(7f, -34f));
            UI.DrawTexture(hover ? Solitaire.sigmarHoverSprite : Solitaire.sigmarSprite, bounds.Min + new Vector2(13f, 13f));
            if (hover && InputManager.IsClickPressed(MouseButtonType.LeftClick)) {
                var solitaireScreen = new SolitaireScreen(SolitaireType.Quintessence);
                solitaireScreen.SetUe(true);
                UI.OpenScreen(solitaireScreen);
                Assets.sounds.click_button.method_28(1f);
            }
        } else
            orig_RenderPuzzleSelect(puzzle, pos, isLarge);
    }
}
