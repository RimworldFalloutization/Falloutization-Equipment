using HarmonyLib;

namespace Falloutization.Equipment;

public class FalloutizationEquipmentMod : Mod
{
    public FalloutizationEquipmentMod(ModContentPack content) : base(content)
    {
        new Harmony("Falloutization.Equipment").PatchAll();
    }
}

[HarmonyPatch(typeof(ThingDefGenerator_Neurotrainer), nameof(ThingDefGenerator_Neurotrainer.ImpliedThingDefs))]
public static class Patch_BobbleheadDefs
{
    internal static bool IsBobblehead(string skillDefName) => skillDefName != null && Bobbleheads.ContainsKey(skillDefName);

    private static readonly Dictionary<string, string> Bobbleheads = new()
    {
        ["Animals"] = "animals",
        ["Artistic"] = "artistic",
        ["Construction"] = "construction",
        ["Cooking"] = "cooking",
        ["Crafting"] = "crafting",
        ["Intellectual"] = "intellectual",
        ["Medicine"] = "medical",
        ["Melee"] = "melee",
        ["Mining"] = "mining",
        ["Plants"] = "plants",
        ["Shooting"] = "shooting",
        ["Social"] = "social",
    };

    public static void Postfix(ref IEnumerable<ThingDef> __result)
    {
        __result = Apply(__result);
    }

    private static IEnumerable<ThingDef> Apply(IEnumerable<ThingDef> defs)
    {
        foreach (ThingDef def in defs)
        {
            if (def.defName.StartsWith("Neurotrainer_") && Bobbleheads.TryGetValue(def.defName.Substring("Neurotrainer_".Length), out string texPath))
                ApplyBobblehead(def, texPath);
            yield return def;
        }
    }

    private static void ApplyBobblehead(ThingDef def, string texPath)
    {
        string skill = def.defName.Substring("Neurotrainer_".Length);
        def.label = ("Falloutization_Bobblehead" + skill + "Label").Translate();
        def.description = ("Falloutization_Bobblehead" + skill + "Description").Translate();
        def.graphicData.texPath = texPath;

        CompProperties_Usable usable = def.GetCompProperties<CompProperties_Usable>();
        if (usable == null)
            return;

        usable.useLabel = ("Falloutization_Bobblehead" + skill + "UseLabel").Translate();
        usable.useJob = DefDatabase<JobDef>.GetNamed("Falloutization_UseBobblehead" + skill);
    }
}

[HarmonyPatch(typeof(CompUseEffect_LearnSkill), nameof(CompUseEffect_LearnSkill.DoEffect))]
public static class Patch_BobbleheadUsedMessage
{
    public static bool Prefix(CompUseEffect_LearnSkill __instance, Pawn user)
    {
        if (!Patch_BobbleheadDefs.IsBobblehead(__instance.Props.skill?.defName))
            return true;

        SkillDef skill = __instance.Props.skill;
        int level = user.skills.GetSkill(skill).GetLevel();
        user.skills.Learn(skill, __instance.Props.xpGainAmount, direct: true);
        int levelAfter = user.skills.GetSkill(skill).GetLevel();
        if (PawnUtility.ShouldSendNotificationAbout(user))
        {
            Messages.Message(
                "Falloutization_BobbleheadUsed".Translate(user.LabelShort, skill.LabelCap, level, levelAfter, user.Named("USER")),
                user,
                MessageTypeDefOf.PositiveEvent);
        }

        return false;
    }
}
