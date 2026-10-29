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

namespace Rydia.RydiaCode.Cards.Rare;

public class Megaflare() : RydiaCard(1, CardType.Attack,
    CardRarity.Rare, TargetType.AllEnemies), IEidolonCard
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<EidolonResonancePower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(30m, ValueProp.Move),
        new PowerVar<MegaflarePower>(3),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        RydiaStaticHoverTips.Eidolon,
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var enemies = CombatState.HittableEnemies.ToList();
        var leftMostEnemy = enemies.First();
        
        if (ownerCreature != null && Owner?.Character is Character.Rydia rydia && (Owner.HasPower<MegaflarePower>() || Owner.HasPower<EvokePower>()))
        {
            CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
            AudioHelper.PlayRandomSummon();
            rydia.PlayAnimation(Owner.Creature, "summon_cast");
            SfxCmd.Play("res://Rydia/sfx/summon_cast.mp3", 1.1f);
            rydia.PlayVfxOnTarget(leftMostEnemy, "res://Rydia/scenes/vfx.tscn", "bahamut");
            SfxCmd.Play("res://Rydia/sfx/from_tifa/final_heaven_1.wav");
            await Task.Delay(733);
            SfxCmd.Play("res://Rydia/sfx/from_tifa/meteostrike_3.wav");
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null)
                .BeforeDamage(async delegate
                {
                    var targets = base.CombatState.HittableEnemies;
                    foreach (var target in targets)
                    {
                        var vfx = NGroundFireVfx.Create(target, VfxColor.Red);
                        if (vfx != null)
                        {
                            NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(vfx);
                        }
                    }
                })
                .Execute(choiceContext);
            await Task.Delay(1000);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        }
        else 
            await PowerCmd.Apply<MegaflarePower>(choiceContext, Owner.Creature, DynamicVars["MegaflarePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MegaflarePower"].UpgradeValueBy(-1);
    }
}