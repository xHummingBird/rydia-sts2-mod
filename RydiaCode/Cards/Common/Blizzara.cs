using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;
using Rydia.RydiaCode.Powers;

namespace Rydia.RydiaCode.Cards.Common;

public class Blizzara() : RydiaCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy), IMagicCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new PowerVar<FreezePower>(1m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FreezePower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Rydia rydia)
        {
            AudioHelper.PlayRandomMagic();
            SfxCmd.Play("res://Rydia/sfx/magic_cast.mp3", 1.1f);
            rydia.PlayAnimation(ownerCreature, "cast");
            await Task.Delay(200);
            rydia.PlayVfxOnTarget(
                play.Target,
                "res://Rydia/scenes/vfx.tscn",
                "blizzard");
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Rydia/sfx/ice.wav")
            .Execute(choiceContext);
        await PowerCmd.Apply<FreezePower>(choiceContext, play.Target, DynamicVars["FreezePower"].BaseValue, Owner.Creature, this);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["FreezePower"].UpgradeValueBy(1);
    }
}