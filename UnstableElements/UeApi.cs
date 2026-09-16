using System;
using System.Collections.Generic;
using MonoMod.ModInterop;

namespace UnstableElements;

[ModExportName("UnstableElements")]
public class UeApi {
    internal static readonly List<Func<Sim, HashSet<HexIndex>>> OtherStableHexesCallbacks = new();
    public static readonly HashSet<HexIndex> OtherStableHexes = new();

    // use with:
    // public static Action<Func<Sim, HashSet<HexIndex>>> RegisterStableHexesCallback;
    public static void RegisterStableHexesCallback(Func<Sim, HashSet<HexIndex>> cb){
        OtherStableHexesCallbacks.Add(cb);
	}
}