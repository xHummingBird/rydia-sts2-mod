using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rydia.RydiaCode.Powers;

public class PiercingMagicPower : RydiaPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

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

        if (cardSource == null)
            return 1m;

        if (cardSource.Owner.Creature != Owner)
            return 1m;

        CardPile? pile = cardSource.Pile;

        if (pile != null &&
            pile.Type == PileType.Play &&
            cardSource.CurrentPlayIndex > 0)
        {
            return 1m;
        }

        int attacksPlayed = CombatManager.Instance.History.CardPlaysStarted.Count(
            entry =>
                entry.HappenedThisTurn(CombatState) &&
                entry.CardPlay.Card.Type == CardType.Attack &&
                entry.CardPlay.Player == Owner.Player);

        pile = cardSource.Pile;

        int currentAttack =
            pile != null && pile.Type == PileType.Play
                ? 1
                : 0;

        if (attacksPlayed > currentAttack)
            return 1m;

        return 1m + Amount / 100m;
    }
}