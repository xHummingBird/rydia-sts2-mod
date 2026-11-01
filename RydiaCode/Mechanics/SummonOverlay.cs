using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Rydia.RydiaCode.Extensions;

namespace Rydia.RydiaCode.Mechanics;

public partial class RydiaSummonDisplay : Control
{
    public static RydiaSummonDisplay? Instance
    {
        get;
        private set;
    }

    private const string ScenePath =
        "res://Rydia/scenes/summon_display.tscn";

    /*
     * Position of the entire Summon display
     * relative to the energy counter.
     */
    private static readonly Vector2 DisplayPosition =
        new(65f, -25f);

    /*
     * Additional position offset for only the number.
     *
     * Negative X = left
     * Positive X = right
     * Negative Y = up
     * Positive Y = down
     */
    private static readonly Vector2 LabelPositionOffset =
        new(0f, 13f);

    private static readonly Texture2D SummonTexture =
        GD.Load<Texture2D>(
            "res://Rydia/images/charui/summon_icon.png");

    /*
     * Hover-tip position relative to the display.
     */
    private static readonly Vector2 HoverTipOffset =
        new(-120f, -550f);

    private Control? _display;
    private TextureRect? _overlay;
    private RichTextLabel? _summonLabel;

    private Player? _player;

    private IHoverTip? _summonHoverTip;

    private int _lastSummon =
        int.MinValue;

    public override void _Ready()
    {
        Instance = this;

        Name =
            "RydiaSummonDisplay";

        MouseFilter =
            MouseFilterEnum.Pass;

        CallDeferred(
            nameof(Setup));
    }

    private async void Setup()
    {
        if (!IsInsideTree())
            return;

        /*
         * Wait for the local combat player.
         */
        for (int i = 0; i < 60; i++)
        {
            var state =
                CombatManager.Instance?
                    .DebugOnlyGetState();

            Player? player =
                state?.Players.FirstOrDefault(
                    currentPlayer =>
                        LocalContext.IsMe(currentPlayer));

            if (player != null)
            {
                if (player.Character is not Character.Rydia)
                {
                    QueueFree();
                    return;
                }

                _player =
                    player;

                break;
            }

            SceneTree? tree =
                GetTree();

            if (tree == null)
                return;

            await ToSignal(
                tree,
                SceneTree.SignalName.ProcessFrame);
        }

        if (_player == null)
        {
            QueueFree();
            return;
        }

        PackedScene? scene =
            GD.Load<PackedScene>(
                ScenePath);

        if (scene == null)
        {
            GD.PushError(
                $"[Rydia Summon] Failed to load {ScenePath}");

            QueueFree();
            return;
        }

        _display =
            scene.Instantiate<Control>();

        AddChild(
            _display);

        _display.SetAnchorsPreset(
            LayoutPreset.BottomLeft);

        _display.Position =
            DisplayPosition;

        GetSceneNodes();

        if (!ValidateRequiredNodes())
        {
            QueueFree();
            return;
        }

        ConfigureDisplay();
        ConfigureLabel();
        ConfigureHoverTip();

        ResetDisplayState();
        UpdateDisplay();
    }

    private void GetSceneNodes()
    {
        if (_display == null)
            return;

        _overlay =
            _display.GetNodeOrNull<TextureRect>(
                "Overlay");

        _summonLabel =
            _display.GetNodeOrNull<RichTextLabel>(
                "RichTextLabel");
    }

    private bool ValidateRequiredNodes()
    {
        bool valid =
            _display != null &&
            _overlay != null &&
            _summonLabel != null;

        if (!valid)
        {
            GD.PushError(
                "[Rydia Summon] summon_display.tscn must contain " +
                "Overlay and RichTextLabel.");
        }

        return valid;
    }

    private void ConfigureDisplay()
    {
        if (_display == null ||
            _overlay == null)
        {
            return;
        }

        _display.MouseFilter =
            MouseFilterEnum.Stop;

        _overlay.Visible =
            true;

        _overlay.Texture =
            SummonTexture;

        /*
         * Prevent the image from blocking mouse input
         * to the root Control.
         */
        _overlay.MouseFilter =
            MouseFilterEnum.Ignore;

        _display.MouseEntered +=
            OnSummonHovered;

        _display.MouseExited +=
            OnSummonUnhovered;
    }

    private void PlayGainAnimation()
    {
        if (_summonLabel == null)
            return;

        _summonLabel.Scale =
            Vector2.One;

        Tween tween =
            CreateTween();

        tween.SetParallel();

        tween.TweenProperty(
            _summonLabel,
            "scale",
            new Vector2(1.35f, 1.35f),
            0.08f);

        tween.TweenProperty(
                _summonLabel,
                "scale",
                Vector2.One,
                0.12f)
            .SetDelay(0.08f);
    }

