using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Rydia.RydiaCode.Powers;

public class AsurasBoonPower : RydiaPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromPower<ShockPower>(),
        HoverTipFactory.FromPower<FreezePower>()
    ];

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner))
            return;

        Flash();

        for (int i = 0; i < Amount; i++)
        {
            Creature? enemy =
                Owner.Player.RunState.Rng.CombatTargets.NextItem(
                    CombatState.HittableEnemies);

            if (enemy == null)
                continue;

            int effect =
                Owner.Player.RunState.Rng.CombatCardGeneration.NextInt(3);

            switch (effect)
            {
                case 0:
                    await PowerCmd.Apply<BurnPower>(
                        new ThrowingPlayerChoiceContext(),
                        enemy,
                        3,
                        Owner,
                        null);
                    break;

                case 1:
                    await PowerCmd.Apply<ShockPower>(
                        new ThrowingPlayerChoiceContext(),
                        enemy,
                        3,
                        Owner,
                        null);
                    break;

                case 2:
                    await PowerCmd.Apply<FreezePower>(
                        new ThrowingPlayerChoiceContext(),
                        enemy,
                        1,
                        Owner,
                        null);
                    break;
            }
        }
    }
}