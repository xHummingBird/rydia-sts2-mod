using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Powers;

public class FeymarchsBlessingPower : RydiaPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon
    ];

    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        if (card.Owner.Creature != Owner)
            return;

        if (card is not IEidolonCard)
            return;

        Flash();

        await CardPileCmd.Draw(
            choiceContext,
            Amount,
            Owner.Player);
    }
}