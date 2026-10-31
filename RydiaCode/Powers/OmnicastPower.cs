using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rydia.RydiaCode.Powers;

public class OmnicastPower : RydiaPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (amount == 0m)
            return;

        if (power.GetTypeForAmount(amount) != PowerType.Debuff)
            return;

        if (!power.Owner.IsEnemy)
            return;

        if (applier != Owner)
            return;

        if (power is ITemporaryPower)
            return;

        Flash();

        await CreatureCmd.Damage(
            choiceContext,
            power.Owner,
            Amount,
            ValueProp.Unpowered,
            Owner);
    }
}