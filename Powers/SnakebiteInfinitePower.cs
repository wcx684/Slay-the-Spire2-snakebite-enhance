using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using OriginalSnakebite = MegaCrit.Sts2.Core.Models.Cards.Snakebite;

namespace snakebite.Powers;

public sealed class SnakebiteInfinitePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<OriginalSnakebite>()];

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
    {
        var owner = Owner;
        if (owner == null || player != owner.Player)
        {
            return;
        }

        Flash();

        List<CardModel> generatedCards = new(Amount);
        for (int i = 0; i < Amount; i++)
        {
            CardModel snakebite = combatState.CreateCard<OriginalSnakebite>(owner.Player);
            snakebite.SetToFreeThisTurn();
            generatedCards.Add(snakebite);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(generatedCards, PileType.Hand, addedByPlayer: true);
    }
}
