using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;
using Rydia.RydiaCode.Powers;

namespace Rydia.RydiaCode.Cards.Uncommon;

public class Hellfire() : RydiaCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy), IEidolonCard
{
    public override TargetType TargetType
    {
        get
        {
            if (Owner.HasPower<EidolonResonancePower>() || Owner.HasPower<EvokePower>())
                return TargetType.AllEnemies;
            return TargetType.AnyEnemy;
        }
    }
    
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<EidolonResonancePower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
        new PowerVar<BurnPower>(5)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon,
        HoverTipFactory.FromPower<BurnPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var enemies = CombatState.HittableEnemies.ToList();
        var powerAmount = DynamicVars["BurnPower"].BaseValue;
        if (Owner.HasPower<EidolonResonancePower>()) powerAmount *= 2;
        var middleEnemy = enemies.First();
        if (enemies.Count > 1)
        {
            int middleIndex = (enemies.Count - 1) / 2;
            middleEnemy = enemies[middleIndex];
        }

        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Rydia rydia)
        {
            AudioHelper.PlayRandomSummon();
            rydia.PlayAnimation(ownerCreature, "summon_cast");
            SfxCmd.Play("res://Rydia/sfx/summon_cast.mp3", 1.1f);
            await Task.Delay(400);
            if (Owner.HasPower<EidolonResonancePower>())
            {
                rydia.PlayVfxOnTarget(
                    middleEnemy,
                    "res://Rydia/scenes/vfx.tscn",
                    "hellfire");
            }
            else
            {
                rydia.PlayVfxOnTarget(
                    play.Target,
                    "res://Rydia/scenes/vfx.tscn",
                    "ifrit");
                middleEnemy = play.Target;
            }

            await Task.Delay(133);
            RydiaExtensions.CombatHelpers.RydiaFakeHit(Owner.Creature, middleEnemy,
                "res://Rydia/sfx/from_killia/fire_punch.wav", playHitVfx: false);
            await Task.Delay(234);
            RydiaExtensions.CombatHelpers.RydiaFakeHit(Owner.Creature, middleEnemy,
                "res://Rydia/sfx/from_killia/fire_punch_2.wav", playHitVfx: false);
            await Task.Delay(166);
            RydiaExtensions.CombatHelpers.RydiaFakeHit(Owner.Creature, middleEnemy,
                "res://Rydia/sfx/from_killia/fire_punch.wav", playHitVfx: false);
            await Task.Delay(167);
            if (Owner.HasPower<EidolonResonancePower>())
            {
                await Task.Delay(33);
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, play).TargetingAllOpponents(base.CombatState)
                    .WithHitFx(null, "res://Rydia/sfx/from_tifa/meteostrike_1.wav")
                    .Execute(choiceContext);
                await Task.Delay(167);
            }
            else
            {
                SfxCmd.Play("res://Rydia/sfx/from_killia/fire_impact (2).wav");
                await Task.Delay(167);
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, play).Targeting(play.Target)
                    .Execute(choiceContext);
            }
            await Task.Delay(300);

        }
        else  
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, play).Targeting(play.Target)
                .Execute(choiceContext);
        if (Owner.HasPower<EidolonResonancePower>() || Owner.HasPower<EvokePower>())
            await PowerCmd.Apply<BurnPower>(choiceContext, CombatState.HittableEnemies, powerAmount, Owner.Creature, this);
        else 
            await PowerCmd.Apply<BurnPower>(choiceContext, play.Target, powerAmount, Owner.Creature, this);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
        DynamicVars["BurnPower"].UpgradeValueBy(2);
    }
}