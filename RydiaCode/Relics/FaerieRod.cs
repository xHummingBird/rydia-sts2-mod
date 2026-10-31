using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Relics;

public class FaerieRod : RydiaRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    public override Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature) &&
            Owner.PlayerCombatState.TurnNumber <= 1)
        {
            Flash();

            SummonManager.GainSummon(
                Owner,
                25);
        }

        return Task.CompletedTask;
    }
}