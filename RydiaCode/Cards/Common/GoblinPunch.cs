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

public class GoblinPunch() : RydiaCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy), IEidolonCard
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<EidolonResonancePower>();
    
    private const string MinimumDamage = "MinimumDamage";
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(16m, ValueProp.Move),
        new DynamicVar(MinimumDamage, 1m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon,
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        
        int maximumDamage = DynamicVars.Damage.IntValue;
        int finalDamage;
        
        finalDamage = RollRandomDamage();

        if (Owner.HasPower<EidolonResonancePower>())
            finalDamage = maximumDamage;
        
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Rydia rydia)
        {
            AudioHelper.PlayRandomSummon();
            rydia.PlayAnimation(ownerCreature, "summon_cast");
            SfxCmd.Play("res://Rydia/sfx/summon_cast.mp3", 1.1f);
            await Task.Delay(400);
            rydia.PlayVfxOnTarget(
                play.Target, 
                "res://Rydia/scenes/vfx.tscn", 
                "goblin_punch");
            await Task.Delay(233);
        }
        await DamageCmd.Attack(finalDamage)
            .FromCard(this, play)
            .WithHitFx(null, "res://Rydia/sfx/from_tifa/punch_critical.wav")
            .WithValueProp(ValueProp.Move)
            .Targeting(play.Target)
            .Execute(choiceContext);
        await Task.Delay(400);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars[MinimumDamage].UpgradeValueBy(2m);
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
    
    private int RollRandomDamage()
    {
        int minimumDamage = DynamicVars[MinimumDamage].IntValue;
        int maximumDamage = DynamicVars.Damage.IntValue;

        return Owner.RunState.Rng.CombatCardSelection.NextInt(
            minimumDamage,
            maximumDamage + 1);
    }
}