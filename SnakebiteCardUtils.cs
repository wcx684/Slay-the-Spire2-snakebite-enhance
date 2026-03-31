using System;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using snakebite.Powers;

namespace snakebite;

internal static class SnakebiteCardUtils
{
    public static void ApplySnakebiteTimeIfActive(CardModel card)
    {
        if (card.Owner?.Creature == null || !card.Owner.Creature.HasPower<SnakebiteTimePower>())
        {
            return;
        }

        if (!IsSnakebiteCard(card) || card.EnergyCost.CostsX)
        {
            return;
        }

        card.SetToFreeThisTurn();
    }

    public static bool IsSnakebiteCard(CardModel card)
    {
        var entry = card.Id.Entry;
        if (!string.IsNullOrEmpty(entry) && entry.Contains("SNAKEBITE", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var title = card.Title;
        return !string.IsNullOrEmpty(title) && title.Contains("蛇咬", StringComparison.Ordinal);
    }

    public static void SetExhaustOnPlay(CardModel card)
    {
        card.ExhaustOnNextPlay = true;
        card.AddKeyword(CardKeyword.Exhaust);
    }
}