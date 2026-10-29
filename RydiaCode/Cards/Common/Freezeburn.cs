using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rydia.RydiaCode.Powers;

namespace Rydia.RydiaCode.Cards.Common;

public class Freezeburn() : RydiaCard(
    1,
    CardType.Skill,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FreezePower>(),
        HoverTipFactory.FromPower<BurnPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<BurnPower>(5),
        new PowerVar<FreezePower>(2),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        bool hasFreeze = play.Target.HasPower<FreezePower>();
        bool hasBurn = play.Target.HasPower<BurnPower>();
        
        if (hasFreeze)
        {
            await PowerCmd.Apply<BurnPower>(
                choiceContext,
                play.Target,
                DynamicVars["BurnPower"].BaseValue,
                Owner.Creature,
                this);
        }

        if (hasBurn)
        {
            await PowerCmd.Apply<FreezePower>(
                choiceContext,
                play.Target,
                DynamicVars["FreezePower"].BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}