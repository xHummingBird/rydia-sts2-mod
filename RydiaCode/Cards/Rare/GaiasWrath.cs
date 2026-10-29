using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;
using Rydia.RydiaCode.Powers;

namespace Rydia.RydiaCode.Cards.Rare;

public class GaiasWrath() : RydiaCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.AllEnemies), IEidolonCard
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<EidolonResonancePower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13m, ValueProp.Move),
        new PowerVar<GaiasWrathPower>(10),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon,
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var enemies = CombatState.HittableEnemies.ToList();
        var leftMostEnemy = enemies.First();
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Rydia rydia)
        {
            AudioHelper.PlayRandomSummon();
            rydia.PlayAnimation(ownerCreature, "summon_cast");
            SfxCmd.Play("res://Rydia/sfx/summon_cast.mp3", 1.1f);
            rydia.PlayVfxBehindTarget(
                leftMostEnemy, 
                "res://Rydia/scenes/vfx.tscn", 
                "titan");
            await Task.Delay(133);
            SfxCmd.Play("res://Rydia/sfx/from_tifa/final_heaven_hit.wav");
            await Task.Delay(467);
            SfxCmd.Play("res://Rydia/sfx/from_tifa/final_heaven_swing.wav");
            await Task.Delay(200);
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Rydia/sfx/from_tifa/meteostrike_1.wav")
            .Execute(choiceContext);
        var powerAmount = DynamicVars["GaiasWrathPower"].BaseValue;
        if (Owner.HasPower<EidolonResonancePower>() || Owner.HasPower<EvokePower>())
            powerAmount *= 2;
        await PowerCmd.Apply<GaiasWrathPower>(choiceContext, CombatState.HittableEnemies, powerAmount, Owner.Creature, this);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["GaiasWrathPower"].UpgradeValueBy(2);
    }
}