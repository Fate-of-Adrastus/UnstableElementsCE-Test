using MonoMod.ModInterop;
using Quintessential;

namespace UnstableElements;

internal class UnstableElements : QuintessentialMod {
    public override string ModId => "unstable_elements";
	public static UnstableElements Instance { get; }


    public override void Load() { }

    public override void LoadContent() {
        Atoms.AddAtomTypes();
        Parts.AddPartTypes();
        Solitaire.Load();
        // not sure about `static` load ordering so i'll leave this here
        typeof(UeApi).ModInterop();
    }
    public override void LoadCompatContent() { }
    public override void FinaliseContent() { }

    public override void PostLoad() { }
	public override void Unload() { }
}
