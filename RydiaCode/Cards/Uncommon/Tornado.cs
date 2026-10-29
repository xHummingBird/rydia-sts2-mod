using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Cards.Uncommon;

public class Tornado() : RydiaCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AllEnemies), IMagicCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move),
        new RepeatVar(5)
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
            SfxCmd.Play("res://Rydia/sfx/wind.wav", 1.2f);
            foreach (var enemy in CombatState.HittableEnemies)
                rydia.PlayVfxOnTarget(enemy, "res://Rydia/scenes/vfx.tscn", "tornado");
            await CommonActions.CardAttack(this, play.Target, DynamicVars.Repeat.IntValue)
                .WithHitFx(null, "res://Rydia/sfx/tornado_hit.wav")
                .WithHitVfxNode(target =>
                {
                    rydia.PlayVfxOnTarget(
                        target,
                        "res://Rydia/scenes/vfx.tscn",
                        "tornado_hit"
                    );
                    return null;
                })
                .Execute(choiceContext);
        }
        else  await CommonActions.CardAttack(this, play.Target, DynamicVars.Repeat.IntValue)
            .Execute(choiceContext);
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}