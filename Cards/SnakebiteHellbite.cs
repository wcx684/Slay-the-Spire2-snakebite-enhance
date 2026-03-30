using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using snakebite.Powers;

namespace snakebite.Cards;

[Pool(typeof(SilentCardPool))]
public sealed class SnakebiteHellbite : CustomCardModel
{
    protected override IEnumerable<string> ExtraRunAssetPaths => NHellraiserVfx.AssetPaths;

    public override string PortraitPath => "res://snakebite/images/cards/SnakebiteHellbitePower.png";

    public SnakebiteHellbite() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SnakebiteHellbitePower>(Owner.Creature, 1m, Owner.Creature, this);
    }

    public override async Task OnEnqueuePlayVfx(Creature? target)
    {
        var vfxContainer = NCombatRoom.Instance?.CombatVfxContainer;
        if (vfxContainer != null)
        {
            vfxContainer.AddChild(NHellraiserVfx.Create(Owner.Creature));
        }

        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
