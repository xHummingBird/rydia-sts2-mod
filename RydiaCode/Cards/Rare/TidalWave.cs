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

public class TidalWave() : RydiaCard(2, CardType.Attack,
    CardRarity.Rare, TargetType.AllEnemies), IEidolonCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
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
        var leftMostEnemy = enemies.First();
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Rydia rydia)
        {
            AudioHelper.PlayRandomSummon();
            rydia.PlayAnimation(ownerCreature, "leviathan");
            SfxCmd.Play("res://Rydia/sfx/summon_cast.mp3", 1.1f);
            SfxCmd.Play("res://Rydia/sfx/water_2.wav");
            await Task.Delay(700);
            rydia.PlayVfxOnTarget(
                leftMostEnemy, 
                "res://Rydia/scenes/vfx.tscn", 
                "tidal_wave");
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Rydia/sfx/tidal_wave.mp3")
            .Execute(choiceContext);
        await Task.Delay(1400);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
    }
}