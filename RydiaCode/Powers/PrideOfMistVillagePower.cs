using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using Rydia.RydiaCode.Character;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Powers;

public class PrideOfMistVillagePower : RydiaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player != Owner.Player)
        {
            return;
        }

        IReadOnlyList<CardModel> eidolonCards = ModelDb
            .CardPool<RydiaCardPool>()
            .AllCards
            .Where(card => card is IEidolonCard)
            .ToList();

        if (eidolonCards.Count == 0)
        {
            return;
        }

        CardModel[] cards = new CardModel[Amount];

        Rng combatCardGeneration =
            Owner.Player.RunState.Rng.CombatCardGeneration;

        for (int i = 0; i < Amount; i++)
        {
            cards[i] = CardFactory.GetDistinctForCombat(
                player,
                eidolonCards,
                1,
                combatCardGeneration
            ).First();

            CardCmd.ApplyKeyword(
                cards[i],
                CardKeyword.Exhaust
            );
        }

        Flash();

        await CardPileCmd.AddGeneratedCardsToCombat(
            cards,
            PileType.Hand,
            Owner.Player
        );
    }
}