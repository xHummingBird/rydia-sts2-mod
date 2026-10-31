using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Relics;

public class StardustRod : RydiaRelic
{
    public override RelicRarity Rarity =>
        RelicRarity.Rare;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon
    ];

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature) &&
            Owner.PlayerCombatState.TurnNumber <= 1)
        {
            Flash();

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
                return;

            eidolon.SetToFreeThisTurn();

            await CardPileCmd.AddGeneratedCardToCombat(
                eidolon,
                PileType.Hand,
                Owner);
        }
    }
}