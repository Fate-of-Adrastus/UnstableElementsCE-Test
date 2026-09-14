#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Mono.Cecil;
using MonoMod;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using UnstableElements;

public class patch_Sim : Sim {

    private extern bool orig_HasOverlap(Molecule molecule, HashSet<HexIndex> additionalHexes);

    private bool HasOverlap(Molecule molecule, HashSet<HexIndex> additionalHexes) {

        bool blocked = orig_HasOverlap(molecule, additionalHexes);
        if (!blocked) // if its not blocked by collisions, but is made of Aether and not stabilized, block it
            if (molecule.GetAtoms().Values.Any() && molecule.GetAtoms().Values.Select(u => u.atomType).All(u => u.QuintAtomType == Atoms.Aether.QuintAtomType))
                if (!molecule.GetAtoms().Keys.All(hex => Atoms.IsHexStabilized(hex)))
                    return true;
        return blocked;
    }


    [MonoModWrapOperation("System.Boolean Sim::IsSameMolecule(Molecule,Molecule)", "Field-Read", "Atom::atomType")]
    static AtomType ModSimValidate(Atom @base, Func<Atom, AtomType> orig) {
        AtomType type = orig(@base);
        return Atoms.IsUraniumState(type) ? Atoms.Uranium : type;
    }
}
