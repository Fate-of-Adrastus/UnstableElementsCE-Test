#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using UnstableElements;

public class patch_SolitaireScreen : SolitaireScreen {
    public patch_SolitaireScreen(SolitaireType solitaireType) : base(solitaireType) {}


    private extern SolitaireState orig_GetState();
    private SolitaireState GetState() {
        return this.IsUe() ? Solitaire.UeSolitaireState : orig_GetState();
    }

    private static void OnSolitaireScreenSetState(Action<SolitaireScreen, SolitaireState> orig, SolitaireScreen self, SolitaireState next) {
        if (self.IsUe())
            Solitaire.UeSolitaireState = next;
        else orig(self, next);
    }

    private extern SolitaireState orig_SetState(SolitaireState state);
    private void SetState(SolitaireState state) {
        if (this.IsUe())
            Solitaire.UeSolitaireState = state;
        else orig_SetState(state);
    }
}
