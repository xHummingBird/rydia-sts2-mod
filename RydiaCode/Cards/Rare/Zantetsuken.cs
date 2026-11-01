using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;
using Rydia.RydiaCode.Powers;

namespace Rydia.RydiaCode.Cards.Rare;

public class Zantetsuken() : RydiaCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy), IEidolonCard
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<EidolonResonancePower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20m, ValueProp.Move),
        new DynamicVar("HpThreshold", 10)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon,
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        decimal threshold = DynamicVars["HpThreshold"].BaseValue;
        if (ownerCreature != null && Owner?.Character is Character.Rydia rydia)
        {
            AudioHelper.PlayRandomSummon();
            rydia.PlayAnimation(ownerCreature, "summon_cast");
            SfxCmd.Play("res://Rydia/sfx/summon_cast.mp3", 1.1f);
            rydia.PlayVfxOnTarget(
                play.Target, 
                "res://Rydia/scenes/vfx.tscn", 
                "odin_vfx");
            await Task.Delay(467);
            SfxCmd.Play("res://Rydia/sfx/zantetsuken_2.wav");
            await Task.Delay(267);
            SfxCmd.Play("res://Rydia/sfx/zantetsuken_2.wav");
            await Task.Delay(266);
            SfxCmd.Play("res://Rydia/sfx/zantetsuken_2.wav");
            await Task.Delay(233);
            SfxCmd.Play("res://Rydia/sfx/sword_swing_heavy.wav");
            
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Rydia/sfx/zantetsuken_1.wav")
            .Execute(choiceContext);
        await Task.Delay(1000);
        if (Owner.HasPower<EidolonResonancePower>() || Owner.HasPower<EvokePower>())
            if (play.Target.CurrentHp * 100 <= play.Target.MaxHp * threshold && play.Target.CurrentHp > 0)
            {
                await DoomPower.DoomKill(new List<Creature> { play.Target });
                return;
            }
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["HpThreshold"].UpgradeValueBy(2);
    }
}