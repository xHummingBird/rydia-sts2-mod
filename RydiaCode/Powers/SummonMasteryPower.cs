using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Powers;

public class SummonMasteryPower : RydiaPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;
    
    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card.Owner != Owner.Player)
            return Task.CompletedTask;

        if (card is not IEidolonCard)
            return Task.CompletedTask;

        CardCmd.ApplyKeyword(
            card,
            CardKeyword.Retain);

        return Task.CompletedTask;
    }

    public override Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        foreach (CardModel card in Owner.Player.PlayerCombatState.AllCards
                     .Where(card => card is IEidolonCard))
        {
            CardCmd.ApplyKeyword(
                card,
                CardKeyword.Retain);
        }

        return Task.CompletedTask;
    }
    
    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!CombatManager.Instance.IsInProgress)
            return;

        if (Owner == null)
            return;

        if (side != Owner.Side)
            return;

        if (Owner.IsDead)
            return;

        if (!Owner.HasPower<EidolonResonancePower>())
            Flash();
        
        SummonManager.GainSummon(Owner.Player, Amount);
    }
}