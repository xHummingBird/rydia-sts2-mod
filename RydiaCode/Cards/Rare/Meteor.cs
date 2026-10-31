using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Cards.Rare;

public class Meteor() : RydiaCard(
    0,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AllEnemies), IMagicCard
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DamageVar(5m, ValueProp.Move)
    ];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int num = ResolveEnergyXValue();
        if (num > 0)
        {
            var ownerCreature = Owner.Creature;
            var enemies = CombatState.HittableEnemies;
            if (enemies.Count == 1)
                num *= 2;
            if (ownerCreature != null && Owner?.Character is Character.Rydia rydia)
            {
                AudioHelper.PlayRandomMagic();
                SfxCmd.Play("res://Rydia/sfx/magic_cast.mp3", 1.1f);
                rydia.PlayAnimation(ownerCreature, "cast");
                await Task.Delay(200);
                foreach (var enemy in enemies)
                    rydia.PlayVfxOnTarget(
                        enemy,
                        "res://Rydia/scenes/vfx.tscn",
                        "meteor");
                SfxCmd.Play("res://Rydia/sfx/from_killia/fire_charge.wav");
                await Task.Delay(200);
                SfxCmd.Play("res://Rydia/sfx/from_killia/fire_impact (1).wav");
            }
            await CommonActions.CardAttack(this, cardPlay.Target, num)
                .WithHitFx(null)
                .BeforeDamage(async delegate
                {
                    foreach (var enemy in enemies)
                    {
                        var vfx = NGroundFireVfx.Create(enemy);
                        if (vfx != null)
                        {
                            NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                        }
                    }
                    SfxCmd.Play("event:/sfx/characters/attack_fire");
                })
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);
    }
}