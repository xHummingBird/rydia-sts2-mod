using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Cards.Common;

public class WhipDance() : RydiaCard(2, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
        new RepeatVar(2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Rydia rydia)
        {
            AudioHelper.PlayRandomAttack();
            SfxCmd.Play("res://Rydia/sfx/whip_swing.mp3", 1.2f);
            rydia.DashTo(ownerCreature, play.Target, distance: 300f, durationSeconds: 0.2F, overrideAnim: "attack");
            await Task.Delay(83);
            SfxCmd.Play("res://Rydia/sfx/whip_hit1.mp3", 1.1f);
            await Task.Delay(17);
            rydia.PlayVfxOnTarget(play.Target, "res://Rydia/scenes/vfx.tscn", "whip_hit");
            await CommonActions.CardAttack(this, play.Target, 2)
                .WithHitFx(null)
                .Execute(choiceContext);
            await rydia.Retreat(ownerCreature, duration: 0.25F);
        }
        else  await CommonActions.CardAttack(this, play.Target, 2)
            .Execute(choiceContext);
        
        CardModel? cardModel = PileType.Draw.GetPile(base.Owner).Cards.Where((CardModel c) => c.Type == CardType.Attack && !c.Keywords.Contains(CardKeyword.Unplayable)).ToList().StableShuffle(base.Owner.RunState.Rng.Shuffle)
            .FirstOrDefault();
        if (cardModel == null)
        {
            cardModel = PileType.Draw.GetPile(base.Owner).Cards.Where((CardModel c) => c.Type == CardType.Attack).ToList().StableShuffle(base.Owner.RunState.Rng.Shuffle)
                .FirstOrDefault();
        }
        if (cardModel != null)
        {
            if (IsUpgraded)
            {
                CardCmd.Upgrade(cardModel);
            }
            
            await CardCmd.AutoPlay(choiceContext, cardModel, null);
        }

    }

    protected override void OnUpgrade()
    {
    }
}