using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Cards.Uncommon;

public class Whiplash() : RydiaCard(1, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0m),
        new ExtraDamageVar(3m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
            CombatManager.Instance.History.CardPlaysFinished.Count(
                entry =>
                    entry.CardPlay.Card is IEidolonCard
            ))
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Rydia rydia)
        {
            AudioHelper.PlayRandomAttack();
            SfxCmd.Play("res://Rydia/sfx/whip_swing.mp3", 1.2f);
            rydia.DashTo(ownerCreature, play.Target, distance: 300f, durationSeconds: 0.2F, overrideAnim: "attack");
            await Task.Delay(83);
            SfxCmd.Play("res://Rydia/sfx/whip_hit1.mp3", 1.1f);
            await Task.Delay(17);
            rydia.PlayVfxOnTarget(play.Target, "res://Rydia/scenes/vfx.tscn", "whip_hit");
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null)
                .Execute(choiceContext);
            await rydia.Retreat(ownerCreature, duration: 0.25F);
        }
        else  await CommonActions.CardAttack(this, play.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}