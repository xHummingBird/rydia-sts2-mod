using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Cards.Common;

public class MagicBarrier() : RydiaCard(
    1,
    CardType.Skill,
    CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5, ValueProp.Move)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardBlock(this, play);

        CardModel exhaustedCard = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(
                CardSelectorPrefs.ExhaustSelectionPrompt,
                1),
            context: choiceContext,
            player: Owner,
            filter: null,
            source: this)).FirstOrDefault();

        if (exhaustedCard != null)
        {
            await CardCmd.Exhaust(
                choiceContext,
                exhaustedCard);

            if (exhaustedCard is IEidolonCard)
                await CommonActions.CardBlock(this, play);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}