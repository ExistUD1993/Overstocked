using GorillaExtensions;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace Overstocked.Patches;

[HarmonyPatch(typeof(GorillaSkin))]
public class GorillaSkinPatches
{
    [HarmonyPatch("ApplyToRig")]
    [HarmonyPrefix]
    public static bool ApplyToRigPatch(VRRig rig) => !GTExt.IsNull((Object)rig);
}