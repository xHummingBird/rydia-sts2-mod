using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;
using Rydia.RydiaCode.Powers;

namespace Rydia.RydiaCode.Cards.Uncommon;

public class MagicBarrage() : RydiaCard(3, CardType.Attack,
    CardRarity.Uncommon, TargetType.AllEnemies), IMagicCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(21m, ValueProp.Move)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromPower<FreezePower>(),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Rydia rydia)
        {
            AudioHelper.PlayRandomMagic();
            SfxCmd.Play("res://Rydia/sfx/magic_cast.mp3", 1.1f);
            rydia.PlayAnimation(ownerCreature, "cast");
            await Task.Delay(200);
            foreach (var enemy in CombatState.HittableEnemies)
            {
                rydia.PlayVfxOnTarget(
                    enemy,
                    "res://Rydia/scenes/vfx.tscn",
                    "magic_barrage");
            }
            SfxCmd.Play("res://Rydia/sfx/from_killia/fire_impact (2).wav");
            await Task.Delay(300);
            foreach (var enemy in CombatState.HittableEnemies)
                RydiaExtensions.CombatHelpers.RydiaFakeHit(ownerCreature, enemy, playHitVfx: false);
            await Task.Delay(500);
            SfxCmd.Play("res://Rydia/sfx/ice.wav");
            await Task.Delay(100);
            foreach (var enemy in CombatState.HittableEnemies)
                RydiaExtensions.CombatHelpers.RydiaFakeHit(ownerCreature, enemy, playHitVfx: false);
            await Task.Delay(300);
        }
        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Rydia/sfx/from_killia/charged_thunder (2).wav")
            .Execute(choiceContext);

        foreach (var enemy in CombatState.HittableEnemies)
        {
            var burnPower = enemy.GetPowerAmount<BurnPower>();
            var freezePower = enemy.GetPowerAmount<FreezePower>();
            if (burnPower > 0)
                await PowerCmd.Apply<BurnPower>(choiceContext, enemy,
                    burnPower, Owner.Creature, this);
            if (freezePower > 0)
                await PowerCmd.Apply<FreezePower>(choiceContext, enemy, freezePower, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}