using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Cards.Common;

public class BurstStorm() : RydiaCard(2, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy), IMagicCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move),
        new EnergyVar(1)
    ];
    
    private bool HasPlayedEidolonCardThisTurn =>
        CombatManager.Instance.History.CardPlaysFinished.Any(
            entry =>
                entry.HappenedThisTurn(CombatState) &&
                entry.CardPlay.Card.Owner == Owner &&
                entry.CardPlay.Card is IEidolonCard);
    
    protected override bool ShouldGlowGoldInternal => HasPlayedEidolonCardThisTurn;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Rydia rydia)
        {
            AudioHelper.PlayRandomMagic();
            SfxCmd.Play("res://Rydia/sfx/magic_cast.mp3", 1.1f);
            rydia.PlayAnimation(ownerCreature, "cast");
            await Task.Delay(200);
            SfxCmd.Play("res://Rydia/sfx/wind.wav", 1.2f);
            
            rydia.PlayVfxOnTarget(play.Target, "res://Rydia/scenes/vfx.tscn", "tornado");
            await Task.Delay(267);
            SfxCmd.Play("res://Rydia/sfx/tornado_hit.wav");
            rydia.PlayVfxOnTarget(
                play.Target,
                "res://Rydia/scenes/vfx.tscn",
                "tornado_hit"
            );
            await Task.Delay(267);
            SfxCmd.Play("res://Rydia/sfx/tornado_hit.wav");
            rydia.PlayVfxOnTarget(
                play.Target,
                "res://Rydia/scenes/vfx.tscn",
                "tornado_hit"
            );
            await Task.Delay(267);
            rydia.PlayVfxOnTarget(
                play.Target,
                "res://Rydia/scenes/vfx.tscn",
                "tornado_hit"
            );
            rydia.PlayAnimation(ownerCreature, "idle");
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Rydia/sfx/tornado_hit.wav")
            .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
    
    public override Task AfterCardEnteredCombat(
        CardModel card)
    {
        if (card != this)
            return Task.CompletedTask;

        if (HasPlayedEidolonCardThisTurn)
        {
            EnergyCost.SetThisTurn(0);
        }

        return Task.CompletedTask;
    }
    
    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
            return Task.CompletedTask;

        if (cardPlay.Card is IEidolonCard)
        {
            EnergyCost.SetThisTurn(0);
        }

        return Task.CompletedTask;
    }
}