using HarmonyLib;
using System.Reflection;

namespace Overstocked.Patches;

[HarmonyPatch(typeof(GorillaSkinToggle))]
public class GorillaSkinTogglePatches
{
    private static readonly FieldInfo Rig = AccessTools.Field(typeof(GorillaSkinToggle), "_rig");

    [HarmonyPatch("OnEnable")]
    [HarmonyPrefix]
    public static bool OnEnablePatch(GorillaSkinToggle __instance)
    {
        return Rig.GetValue(__instance) 
               != null;
    }
}