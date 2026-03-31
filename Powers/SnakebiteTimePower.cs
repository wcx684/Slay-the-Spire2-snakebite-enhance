using System;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace snakebite.Powers;

public sealed class SnakebiteTimePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    // Reuse a vanilla time-related power icon to keep style consistent with base game.
    public override string? CustomPackedIconPath => ModelDb.Power<EnergyNextTurnPower>().PackedIconPath;

    public override string? CustomBigIconPath => ModelDb.Power<EnergyNextTurnPower>().ResolvedBigIconPath;

    public override string? CustomBigBetaIconPath => ModelDb.Power<EnergyNextTurnPower>().ResolvedBigIconPath;

    public override Task AfterCardGeneratedForCombat(CardModel card, bool addedByPlayer)
    {
        if (card.Owner.Creature != Owner || !SnakebiteCardUtils.IsSnakebiteCard(card) || card.EnergyCost.CostsX)
        {
            return Task.CompletedTask;
        }

        Flash();
        card.SetToFreeThisTurn();
        return Task.CompletedTask;
    }

    public override Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner.Creature != Owner || !SnakebiteCardUtils.IsSnakebiteCard(card) || card.EnergyCost.CostsX)
        {
            return Task.CompletedTask;
        }

        Flash();
        card.SetToFreeThisTurn();
        return Task.CompletedTask;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (Owner.Side != side)
        {
            return;
        }

        await PowerCmd.Remove(this);
    }
}