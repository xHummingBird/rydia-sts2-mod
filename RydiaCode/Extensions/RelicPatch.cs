using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using Rydia.RydiaCode.Cards.Ancient;
using Rydia.RydiaCode.Cards.Basic;
using Rydia.RydiaCode.Relics;

namespace Rydia.RydiaCode.Extensions;

[HarmonyPatch(typeof(TouchOfOrobas), "GetUpgradedStarterRelic")]
internal static class RydiaTouchOfOrobasPatch
{
    private static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is Whip)
        {
            __result = ModelDb.Relic<MysticWhip>().ToMutable();
        }
    }
}

[HarmonyPatch(typeof(ArchaicTooth), "TranscendenceUpgrades", MethodType.Getter)]
internal static class RydiaArchaicToothTranscendencePatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Dictionary<ModelId, CardModel> __result)
    {
        __result[ModelDb.Card<MistBreath>().Id] = ModelDb.Card<RadiantBreath>();
    }
}

[HarmonyPatch(typeof(DustyTome), nameof(DustyTome.SetupForPlayer))]
public static class DustyTomeSetupPatch
{
    [HarmonyPostfix]
    public static void Postfix(DustyTome __instance, Player player)
    {
        if (player.Character is not Character.Rydia)
            return;

        __instance.AncientCard = ModelDb.Card<Evoke>().Id;
    }
}


[HarmonyPatch(typeof(DustyTome), nameof(DustyTome.AfterObtained))]
public static class DustyTomePatch
{
    [HarmonyPrefix]
    public static void Prefix(DustyTome __instance)
    {
        if (__instance.Owner?.Character is not Character.Rydia)
            return;
        
        __instance.AncientCard = ModelDb.Card<Evoke>().Id;
    }
}