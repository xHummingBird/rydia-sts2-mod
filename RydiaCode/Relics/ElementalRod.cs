using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using Rydia.RydiaCode.Powers;

namespace Rydia.RydiaCode.Relics;

public class ElementalRod : RydiaRelic
{
    private int _burnApplied;

    private int _freezeApplied;

    private int _shockApplied;

    public override RelicRarity Rarity =>
        RelicRarity.Shop;

    public override bool ShowCounter =>
        true;

    public override int DisplayAmount =>
        BurnApplied +
        FreezeApplied +
        ShockApplied;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(1m),
        new PowerVar<DexterityPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BurnPower>(),
        HoverTipFactory.FromPower<FreezePower>(),
        HoverTipFactory.FromPower<ShockPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    private int BurnApplied
    {
        get => _burnApplied;
        set
        {
            AssertMutable();
            _burnApplied = value;
            UpdateStatus();
        }
    }

    private int FreezeApplied
    {
        get => _freezeApplied;
        set
        {
            AssertMutable();
            _freezeApplied = value;
            UpdateStatus();
        }
    }

    private int ShockApplied
    {
        get => _shockApplied;
        set
        {
            AssertMutable();
            _shockApplied = value;
            UpdateStatus();
        }
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (amount <= 0m)
            return;

        if (applier != Owner.Creature)
            return;

        if (power is BurnPower)
            BurnApplied = 1;
        else if (power is FreezePower)
            FreezeApplied = 1;
        else if (power is ShockPower)
            ShockApplied = 1;
        else
            return;

        if (BurnApplied <= 0 ||
            FreezeApplied <= 0 ||
            ShockApplied <= 0)
            return;

        BurnApplied = 0;
        FreezeApplied = 0;
        ShockApplied = 0;

        Flash();

        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Strength.BaseValue,
            Owner.Creature,
            null);

        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars.Dexterity.BaseValue,
            Owner.Creature,
            null);
    }

    public override Task AfterCombatEnd(
        CombatRoom room)
    {
        BurnApplied = 0;
        FreezeApplied = 0;
        ShockApplied = 0;

        return Task.CompletedTask;
    }

    private void UpdateStatus()
    {
        Status =
            DisplayAmount > 0
                ? RelicStatus.Active
                : RelicStatus.Normal;
    }
}