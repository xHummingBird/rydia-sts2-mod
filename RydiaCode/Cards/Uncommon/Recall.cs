using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Rydia.RydiaCode.Cards.Uncommon;

public class Recall() : RydiaCard(
    1,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        CardModel? attack = CardFactory.GetDistinctForCombat(
                Owner,
                Owner.Character.CardPool
                    .GetUnlockedCards(
                        Owner.UnlockState,
                        Owner.RunState.CardMultiplayerConstraint)
                    .Where(card => card.Type == CardType.Attack),
                1,
                Owner.RunState.Rng.CombatCardGeneration)
            .FirstOrDefault();

        if (attack != null)
        {
            attack.SetToFreeThisTurn();

            await CardPileCmd.AddGeneratedCardToCombat(
                attack,
                PileType.Hand,
                Owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}