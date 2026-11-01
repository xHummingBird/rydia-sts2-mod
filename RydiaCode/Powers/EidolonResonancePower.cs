using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;

namespace Rydia.RydiaCode.Powers;

public class EidolonResonancePower : RydiaPower
{
    public override PowerType Type =>
        PowerType.Buff;

    public override PowerStackType StackType =>
        PowerStackType.Single;
    
    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var ownerCreature = Owner;
        var rydia = Owner?.Player.Character as Character.Rydia;
        if (ownerCreature != null && rydia != null)
        {
           AudioHelper.PlayRandomSummon();
           SfxCmd.Play("res://Rydia/sfx/summon_cast.mp3", 1.1f);
           float duration = rydia.PlayAnimation(ownerCreature, "resonance").total;
           await Task.Delay((int)(0.9f * 1000f));
           rydia.PlayAnimation(ownerCreature, "idle_resonance");
        }
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (!props.IsPoweredAttack())
            return 1m;

        if (dealer != Owner)
            return 1m;

        if (cardSource is not IEidolonCard)
            return 1m;

        return 2m;
    }
}