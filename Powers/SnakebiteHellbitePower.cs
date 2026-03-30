using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace snakebite.Powers;

public sealed class SnakebiteHellbitePower : PowerModel
{
    private HashSet<CardModel>? _autoplayingCards;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    private HashSet<CardModel> AutoplayingCards
    {
        get
        {
            AssertMutable();
            _autoplayingCards ??= new HashSet<CardModel>();
            return _autoplayingCards;
        }
    }

    public override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        var ownerCombatState = Owner.CombatState;
        if (ownerCombatState == null)
        {
            return;
        }

        if (card.Owner.Creature == Owner && IsSnakebiteCard(card) && !ownerCombatState.HittableEnemies.All((Creature c) => c.ShowsInfiniteHp))
        {
            AutoplayingCards.Add(card);
            await CardCmd.AutoPlay(choiceContext, card, null);
            AutoplayingCards.Remove(card);
        }
    }

    public override Task BeforeAttack(AttackCommand command)
    {
        if (!AutoplayingCards.Contains(command.ModelSource))
        {
            return Task.CompletedTask;
        }

        command.WithHitFx("vfx/hellraiser_attack_vfx", command.HitSfx, command.TmpHitSfx);

        var attackerCharacter = command.Attacker?.Player?.Character;
        if (attackerCharacter != null)
        {
            command.WithAttackerAnim("Cast", attackerCharacter.CastAnimDelay);
        }

        command
            .SpawningHitVfxOnEachCreature()
            .WithHitVfxSpawnedAtBase();

        return Task.CompletedTask;
    }

    private static bool IsSnakebiteCard(CardModel card)
    {
        var entry = card.Id.Entry;
        if (!string.IsNullOrEmpty(entry) && entry.Contains("SNAKEBITE", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var title = card.Title;
        return !string.IsNullOrEmpty(title) && title.Contains("蛇咬", StringComparison.Ordinal);
    }
}
