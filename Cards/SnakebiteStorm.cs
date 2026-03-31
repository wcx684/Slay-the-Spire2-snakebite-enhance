using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using OriginalSnakebite = MegaCrit.Sts2.Core.Models.Cards.Snakebite;

namespace snakebite.Cards;

[Pool(typeof(SilentCardPool))]
public sealed class SnakebiteStorm : CustomCardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<OriginalSnakebite>(IsUpgraded)];

    public override string PortraitPath
    {
        get
        {
            string entry = Id.Entry.ToLowerInvariant();
            int separator = entry.IndexOf('-');
            if (separator >= 0 && separator + 1 < entry.Length)
            {
                entry = entry[(separator + 1)..];
            }

            return $"res://snakebite/images/cards/{entry}.png";
        }
    }

    public SnakebiteStorm() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = Owner;
        if (owner == null)
        {
            return;
        }

        var combatState = CombatState;
        if (combatState == null)
        {
            return;
        }

        List<CardModel> handCards = PileType.Hand.GetPile(owner).Cards.ToList();
        int discardedCount = handCards.Count;

        // Snapshot hand first so this card mirrors Storm of Steel sequencing.
        await CardCmd.Discard(choiceContext, handCards);
        await Cmd.CustomScaledWait(0f, 0.25f);

        if (discardedCount <= 0)
        {
            return;
        }

        List<CardModel> generatedSnakebites = new(discardedCount);
        for (int i = 0; i < discardedCount; i++)
        {
            CardModel snakebite = combatState.CreateCard<OriginalSnakebite>(owner);
            SnakebiteCardUtils.SetExhaustOnPlay(snakebite);
            SnakebiteCardUtils.ApplySnakebiteTimeIfActive(snakebite);
            generatedSnakebites.Add(snakebite);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(generatedSnakebites, PileType.Hand, addedByPlayer: true);

        if (!IsUpgraded)
        {
            return;
        }

        foreach (CardModel snakebite in generatedSnakebites)
        {
            CardCmd.Upgrade(snakebite);
        }
    }
}