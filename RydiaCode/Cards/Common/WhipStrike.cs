using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Cards.Common;

public class WhipStrike() : RydiaCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        new DynamicVar("Summon", 5)
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

        SummonManager.GainSummon(Owner, DynamicVars["Summon"].IntValue);

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["Summon"].UpgradeValueBy(3);
    }
}