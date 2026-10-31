using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Powers;

namespace Rydia.RydiaCode.Cards.Uncommon;

public class MindBlast() : RydiaCard(
    1,
    CardType.Attack,
    CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    private const string CalculatedHits = "CalculatedHits";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar(CalculatedHits).WithMultiplier(
            (CardModel card, Creature? _) =>
            {
                var cardsInHand =
                    PileType.Hand.GetPile(card.Owner).Cards;

                bool hasResonance =
                    card.Owner.Creature.HasPower<EidolonResonancePower>() ||
                    card.Owner.Creature.HasPower<EvokePower>();

                if (hasResonance)
                    return cardsInHand.Count;

                return cardsInHand.Count(
                    handCard =>
                        handCard.Type == CardType.Skill);
            })
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        int hitCount =
            (int)((CalculatedVar)DynamicVars[CalculatedHits])
                .Calculate(play.Target);
        var ownerCreature = Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Rydia rydia)
        {
            AudioHelper.PlayRandomSummon();
            rydia.PlayAnimation(ownerCreature, "summon_cast");
            SfxCmd.Play("res://Rydia/sfx/summon_cast.mp3", 1.1f);
            await Task.Delay(300);
            rydia.PlayVfxOnTarget(
                CombatState.HittableEnemies.First(), 
                "res://Rydia/scenes/vfx.tscn", 
                "mindflayer");
            await Task.Delay(200);
            SfxCmd.Play("res://Rydia/sfx/from_tifa/chi_sfx.wav");
        }
        
        await DamageCmd.Attack(
                DynamicVars.Damage.BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this, play)
            .Targeting(play.Target)
            .Execute(choiceContext);
        await Task.Delay(300);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}