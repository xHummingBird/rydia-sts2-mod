using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Rydia.RydiaCode.Cards.Rare;

public class RapidMagic() : RydiaCard(
    2,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        foreach (var card in PileType.Hand.GetPile(Owner).Cards)
        {
            if (card.Type == CardType.Attack && !card.EnergyCost.CostsX)
                card.SetToFreeThisTurn();
        }

        await PowerCmd.Apply<NoBlockPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}