using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Powers;

public class FastTalkerPower : RydiaPower
{
    private bool _triggeredThisTurn;

    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (_triggeredThisTurn)
            return;

        if (cardPlay.Card.Owner.Creature != Owner)
            return;

        if (cardPlay.Card is not IEidolonCard)
            return;

        _triggeredThisTurn = true;

        Flash();

        await PlayerCmd.GainEnergy(
            (int)Amount,
            Owner.Player);
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner))
            _triggeredThisTurn = false;

        return Task.CompletedTask;
    }
}