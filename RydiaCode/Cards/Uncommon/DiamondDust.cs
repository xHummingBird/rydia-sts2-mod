using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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

public class DiamondDust() : RydiaCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AllEnemies), IEidolonCard
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<EidolonResonancePower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(11m, ValueProp.Move),
        new PowerVar<FreezePower>(2),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon,
        HoverTipFactory.FromPower<FreezePower>()
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
            {
                rydia.PlayVfxOnTarget(
                    CombatState.HittableEnemies.First(),
                    "res://Rydia/scenes/vfx.tscn",
                    "shiva");
            }
            SfxCmd.Play("res://Rydia/sfx/wind.wav", 1.1f);
            await Task.Delay(467);
            foreach (var enemy in CombatState.HittableEnemies)
                rydia.PlayVfxOnTarget(
                    enemy,
                    "res://Rydia/scenes/vfx.tscn",
                    "diamond_dust");
            SfxCmd.Play("res://Rydia/sfx/ice.wav");
            await Task.Delay(467);
            SfxCmd.Play("res://Rydia/sfx/ice_2.wav");
            await Task.Delay(133);
        }
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, play).TargetingAllOpponents(base.CombatState)
            .BeforeDamage(async delegate
            {
                var targets = base.CombatState.HittableEnemies;
                foreach (var target in targets)
                {
                    var vfx = NGroundFireVfx.Create(target, VfxColor.Cyan);
                    if (vfx != null)
                    {
                        NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                    }
                }
            })
            .Execute(choiceContext);
        if (Owner.HasPower<EidolonResonancePower>())
            await PowerCmd.Apply<DiamondDustPower>(choiceContext, CombatState.HittableEnemies, DynamicVars["FreezePower"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<FreezePower>(choiceContext, CombatState.HittableEnemies, DynamicVars["FreezePower"].BaseValue, Owner.Creature, this);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["FreezePower"].UpgradeValueBy(1);
    }
}