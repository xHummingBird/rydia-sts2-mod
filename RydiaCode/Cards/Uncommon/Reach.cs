using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Rydia.RydiaCode.Cards.Uncommon;

public class Reach() : RydiaCard(
    1,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var drawnCards = await CardPileCmd.Draw(
            choiceContext,
            1,
            Owner);
        
        var drawnCard = drawnCards.FirstOrDefault();

        if (drawnCard?.Type == CardType.Attack)
            drawnCard.SetToFreeThisTurn();
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}