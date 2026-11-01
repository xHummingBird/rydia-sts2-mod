using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;
using Rydia.RydiaCode.Powers;

namespace Rydia.RydiaCode.Cards.Basic;

public class MistBreath() : RydiaCard(2, CardType.Attack,
    CardRarity.Basic, TargetType.AnyEnemy), IEidolonCard
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<EidolonResonancePower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(14m),
        new ExtraDamageVar(5m),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((CardModel card, Creature? target) =>
            {
                if (!card.Owner.Creature.HasPower<EidolonResonancePower>() &&
                    !card.Owner.Creature.HasPower<EvokePower>())
                    return 0;

                return target?.Powers.Count(ShouldCountPower) ?? 0;
            })
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon,
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Rydia rydia)
        {
            AudioHelper.PlayRandomSummon();
            rydia.PlayAnimation(ownerCreature, "summon_cast");
            SfxCmd.Play("res://Rydia/sfx/summon_cast.mp3", 1.1f);
            await Task.Delay(500);
            rydia.PlayVfxOnTarget(
                play.Target, 
                "res://Rydia/scenes/vfx.tscn", 
                "mist_breath");
            SfxCmd.Play("res://Rydia/sfx/mist_breath.mp3");
            await Task.Delay(800);
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Rydia/sfx/ice.wav")
            .BeforeDamage(async delegate
            {
                var vfx = NGroundFireVfx.Create(play.Target, VfxColor.White);
                    if (vfx != null)
                    {
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                        SfxCmd.Play("event:/sfx/characters/attack_fire");
                    }
            })
            .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(4m);
        DynamicVars.ExtraDamage.UpgradeValueBy(3m);
    }
    
    private static bool ShouldCountPower(PowerModel power)
    {
        if (power.TypeForCurrentAmount == PowerType.Debuff)
        {
            return !(power is ITemporaryPower);
        }
        return false;
    }
}