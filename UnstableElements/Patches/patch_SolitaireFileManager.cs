#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using UnstableElements;

public class patch_SolitaireFileManager : SolitaireScreen {
    public patch_SolitaireFileManager(SolitaireType solitaireType) : base(solitaireType) {}

    public static extern SolitaireGameState orig_GetRandomFromFile(SolitaireType type);
    public static SolitaireGameState GetRandomFromFile(SolitaireType type) {
        return SolitaireExt.IsCurrentSolitaireUe() ? Solitaire.GenerateSolitaireBoard() : orig_GetRandomFromFile(type);
    }
}
