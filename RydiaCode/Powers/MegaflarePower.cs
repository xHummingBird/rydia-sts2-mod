using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;

namespace Rydia.RydiaCode.Powers;

public class MegaflarePower : RydiaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(30m, ValueProp.Move)
    ];

    /// <summary>
    /// Set the amount of damage that this instance of The Bomb will deal.
    /// This is necessary because Amount is used to track the number of turns left until exploding.
    /// </summary>
    /// <param name="damage">Amount of damage to deal</param>
    public void SetDamage(decimal damage)
    {
        AssertMutable();
        base.DynamicVars.Damage.BaseValue = damage;
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(base.Owner))
        {
            return;
        }
        if (base.Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }
        Flash();
        var targets = base.CombatState.HittableEnemies;
        Character.Rydia? rydia = Owner?.Player.Character as Character.Rydia;

        if (rydia != null)
        {
            AudioHelper.PlayRandomSummon();
            rydia.PlayAnimation(Owner, "summon_cast");
            SfxCmd.Play("res://Rydia/sfx/summon_cast.mp3", 1.1f);
            rydia.PlayVfxOnTarget(targets.First(), "res://Rydia/scenes/vfx.tscn", "bahamut");
            SfxCmd.Play("res://Rydia/sfx/from_tifa/final_heaven_1.wav");
            await Task.Delay(733);
            SfxCmd.Play("res://Rydia/sfx/from_tifa/meteostrike_3.wav");
        }

        foreach (var target in targets)
        {
            var vfx = NGroundFireVfx.Create(target);
            if (vfx != null)
            {
                NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
            }
        }
        await CreatureCmd.Damage(choiceContext, base.CombatState.HittableEnemies, base.DynamicVars.Damage, base.Owner);
        await Task.Delay(1000);
        await PowerCmd.Remove(this);
    }
}