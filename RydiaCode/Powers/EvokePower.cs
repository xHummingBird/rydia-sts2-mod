using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Powers;

public class EvokePower : RydiaPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;

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

        if (dealer != Owner)
            return 1m;

        if (cardSource is not IEidolonCard)
            return 1m;

        // Eidolon Resonance already provides the x2 multiplier.
        // Do not stack Evoke on top of it.
        if (Owner.HasPower<EidolonResonancePower>())
            return 1m;

        return 2m;
    }
}