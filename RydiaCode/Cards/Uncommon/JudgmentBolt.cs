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

public class JudgmentBolt() : RydiaCard(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.RandomEnemy), IEidolonCard
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<EidolonResonancePower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move),
        new PowerVar<ShockPower>(3),
        new RepeatVar(1)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon,
        HoverTipFactory.FromPower<ShockPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        Character.Rydia? rydia =
            base.Owner.Character as Character.Rydia;
        if (ownerCreature != null && rydia != null)
        {
            AudioHelper.PlayRandomSummon();
            rydia.PlayAnimation(ownerCreature, "ramuh");
            SfxCmd.Play("res://Rydia/sfx/summon_cast.mp3", 1.1f);
            SfxCmd.Play("res://Rydia/sfx/from_killia/charge_thunder (3).wav");
            await Task.Delay(700);
        }
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCard(this, play)
            .TargetingRandomOpponents(base.CombatState)
            .WithHitVfxNode(target =>
            {
                if (rydia == null)
                {
                    return null;
                }
                
                rydia.PlayVfxOnTarget(
                    target,
                    "res://Rydia/scenes/vfx.tscn",
                    "ramuh_thunder"
                );
                SfxCmd.Play("res://Rydia/sfx/from_killia/charged_thunder (2).wav");
                return null;
            })
            .Execute(choiceContext);
        if (Owner.HasPower<EidolonResonancePower>() || Owner.HasPower<EvokePower>())
            await PowerCmd.Apply<ShockPersistencePower>(choiceContext, CombatState.HittableEnemies, 2, Owner.Creature, this);
        await PowerCmd.Apply<ShockPower>(choiceContext, CombatState.HittableEnemies, DynamicVars["ShockPower"].BaseValue, Owner.Creature, this);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
        DynamicVars["ShockPower"].UpgradeValueBy(1);
    }
}