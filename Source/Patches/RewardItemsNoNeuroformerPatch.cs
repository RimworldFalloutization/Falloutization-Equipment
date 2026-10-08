using System.Collections.Generic;
using HarmonyLib;

namespace Falloutization.Equipment.Patches;

/// <summary>
/// Vanilla injects psylink neuroformers into item quest rewards via special timing
/// logic in <see cref="Reward_Items.InitFromValue"/> (not thingSetMakerTags).
/// Disallow the def so that path never fires and tag-based fillers also skip it.
/// </summary>
[HarmonyPatch(typeof(Reward_Items), nameof(Reward_Items.InitFromValue))]
public static class RewardItemsNoNeuroformerPatch
{
    public static void Prefix(ref RewardsGeneratorParams parms)
    {
        ThingDef neuroformer = ThingDefOf.PsychicAmplifier;
        if (neuroformer == null)
        {
            return;
        }

        if (parms.disallowedThingDefs != null && parms.disallowedThingDefs.Contains(neuroformer))
        {
            return;
        }

        List<ThingDef> disallowed = parms.disallowedThingDefs != null
            ? new List<ThingDef>(parms.disallowedThingDefs)
            : new List<ThingDef>();
        disallowed.Add(neuroformer);
        parms.disallowedThingDefs = disallowed;
    }
}
