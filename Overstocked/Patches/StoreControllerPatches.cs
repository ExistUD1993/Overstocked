using GorillaNetworking.Store;
using HarmonyLib;
using System.Collections.Generic;

namespace Overstocked.Patches;

[HarmonyPatch(typeof(StoreController))]
public class StoreControllerPatches
{
    [HarmonyPatch(nameof(StoreController.AddStandToPlayfabIDDictionary))]
    [HarmonyPrefix]
    public static bool AddStandToPlayfabIDDictionaryPatch(StoreController __instance, DynamicCosmeticStand dynamicCosmeticStand)
    {
        if (string.IsNullOrEmpty(dynamicCosmeticStand.StandName) || string.IsNullOrEmpty(dynamicCosmeticStand.thisCosmeticName))
            return false;

        if (__instance.StandsByPlayfabID.ContainsKey(dynamicCosmeticStand.thisCosmeticName))
        {
            if (!__instance.StandsByPlayfabID[dynamicCosmeticStand.thisCosmeticName].Contains(dynamicCosmeticStand))
                __instance.StandsByPlayfabID[dynamicCosmeticStand.thisCosmeticName].Add(dynamicCosmeticStand);
        }
        else
        {
            __instance.StandsByPlayfabID.Add(dynamicCosmeticStand.thisCosmeticName, new List<DynamicCosmeticStand>(1) { dynamicCosmeticStand });
        }

        return false;
    }
}