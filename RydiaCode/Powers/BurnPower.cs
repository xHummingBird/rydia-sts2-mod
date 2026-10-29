using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Cards.Rare;

namespace Rydia.RydiaCode.Powers;

public sealed class BurnPower : RydiaPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Color AmountLabelColor => PowerModel._normalAmountLabelColor;

    private int TriggerCount =>
        Math.Min(
            Amount,
            Owner.HasPower<FreezePower>() ? 2 : 1);

    public int CalculateTotalDamageNextTurn()
    {
        decimal totalDamage = 0m;
        int triggerCount = TriggerCount;

        for (int i = 0; i < triggerCount; i++)
        {
            decimal damage = Amount - i;

            damage = Hook.ModifyDamage(
                Owner.CombatState.RunState,
                Owner.CombatState,
                Owner,
                null,
                damage,
                ValueProp.Unblockable | ValueProp.Unpowered,
                null,
                null,
                ModifyDamageHookType.All,
                CardPreviewMode.None,
                out IEnumerable<AbstractModel> _);

            totalDamage += damage;
        }

        return (int)totalDamage;
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner))
        {
            await Trigger();
        }
    }

    public async Task Trigger()
    {
        int iterations = TriggerCount;

        for (int i = 0; i < iterations; i++)
        {
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(Owner));
            SfxCmd.Play("event:/sfx/characters/attack_fire");
            
            await CreatureCmd.Damage(
                new ThrowingPlayerChoiceContext(),
                Owner,
                Amount,
                ValueProp.Unblockable | ValueProp.Unpowered,
                null,
                null);

            if (Owner.IsAlive)
            {
                await PowerCmd.Decrement(this);
            }
            else
            {
                await Cmd.CustomScaledWait(0.1f, 0.25f);
            }
        }
    }
    
    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner)
        {
            return 1m;
        }

        if (cardSource is not TidalWave)
        {
            return 1m;
        }
        
        if (dealer.HasPower<EidolonResonancePower>() || dealer.HasPower<EvokePower>()) 
            return 2.0m;

        return 1.5m;
    }
}