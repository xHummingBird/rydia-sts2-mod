using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Relics;

public class MysticWhip : SummonRelicBase
{
    public override RelicRarity Rarity =>
        RelicRarity.Ancient;

    protected override int SummonPerTurn =>
        7;

    protected override int SummonPerAttack =>
        5;

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

        if (dealer != Owner.Creature)
            return 1m;

        if (cardSource is not IEidolonCard)
            return 1m;

        return 1.25m;
    }
}