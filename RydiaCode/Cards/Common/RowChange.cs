using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Rydia.RydiaCode.Cards.Common;

public class RowChange() : RydiaCard(
    0,
    CardType.Skill,
    CardRarity.Common,
    TargetType.Self)
{
    private bool HasPlayedTwoAttacksThisTurn =>
        CombatManager.Instance.History.CardPlaysFinished.Count(
            entry =>
                entry.HappenedThisTurn(CombatState) &&
                entry.CardPlay.Card.Owner == Owner &&
                entry.CardPlay.Card.Type == CardType.Attack) >= 2;

    protected override bool ShouldGlowGoldInternal =>
        HasPlayedTwoAttacksThisTurn;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(4, ValueProp.Move),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CommonActions.CardBlock(this, play);

        if (HasPlayedTwoAttacksThisTurn)
        {
            await CommonActions.CardBlock(this, play);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
    }
}