using GorillaExtensions;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Overstocked.Patches;

[HarmonyPatch(typeof(GorillaSkinToggle))]
public class GorillaSkinTogglePatches
{
    private static readonly FieldInfo _rig = AccessTools.Field(typeof(GorillaSkinToggle), "_rig");

    [HarmonyPatch("OnEnable")]
    [HarmonyPrefix]
    public static bool OnEnablePatch(GorillaSkinToggle __instance)
    {
        return !GTExt.IsNull(_rig.GetValue(__instance) as Object);
    }
}