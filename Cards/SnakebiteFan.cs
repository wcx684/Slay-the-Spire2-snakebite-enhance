using System.Collections.Generic;
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
using snakebite.Powers;
using OriginalSnakebite = MegaCrit.Sts2.Core.Models.Cards.Snakebite;

namespace snakebite.Cards;

[Pool(typeof(SilentCardPool))]
public sealed class SnakebiteFan : CustomCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(4)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<OriginalSnakebite>(IsUpgraded),
        HoverTipFactory.FromPower<SnakebiteFanPower>()
    ];

    public override string PortraitPath => "res://snakebite/images/cards/snakebite_fan.png";

    public SnakebiteFan() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var owner = Owner;
        var combatState = CombatState;
        if (owner == null || combatState == null)
        {
            return;
        }

        await CreatureCmd.TriggerAnim(owner.Creature, "Cast", owner.Character.CastAnimDelay);

        await PowerCmd.Apply<SnakebiteFanPower>(owner.Creature, 1m, owner.Creature, this);

        List<CardModel> generatedCards = new(DynamicVars.Cards.IntValue);
        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            CardModel snakebite = combatState.CreateCard<OriginalSnakebite>(owner);
            SnakebiteCardUtils.ApplySnakebiteTimeIfActive(snakebite);
            generatedCards.Add(snakebite);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(generatedCards, PileType.Hand, addedByPlayer: true);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
