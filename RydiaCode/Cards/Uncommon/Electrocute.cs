using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rydia.RydiaCode.Powers;

namespace Rydia.RydiaCode.Cards.Uncommon;

public class Electrocute() : RydiaCard(
    1,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ShockPower>(3m),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ShockPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<ShockPower>(
            choiceContext,
            CombatState.HittableEnemies,
            DynamicVars["ShockPower"].BaseValue,
            Owner.Creature,
            this);

        await PowerCmd.Apply<ShockPersistencePower>(
            choiceContext,
            CombatState.HittableEnemies,
            1,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}