    private void ConfigureLabel()
    {
        if (_summonLabel == null)
            return;

        _summonLabel.Visible =
            true;

        _summonLabel.MouseFilter =
            MouseFilterEnum.Ignore;

        Font? font =
            GD.Load<Font>(
                "res://themes/kreon_bold_shared.tres");

        if (font != null)
        {
            _summonLabel.AddThemeFontOverride(
                "font",
                font);

            _summonLabel.AddThemeFontOverride(
                "normal_font",
                font);
        }

        _summonLabel.AddThemeFontSizeOverride(
            "normal_font_size",
            24);

        _summonLabel.AddThemeColorOverride(
            "default_color",
            Colors.White);

        _summonLabel.AddThemeColorOverride(
            "font_outline_color",
            new Color(
                0.15f,
                0.08f,
                0.02f));

        _summonLabel.AddThemeConstantOverride(
            "outline_size",
            7);

        _summonLabel.BbcodeEnabled =
            true;

        _summonLabel.FitContent =
            true;

        _summonLabel.Position +=
            LabelPositionOffset;
    }

    private void ConfigureHoverTip()
    {
        _summonHoverTip =
            RydiaStaticHoverTips.Summon;
    }

    public override void _Process(
        double delta)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_player == null ||
            _summonLabel == null)
        {
            return;
        }

        int summon =
            SummonManager.GetSummon(
                _player);

        if (summon == _lastSummon)
            return;

        SetSummonDisplay(
            summon);
    }

    private void SetSummonDisplay(
        int summon)
    {
        int previous =
            _lastSummon;

        _lastSummon =
            Mathf.Max(
                summon,
                0);

        if (_lastSummon > previous)
        {
            PlayGainAnimation();
        }

        if (_overlay == null ||
            _summonLabel == null)
        {
            return;
        }

        /*
         * Rydia does not have separate empty/full textures.
         * The same Summon texture is always shown.
         */
        _overlay.Texture =
            SummonTexture;

        /*
         * Unlike Tidus's Overdrive display, the number
         * remains visible when Summon is full.
         */
        _summonLabel.Visible =
            true;

        _summonLabel.Text =
            $"[center]{_lastSummon}[/center]";
    }

    private void OnSummonHovered()
    {
        if (_display == null ||
            _summonHoverTip == null)
        {
            return;
        }

        NHoverTipSet.Clear();

        Control? tip =
            NHoverTipSet.CreateAndShow(
                _display,
                _summonHoverTip);

        if (tip == null)
            return;

        tip.GlobalPosition =
            _display.GlobalPosition +
            HoverTipOffset;

        tip.MouseFilter =
            MouseFilterEnum.Ignore;
    }

    private void OnSummonUnhovered()
    {
        if (_display == null)
            return;

        NHoverTipSet.Remove(
            _display);
    }

    private void ResetDisplayState()
    {
        _lastSummon =
            int.MinValue;

        if (_overlay != null)
        {
            _overlay.Texture =
                SummonTexture;
        }

        if (_summonLabel != null)
        {
            _summonLabel.Visible =
                true;

            _summonLabel.Text =
                "[center]0[/center]";
        }
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance =
                null;
        }

        if (_display != null)
        {
            _display.MouseEntered -=
                OnSummonHovered;

            _display.MouseExited -=
                OnSummonUnhovered;

            NHoverTipSet.Remove(
                _display);
        }

        _summonHoverTip =
            null;

        _player =
            null;

        _overlay =
            null;

        _summonLabel =
            null;

        _display =
            null;
    }
}

[HarmonyPatch(
    typeof(NEnergyCounter),
    nameof(NEnergyCounter._Ready))]
public static class RydiaSummonDisplayOverlayPatch
{
    public static void Postfix(
        NEnergyCounter __instance)
    {
        if (__instance == null)
            return;

        if (!GodotObject.IsInstanceValid(
                __instance))
        {
            return;
        }

        var state =
            CombatManager.Instance?
                .DebugOnlyGetState();

        Player? player =
            state?.Players.FirstOrDefault(
                currentPlayer =>
                    LocalContext.IsMe(currentPlayer));

        if (player?.Character is not Character.Rydia)
            return;

        if (__instance.GetNodeOrNull<RydiaSummonDisplay>(
                "RydiaSummonDisplay") != null)
        {
            return;
        }

        __instance.AddChild(
            new RydiaSummonDisplay
            {
                Name =
                    "RydiaSummonDisplay"
            });
    }
}