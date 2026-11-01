using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
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

public class Thundara() : RydiaCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy), IMagicCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new PowerVar<ShockPower>(2m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ShockPower>(),
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
                "thunder");
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "event:/sfx/characters/defect/defect_lightning_passive")
            .Execute(choiceContext);
        await PowerCmd.Apply<ShockPower>(choiceContext, play.Target, DynamicVars["ShockPower"].BaseValue, Owner.Creature, this);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["ShockPower"].UpgradeValueBy(1);
    }
}