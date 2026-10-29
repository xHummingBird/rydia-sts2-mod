using BaseLib.Extensions;
using BaseLib.Utils;
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

namespace Rydia.RydiaCode.Cards.Common;

public class SelfDestruct() : RydiaCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AllEnemies), IEidolonCard
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<EidolonResonancePower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon,
        HoverTipFactory.FromPower<BurnPower>(),
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
            await Task.Delay(300);
            rydia.PlayVfxOnTarget(
                CombatState.HittableEnemies.First(), 
                "res://Rydia/scenes/vfx.tscn", 
                "bomb");
            SfxCmd.Play("res://Rydia/sfx/self_destruct.mp3", 1.1f);
            await Task.Delay(267);
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null)
            .Execute(choiceContext);
        if (Owner.HasPower<EidolonResonancePower>() || Owner.HasPower<EvokePower>())
            foreach (var enemy in CombatState.HittableEnemies)
            {
                BurnPower power = enemy.GetPower<BurnPower>();
                if (power != null)
                {
                    await power.Trigger();
                }
            }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}