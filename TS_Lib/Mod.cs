using TS_Lib.Save;
using UnityEngine;
using Verse;

namespace TS_Lib;

public class TSLibMod : Mod
{
    public TSLibMod(ModContentPack content) : base(content)
    {
        ConverterRegistrator.Register();
    }
}
