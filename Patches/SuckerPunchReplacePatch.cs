using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace snakebite.Patches;

[HarmonyPatch(typeof(SuckerPunch), "OnPlay")]
internal static class SuckerPunchReplacePatch
{
    [HarmonyPrefix]
    private static bool Prefix(SuckerPunch __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        __result = PlayReplacedEffect(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task PlayReplacedEffect(SuckerPunch card, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var owner = card.Owner;
        if (owner?.Creature == null)
        {
            return;
        }

        decimal poisonAmount = card.IsUpgraded ? 7m : 5m;
        decimal weakAmount = card.DynamicVars.Weak.BaseValue;

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);
        await PowerCmd.Apply<PoisonPower>(cardPlay.Target, poisonAmount, owner.Creature, card);
        await PowerCmd.Apply<WeakPower>(cardPlay.Target, weakAmount, owner.Creature, card);
    }
}
