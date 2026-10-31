using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Rydia.RydiaCode.Powers;

namespace Rydia.RydiaCode.Cards.Uncommon;

public class Omnicast() : RydiaCard(
    1,
    CardType.Power,
    CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<OmnicastPower>(5),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<OmnicastPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["OmnicastPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["OmnicastPower"].UpgradeValueBy(2);
    }
}