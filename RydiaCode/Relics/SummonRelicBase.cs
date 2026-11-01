using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using Rydia.RydiaCode.Cards;
using Rydia.RydiaCode.Cards.Basic;
using Rydia.RydiaCode.Extensions;
using Rydia.RydiaCode.Mechanics;
using Rydia.RydiaCode.Powers;

namespace Rydia.RydiaCode.Relics;

public abstract class SummonRelicBase : RydiaRelic
{
    [SavedProperty]
    public int StoredSummon { get; internal set; }

    public virtual int MaxSummon => 100;

    protected virtual int SummonPerTurn => 5;

    protected virtual int SummonPerAttack => 5;

    protected virtual int SummonPerEidolonCard => 8;

    public override bool ShowCounter => false;

    public override Task BeforeCombatStart()
    {
        SummonManager.ResetSummon(Owner);
        
        Status = RelicStatus.Normal;
        var room = Owner.RunState.CurrentRoom;
        SummonManager.ResetSummon(Owner);
        BattleTheme.Play(
            RunManager.Instance.NetService.NetId,
            Owner.RunState.CurrentActIndex + 1,
            room.RoomType);

        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStartLate(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!CombatManager.Instance.IsInProgress)
            return;

        if (Owner?.Creature == null)
            return;

        if (side != Owner.Creature.Side)
            return;

        if (Owner.Creature.IsDead)
            return;

        int amount = CalculateTurnStartGain();

        amount = ModifyTurnStartSummonGain(amount);

        GainSummon(amount);

        await SummonManager.CheckSummonReady(
            Owner.Creature,
            null,
            Owner.Creature,
            null);
    }

    public override async Task AfterCombatVictory(
        CombatRoom room)
    {
        Creature? creature = Owner?.Creature;
    
        if (creature == null)
            return;
    
        // Only the client controlling Tidus runs local visuals/audio.
        if (!LocalContext.IsMe(creature))
            return;
        
        // Only boss combats play the fanfare and delay Tidus's rewards.
        bool isBoss =
            room.RoomType == RoomType.Boss;
        
        BattleTheme.PlayVictoryFanfare(
            RunManager.Instance.NetService.NetId,
            isBoss);
    }

    public override async Task AfterCardPlayed(
    PlayerChoiceContext choiceContext,
    CardPlay cardPlay)
    {
        if (!CombatManager.Instance.IsInProgress)
            return;

        CardModel? card = cardPlay.Card;

        if (card == null)
            return;

        if (card.Owner != Owner)
            return;

        int amount = 0;

        Character.Rydia? rydia = Owner.Character as Character.Rydia;

        bool isEidolon = card is IEidolonCard;
        bool hasEvoke = isEidolon && Owner.HasPower<EvokePower>();
        bool hadEidolonResonance =
            isEidolon &&
            Owner.HasPower<EidolonResonancePower>();

        /*
         * Eidolon cards take priority over normal Attack cards.
         *
         * If Evoke is active, the Eidolon does not generate Summon.
         * Otherwise, the Eidolon generates Summon normally, including
         * while Eidolon Resonance is active.
         */
        if (isEidolon)
        {
            if (!hasEvoke)
            {
                amount = CalculateEidolonCardGain(card);

                amount = ModifyEidolonCardSummonGain(
                    card,
                    amount);
            }

            rydia?.PlayAnimation(
                Owner.Creature,
                "idle");
        }
        else if (card.Type == CardType.Attack)
        {
            amount = CalculateAttackGain(card);

            amount = ModifyAttackSummonGain(
                card,
                amount);
        }

        if (card is IMagicCard)
        {
            rydia?.PlayAnimation(
                Owner.Creature,
                "idle");
        }

        amount = ModifyTotalCardSummonGain(
            card,
            amount);

        if (amount > 0)
        {
            GainSummon(amount);

            await SummonManager.CheckSummonReady(
                Owner.Creature,
                choiceContext,
                Owner.Creature,
                null);
        }

        /*
         * Evoke always has priority over Eidolon Resonance.
         *
         * If both are active, consume Evoke and leave
         * Eidolon Resonance untouched.
         */
        if (isEidolon && hasEvoke)
        {
            await PowerCmd.Remove<EvokePower>(
                Owner.Creature);

            return;
        }

        /*
         * If Evoke wasn't responsible for this Eidolon,
         * consume the existing Eidolon Resonance normally.
         *
         * Petrifying Breath remains the exception.
         */
        if (isEidolon &&
            hadEidolonResonance &&
            card is not PetrifyingBreath)
        {
            await PowerCmd.Remove<EidolonResonancePower>(
                Owner.Creature);
            SummonManager.SetSummon(Owner, 0);
            rydia?.PlayAnimation(
                Owner.Creature,
                "idle");
        }
    }

    protected virtual int CalculateTurnStartGain()
    {
        return SummonPerTurn;
    }

    protected virtual int CalculateAttackGain(
        CardModel card)
    {
        return SummonPerAttack;
    }

    protected virtual int CalculateEidolonCardGain(
        CardModel card)
    {
        return SummonPerEidolonCard;
    }

    // =====================================================================
    // OVERRIDE POINTS
    // =====================================================================

    protected virtual int ModifyTurnStartSummonGain(
        int amount)
    {
        return amount;
    }

    protected virtual int ModifyAttackSummonGain(
        CardModel card,
        int amount)
    {
        return amount;
    }

    protected virtual int ModifyEidolonCardSummonGain(
        CardModel card,
        int amount)
    {
        return amount;
    }

    protected virtual int ModifyTotalCardSummonGain(
        CardModel card,
        int amount)
    {
        return amount;
    }

    /// <summary>
    /// Called whenever this relic's stored Summon changes.
    /// Derived summon relics can override this for UI or other effects.
    /// </summary>
    protected virtual void OnSummonChanged(
        int previousAmount,
        int newAmount)
    {
    }

    // =====================================================================
    // MANAGER COMMUNICATION
    // =====================================================================

    internal void NotifySummonChanged(
        int previousAmount,
        int newAmount)
    {
        OnSummonChanged(
            previousAmount,
            newAmount);
    }

    // =====================================================================
    // HELPERS
    // =====================================================================

    private void GainSummon(
        int requestedAmount)
    {
        requestedAmount = Math.Max(
            0,
            requestedAmount);

        if (requestedAmount <= 0)
            return;

        SummonManager.GainSummon(
            Owner,
            requestedAmount);
    }
}