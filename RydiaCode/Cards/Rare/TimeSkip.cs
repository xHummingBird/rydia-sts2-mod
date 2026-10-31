using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Cards.Rare;

public class TimeSkip() : RydiaCard(
    0,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(4)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        foreach (CardModel card in PileType.Hand.GetPile(Owner).Cards.ToList())
        {
            await CardPileCmd.Add(
                card,
                PileType.Draw);
        }

        await CardPileCmd.Shuffle(
            choiceContext,
            Owner);

        var drawnCards = await CardPileCmd.Draw(
            choiceContext,
            DynamicVars.Cards.BaseValue,
            Owner);

        foreach (var drawnCard in drawnCards)
        {
            if (drawnCard is IEidolonCard && !drawnCard.IsUpgraded)
                CardCmd.Upgrade(drawnCard);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(2m);
    }
}