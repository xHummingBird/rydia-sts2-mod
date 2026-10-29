using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Cards.Uncommon;

namespace Rydia.RydiaCode.Powers;

public sealed class FreezePower : RydiaPower
{
    private const string DamageDecrease = "DamageDecrease";

    public override PowerType Type =>
        PowerType.Debuff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(DamageDecrease, 0.20m)
    ];

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (!props.IsPoweredAttack())
            return 1m;
        
        var damageDecrease = DynamicVars[DamageDecrease].BaseValue;

        if (dealer.HasPower<DiamondDustPower>())
            damageDecrease *= 2;
        
        if (dealer.HasPower<ConductivePower>())
            damageDecrease *= 2;
        
        // The creature with Freeze deals 20% less Attack damage.
        if (dealer == Owner)
        {
            return 1 - damageDecrease;
        }
        
        if (cardSource is Quaga && target == Owner)
        {
            if (cardSource.IsUpgraded)
                return 1.66m;
            
            return 1.5m;
        }
        
        return 1m;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
        {
            await PowerCmd.TickDownDuration(this);
        }
    }
}