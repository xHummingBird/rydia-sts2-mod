using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Cards.Common;

public class EidolonArmor() : RydiaCard(
    1,
    CardType.Skill,
    CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5, ValueProp.Move),
        new CardsVar(1),
        new DynamicVar("BlockNextTurn", 5m)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardBlock(this, play);

        var drawnCards = await CardPileCmd.Draw(
            choiceContext,
            1,
            Owner);
        
        var drawnCard = drawnCards.FirstOrDefault();

        if (drawnCard is IEidolonCard)
            await PowerCmd.Apply<BlockNextTurnPower>(
                choiceContext,
                Owner.Creature,
                DynamicVars["BlockNextTurn"].BaseValue,
                Owner.Creature,
                this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["BlockNextTurn"].UpgradeValueBy(2m);
    }
}