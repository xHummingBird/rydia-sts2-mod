using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Rydia.RydiaCode.Powers;
using Rydia.RydiaCode.Relics;

namespace Rydia.RydiaCode.Mechanics;

public static class SummonManager
{
    public const int DefaultMaxSummon = 100;

    // =====================================================================
    // READ
    // =====================================================================

    public static int GetSummon(
        Player? player)
    {
        SummonRelicBase? relic =
            GetSummonRelic(player);

        return relic?.StoredSummon ?? 0;
    }

    public static int GetMaxSummon(
        Player? player)
    {
        SummonRelicBase? relic =
            GetSummonRelic(player);

        return relic?.MaxSummon
               ?? DefaultMaxSummon;
    }

    public static bool IsFull(
        Player? player)
    {
        SummonRelicBase? relic =
            GetSummonRelic(player);

        if (relic == null)
            return false;

        return relic.StoredSummon >=
               relic.MaxSummon;
    }

    public static bool HasSummon(
        Player? player,
        int amount)
    {
        if (amount <= 0)
            return true;

        SummonRelicBase? relic =
            GetSummonRelic(player);

        if (relic == null)
            return false;

        return relic.StoredSummon >= amount;
    }

    // =====================================================================
    // SET
    // =====================================================================

    /// <summary>
    /// Sets Summon and returns the actual signed change.
    ///
    /// Examples:
    /// 40 to 60 returns 20.
    /// 60 to 40 returns -20.
    /// </summary>
    public static int SetSummon(
        Player? player,
        int amount)
    {
        SummonRelicBase? relic =
            GetSummonRelic(player);

        if (relic == null)
            return 0;

        int previousAmount =
            relic.StoredSummon;

        int newAmount =
            Math.Clamp(
                amount,
                0,
                relic.MaxSummon);

        if (previousAmount == newAmount)
            return 0;

        relic.StoredSummon =
            newAmount;

        relic.NotifySummonChanged(
            previousAmount,
            newAmount);

        return newAmount - previousAmount;
    }

    // =====================================================================
    // GAIN
    // =====================================================================

    /// <summary>
    /// Adds Summon and returns the amount actually gained.
    ///
    /// Adding 10 at 95/100 returns 5.
    /// </summary>
    public static int GainSummon(
        Player? player,
        int amount)
    {
        if (amount <= 0)
            return 0;

        SummonRelicBase? relic =
            GetSummonRelic(player);

        if (relic == null)
            return 0;

        int previousAmount =
            relic.StoredSummon;

        SetSummon(
            player,
            previousAmount + amount);

        return relic.StoredSummon -
               previousAmount;
    }

    // =====================================================================
    // SPEND
    // =====================================================================

    /// <summary>
    /// Attempts to spend the complete amount.
    /// Nothing is spent if there is not enough Summon.
    /// </summary>
    public static bool TrySpendSummon(
        Player? player,
        int amount)
    {
        if (amount <= 0)
            return true;

        SummonRelicBase? relic =
            GetSummonRelic(player);

        if (relic == null)
            return false;

        if (relic.StoredSummon < amount)
            return false;

        SetSummon(
            player,
            relic.StoredSummon - amount);

        return true;
    }

    /// <summary>
    /// Spends up to the requested amount and returns the
    /// amount actually spent.
    /// </summary>
    public static int SpendSummon(
        Player? player,
        int amount)
    {
        if (amount <= 0)
            return 0;

        SummonRelicBase? relic =
            GetSummonRelic(player);

        if (relic == null)
            return 0;

        int amountSpent =
            Math.Min(
                relic.StoredSummon,
                amount);

        if (amountSpent <= 0)
            return 0;

        SetSummon(
            player,
            relic.StoredSummon - amountSpent);

        return amountSpent;
    }

    // =====================================================================
    // READY CHECK
    // =====================================================================

    public static async Task CheckSummonReady(
        Creature owner,
        PlayerChoiceContext? choiceContext,
        Creature? source,
        CardModel? card)
    {
        if (owner == null)
            return;

        int summon =
            GetSummon(owner.Player);

        int maxSummon =
            GetMaxSummon(owner.Player);

        bool isReady =
            summon >= maxSummon;

        bool alreadyReady =
            owner.HasPower<EidolonResonancePower>();

        if (isReady && !alreadyReady)
        {
            await PowerCmd.Apply<EidolonResonancePower>(
                choiceContext,
                owner,
                1m,
                null,
                null);
        }
        else if (!isReady && alreadyReady)
        {
            await PowerCmd.Remove<EidolonResonancePower>(
                owner);
        }
    }

    // =====================================================================
    // RESET
    // =====================================================================

    public static void ResetSummon(
        Player? player)
    {
        SetSummon(
            player,
            0);
    }

    // =====================================================================
    // RELIC LOOKUP
    // =====================================================================

    public static SummonRelicBase?
        GetSummonRelic(
            Player? player)
    {
        if (player == null)
            return null;

        return player.Relics
            .OfType<SummonRelicBase>()
            .FirstOrDefault();
    }
}