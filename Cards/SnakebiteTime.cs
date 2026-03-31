using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using snakebite.Powers;
using OriginalSnakebite = MegaCrit.Sts2.Core.Models.Cards.Snakebite;

namespace snakebite.Cards;

[Pool(typeof(SilentCardPool))]
public sealed class SnakebiteTime : CustomCardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<OriginalSnakebite>(IsUpgraded)];

    public override string PortraitPath => "res://snakebite/images/cards/snakebiteTime.png";

    public SnakebiteTime() : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = Owner;
        if (owner == null)
        {
            return;
        }

        await PowerCmd.Apply<SnakebiteTimePower>(owner.Creature, 1m, owner.Creature, this);

        foreach (PileType pileType in new[] { PileType.Hand, PileType.Draw, PileType.Discard })
        {
            foreach (CardModel card in pileType.GetPile(owner).Cards)
            {
                if (card is OriginalSnakebite)
                {
                    SnakebiteCardUtils.ApplySnakebiteTimeIfActive(card);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
