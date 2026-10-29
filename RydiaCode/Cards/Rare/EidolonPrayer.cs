using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Cards.Rare;

public class EidolonPrayer() : RydiaCard(
    0,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon,
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var eidolonCards = GetEidolonCards().ToList();
        int exhaustedCount = eidolonCards.Count;

        foreach (var card in eidolonCards)
        {
            await CardCmd.Exhaust(
                choiceContext,
                card);
        }

        for (int i = 0; i < exhaustedCount; i++)
        {
            CardModel? eidolon = CardFactory.GetDistinctForCombat(
                    Owner,
                    Owner.Character.CardPool
                        .GetUnlockedCards(
                            Owner.UnlockState,
                            Owner.RunState.CardMultiplayerConstraint)
                        .Where(card => card is IEidolonCard),
                    1,
                    Owner.RunState.Rng.CombatCardGeneration)
                .FirstOrDefault();

            if (eidolon == null)
                continue;

            if (IsUpgraded)
                CardCmd.Upgrade(eidolon);

            await CardPileCmd.AddGeneratedCardToCombat(
                eidolon,
                PileType.Hand,
                Owner);
        }
    }

    protected override void OnUpgrade()
    {
    }

    private IEnumerable<CardModel> GetEidolonCards()
    {
        CardPile hand = PileType.Hand.GetPile(Owner);

        return hand.Cards.Where(card => card is IEidolonCard);
    }
}