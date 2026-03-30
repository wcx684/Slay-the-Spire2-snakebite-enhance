using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using OriginalPerfectedStrike = MegaCrit.Sts2.Core.Models.Cards.PerfectedStrike;

namespace snakebite.Cards;

[Pool(typeof(SilentCardPool))]
public sealed class SnakebitePerfected : CustomCardModel
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new ExtraDamageVar(2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    public override string PortraitPath => ModelDb.Card<OriginalPerfectedStrike>().PortraitPath;

    public SnakebitePerfected() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var owner = Owner;
        if (owner == null)
        {
            return;
        }

        int snakebiteCount = owner.PlayerCombatState?.AllCards.Count(IsSnakebiteCard) ?? 0;
        decimal poisonAmount = 7m + DynamicVars.ExtraDamage.BaseValue * snakebiteCount;

        await PowerCmd.Apply<PoisonPower>(cardPlay.Target, poisonAmount, owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }

    private static bool IsSnakebiteCard(CardModel card)
    {
        return card.Id.Entry.Contains("SNAKEBITE", StringComparison.OrdinalIgnoreCase);
    }
}
