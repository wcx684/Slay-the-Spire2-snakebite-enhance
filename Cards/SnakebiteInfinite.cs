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
using MegaCrit.Sts2.Core.Models.Powers;
using OriginalInfiniteBlades = MegaCrit.Sts2.Core.Models.Cards.InfiniteBlades;
using OriginalSnakebite = MegaCrit.Sts2.Core.Models.Cards.Snakebite;
using snakebite.Powers;

namespace snakebite.Cards;

[Pool(typeof(SilentCardPool))]
public sealed class SnakebiteInfinite : CustomCardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<OriginalSnakebite>(IsUpgraded),
        HoverTipFactory.FromPower<SnakebiteInfinitePower>()
    ];

    public override string PortraitPath => ModelDb.Card<OriginalInfiniteBlades>().PortraitPath;

    public SnakebiteInfinite() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = Owner;
        if (owner == null)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);
        await PowerCmd.Apply<SnakebiteInfinitePower>(owner.Creature, 1m, owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
