using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Powers;

namespace Rydia.RydiaCode.Relics;

public class IceRod : RydiaRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

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

        if (target == null)
            return 1m;

        if (!target.HasPower<FreezePower>())
            return 1m;

        return 1.25m;
    }
}