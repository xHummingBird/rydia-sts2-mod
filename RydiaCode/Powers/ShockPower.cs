using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Cards.Uncommon;

namespace Rydia.RydiaCode.Powers;

public class ShockPower : RydiaPower
{
    private class Data
    {
        public CardPlay? CardPlayToTrack;

        public int AmountBeforeCardPlayed;

        public bool PersistenceChecked;

        public bool HadShockPersistenceBeforeAttack;

        public bool ShockTriggered;
    }

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task BeforeCardPlayed(
        CardPlay cardPlay)
    {
        Data data = GetInternalData<Data>();

        data.CardPlayToTrack = cardPlay;
        data.AmountBeforeCardPlayed = Amount;
        data.PersistenceChecked = false;
        data.HadShockPersistenceBeforeAttack = false;
        data.ShockTriggered = false;
        
        return Task.CompletedTask;
    }

    public override Task BeforeAttack(AttackCommand command)
    {
        if (!command.DamageProps.IsPoweredAttack())
            return Task.CompletedTask;

        Data data = GetInternalData<Data>();

        if (data.CardPlayToTrack == null)
            return Task.CompletedTask;

        if (command.ModelSource != data.CardPlayToTrack.Card)
            return Task.CompletedTask;

        if (data.PersistenceChecked)
            return Task.CompletedTask;

        data.PersistenceChecked = true;
        data.HadShockPersistenceBeforeAttack =
            Owner.GetPower<ShockPersistencePower>() != null;

        return Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner)
            return 0m;

        if (!props.IsPoweredAttack())
            return 0m;

        Data data = GetInternalData<Data>();

        if (data.CardPlayToTrack == null)
            return 0m;

        if (cardPlay != data.CardPlayToTrack)
            return 0m;

        if (data.AmountBeforeCardPlayed <= 0)
            return 0m;

        if (target.HasPower<BurnPower>())
            return data.AmountBeforeCardPlayed * 2;
        
        return data.AmountBeforeCardPlayed;
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner)
            return Task.CompletedTask;

        if (!props.IsPoweredAttack())
            return Task.CompletedTask;

        Data data = GetInternalData<Data>();

        if (data.CardPlayToTrack == null)
            return Task.CompletedTask;

        if (cardSource != data.CardPlayToTrack.Card)
            return Task.CompletedTask;

        data.ShockTriggered = true;

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        Data data = GetInternalData<Data>();
        
        if (cardPlay != data.CardPlayToTrack)
            return;

        int amountToRemove = data.AmountBeforeCardPlayed;
        bool shockTriggered = data.ShockTriggered;
        bool hadShockPersistenceBeforeAttack =
            data.HadShockPersistenceBeforeAttack;

        data.CardPlayToTrack = null;
        data.AmountBeforeCardPlayed = 0;
        data.PersistenceChecked = false;
        data.HadShockPersistenceBeforeAttack = false;
        data.ShockTriggered = false;

        if (!shockTriggered)
            return;

        if (hadShockPersistenceBeforeAttack)
            return;

        amountToRemove = Math.Min(amountToRemove, Amount);

        if (amountToRemove <= 0)
            return;

        await PowerCmd.Apply<ConductivePower>(choiceContext, Owner, 1, null, null);
        
        if (cardPlay.Card is Thundaga)
            return;

        await PowerCmd.ModifyAmount(
            choiceContext,
            this,
            -amountToRemove,
            null,
            null);
    }
}