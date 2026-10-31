using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Rydia.RydiaCode.Character;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Cards.Rare;

public class InvocationConvocation() : RydiaCard(
    1,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        CardModel? eidolon = CardFactory.GetDistinctForCombat(
                Owner,
                ModelDb.CardPool<RydiaCardPool>()
                    .AllCards
                    .Where(card => card is IEidolonCard),
                1,
                Owner.RunState.Rng.CombatCardGeneration)
            .FirstOrDefault();

        if (eidolon == null)
            return;

        if (IsUpgraded)
            CardCmd.Upgrade(eidolon);

        eidolon.AddKeyword(CardKeyword.Exhaust);

        await CardCmd.AutoPlay(
            choiceContext,
            eidolon, null);
    }

    protected override void OnUpgrade()
    {
    }
}