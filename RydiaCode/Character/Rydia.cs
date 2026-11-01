using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Rydia.RydiaCode.Extensions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Rydia.RydiaCode.Cards.Basic;
using Rydia.RydiaCode.Powers;
using Rydia.RydiaCode.Relics;

namespace Rydia.RydiaCode.Character;

public class Rydia : PlaceholderCharacterModel
{
    public const string CharacterId = "Rydia";

    public static readonly Color Color = new("ffffff");
    
    private Vector2? _originalPosition;

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeRydia>(),
        ModelDb.Card<StrikeRydia>(),
        ModelDb.Card<StrikeRydia>(),
        ModelDb.Card<StrikeRydia>(),
        ModelDb.Card<PetrifyingBreath>(),
        ModelDb.Card<MistBreath>(),
        ModelDb.Card<DefendRydia>(),
        ModelDb.Card<DefendRydia>(),
        ModelDb.Card<DefendRydia>(),
        ModelDb.Card<DefendRydia>(),
        ModelDb.Card<DefendRydia>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<Whip>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<RydiaCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<RydiaRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<RydiaPotionPool>();

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets.
        These are just some of the simplest assets, given some placeholders to differentiate your character with.
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }
    
    public override CustomEnergyCounter? CustomEnergyCounter =>
        new(
            EnergyCounterPaths,
            new Color(0.2f, 0.2f, 0.2f),
            new Color(1f, 1f, 1f));
    
    private string EnergyCounterPaths(int i)
    {
        return i switch
        {
            1 => "charui/big_energy.png".ImagePath(),
            _ => "charui/blank.png".ImagePath()
        };
    }

    public override string CustomIconTexturePath =>
        "character_icon_rydia.png".CharacterUiPath();

    public override string CustomCharacterSelectIconPath =>
        "char_select_rydia.png".CharacterUiPath();

    public override string CustomCharacterSelectLockedIconPath =>
        "char_select_char_name_locked.png".CharacterUiPath();

    public override string CustomMapMarkerPath =>
        "map_marker_rydia.png".CharacterUiPath();

    private const string CustomVisualScenePath =
        "res://Rydia/scenes/rydia.tscn";

    public override string CustomRestSiteAnimPath =>
        "res://Rydia/scenes/rydia_rest_site.tscn";

    public override string CustomCharacterSelectBg =>
        "res://Rydia/images/charui/char_selection_bg_rydia.tscn";

    public override string CustomMerchantAnimPath =>
        "res://Rydia/scenes/rydia_merchant.tscn";

    public override string CharacterSelectSfx =>
        "res://Rydia/sounds/victory (1).wav";

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        RydiaAssets.EnsurePreloaded();

        return NodeFactory<NCreatureVisuals>.CreateFromScene(
            CustomVisualScenePath);
    }
    
    public (
        float total,
        float[] impacts
    ) PlayAnimation(
        Creature creature,
        string trigger,
        bool normalSpeed = false)
    {
        if (creature == null ||
            string.IsNullOrEmpty(trigger))
        {
            return (
                0f,
                Array.Empty<float>());
        }

        var node =
            NCombatRoom.Instance?
                .GetCreatureNode(creature);

        if (node?.Visuals == null)
        {
            return (
                0f,
                Array.Empty<float>());
        }

        var animPlayer =
            node.Visuals.GetNodeOrNull<AnimationPlayer>(
                "AnimationPlayer");

        if (animPlayer == null)
        {
            return (
                0f,
                Array.Empty<float>());
        }
        
        var mode = creature.HasPower<EidolonResonancePower>() ? "resonance" : "rydia";
        string godotTrigger =
            trigger.ToLowerInvariant() switch
            {
                "hit" => "hurt",
                "idle" or "idle_loop" => $"idle_{mode}",
                "attack" => "attack_rydia",
                "dead" => "die",
                "die" => "die",
                "cast" => "cast_rydia",
                _ => trigger
            };

        if (!animPlayer.HasAnimation(godotTrigger))
        {
            return (
                0f,
                Array.Empty<float>());
        }

        var animation =
            animPlayer.GetAnimation(godotTrigger);

        float speed = 1.0f;

        animPlayer.SpeedScale = speed;
        animPlayer.Play(godotTrigger);

        if (godotTrigger is not ("die" or "dead"))
        {
            if (creature.HasPower<EidolonResonancePower>() || creature.HasPower<EvokePower>())
                animPlayer.Queue("idle_resonance");
            animPlayer.Queue("idle_rydia");
        }

        float effectiveLength =
            (float)animation.Length / speed;

        return (
            effectiveLength,
            Array.Empty<float>());
    }

    // =========================================================================
    // DASH TO
    // =========================================================================

    public async Task DashTo(
        Creature player,
        Creature target,
        float durationSeconds = 0.3f,
        float distance = 200f,
        bool dashBehind = false,
        string? overrideAnim = null)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player);
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node == null || targetNode == null) return;

        if (!_originalPosition.HasValue)
            _originalPosition = node.GlobalPosition;
        
        PlayAnimation(player, overrideAnim ??"dash");
		
        bool playerIsLeftOfTarget = node.GlobalPosition.X < targetNode.GlobalPosition.X;
		
        Vector2 offsetDir = playerIsLeftOfTarget ? Vector2.Left : Vector2.Right;
		
        if (dashBehind)
            offsetDir = -offsetDir;

        Vector2 targetPos = targetNode.GlobalPosition + offsetDir * distance;

        var tween = node.CreateTween();
        tween.TweenProperty(node, "global_position", targetPos, durationSeconds)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        await node.ToSignal(tween, Tween.SignalName.Finished);
    }

    public async Task Retreat(
        Creature player,
        string? animation = "retreat",
        bool goIdle = true,
        float duration = 0.3f)
    {
        var node = NCombatRoom.Instance?.GetCreatureNode(player);
        if (node == null || !_originalPosition.HasValue) return;

        if (!string.IsNullOrEmpty(animation))
            PlayAnimation(player, animation);

        var tween = node.CreateTween();
        tween.TweenProperty(node, "global_position", _originalPosition.Value, duration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.InOut);

        await node.ToSignal(tween, Tween.SignalName.Finished);

        _originalPosition = null;

        var visuals = node.Visuals.GetNodeOrNull<Node2D>("Visuals");
        if (visuals != null)
            visuals.Position = Vector2.Zero;

        if (goIdle)
            PlayAnimation(player, "idle");
    }

    // =========================================================================
    // SCREEN SHAKE
    // =========================================================================

    public void DoScreenShake(
        ShakeStrength strength =
            ShakeStrength.Medium,
        ShakeDuration duration =
            ShakeDuration.Short)
    {
        NGame.Instance?.ScreenShake(
            strength,
            duration);
    }

    // =========================================================================
    // VFX
    // =========================================================================

    public Node2D PlayVfxOnTarget(
        Creature target,
        string path,
        string animName)
    {
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);

        if (targetNode?.Visuals == null)
            return null;

        var scene = GD.Load<PackedScene>(path);
        var vfx = scene.Instantiate<Node2D>();

        targetNode.Visuals.AddChild(vfx);
        vfx.Position = Vector2.Zero;

        var animPlayer =
            vfx.GetNode<AnimationPlayer>("AnimationPlayer");

        if (animPlayer.HasAnimation(animName))
            animPlayer.Play(animName);

        return vfx;
    }
    
    public Node2D PlayVfxBehindTarget(
        Creature target,
        string path,
        string animName)
    {
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);

        if (targetNode?.Visuals == null)
            return null;

        var scene = GD.Load<PackedScene>(path);
        var vfx = scene.Instantiate<Node2D>();

        targetNode.Visuals.AddChild(vfx);
        vfx.Position = Vector2.Zero;

        // Render behind the target's other visuals
        targetNode.Visuals.MoveChild(vfx, 0);

        var animPlayer =
            vfx.GetNode<AnimationPlayer>("AnimationPlayer");

        if (animPlayer.HasAnimation(animName))
            animPlayer.Play(animName);

        return vfx;
    }

    /// <summary>
    /// Use this overload when the owner creature is already available.
    /// This avoids searching the combat creature list.
    /// </summary>

    // =========================================================================
    // AUTOMATIC ANIMATION TRIGGERS
    // =========================================================================

    [HarmonyPatch(
        typeof(NCreature),
        nameof(NCreature.SetAnimationTrigger))]
    public static class NCreatureSetTriggerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            NCreature __instance,
            string trigger)
        {
            if (__instance.Entity?
                    .Player?
                    .Character
                is Rydia character)
            {
                character.PlayAnimation(
                    __instance.Entity,
                    trigger);

                return false;
            }

            return true;
        }
    }

    // =========================================================================
    // DEATH ANIMATION
    // =========================================================================

    [HarmonyPatch(
        typeof(NCreature),
        nameof(NCreature.StartDeathAnim))]
    public static class RydiaStartDeathAnimPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            NCreature __instance,
            ref float __result)
        {
            if (__instance.Entity?
                    .Player?
                    .Character
                is not Rydia character)
            {
                return;
            }

            AudioHelper.PlayRandomGameover();
            BattleTheme.Stop();
            
            var animation =
                character.PlayAnimation(
                    __instance.Entity,
                    "die");

            __result =
                animation.total > 0f
                    ? animation.total
                    : 1.5f;
        }
    }
    
    [HarmonyPatch(
        typeof(NAudioManager),
        nameof(NAudioManager.PlayMusic))]
    public static class RydiaGameOverMusicPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(string music)
        {
            // Only intercept the vanilla game over music.
            if (music != "event:/temp/sfx/game_over")
                return true;

            if (RunManager.Instance == null)
                return true;

            var runState = RunManager.Instance.DebugOnlyGetState();

            if (runState == null)
                return true;

            var localPlayer = LocalContext.GetMe(runState);

            if (localPlayer?.Character is not Rydia)
                return true;

            BattleTheme.PlayGameOverTheme();

            // Skip vanilla game over music.
            return false;
        }
    }
    
    [HarmonyPatch(
        typeof(Hook),
        nameof(Hook.AfterCombatVictory))]
    public static class RydiaVictoryAnimationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            IRunState runState,
            CombatState? combatState)
        {
            var creatures =
                combatState?
                    .Creatures?
                    .Where(creature =>
                        creature.IsPlayer);
    
            if (creatures == null)
                return;
    
            foreach (var creature in creatures)
            {
                if (creature.Player?.Character
                    is not Rydia character)
                {
                    continue;
                }
    
                var node =
                    NCombatRoom.Instance?
                        .GetCreatureNode(creature);
    
                var animPlayer =
                    node?
                        .Visuals?
                        .GetNodeOrNull<AnimationPlayer>(
                            "AnimationPlayer");
    
                if (animPlayer == null)
                    continue;
    
                AudioHelper.PlayRandomVictory();

                float speed = 1.0f;
    
                animPlayer.SpeedScale = speed;
    
                if (animPlayer.HasAnimation(
                    "victory_before"))
                {
                    animPlayer.Play(
                        "victory_before");
    
                    if (animPlayer.HasAnimation(
                        "victory"))
                    {
                        animPlayer.Queue(
                            "victory");
                    }
                }
                else if (animPlayer.HasAnimation(
                    "victory"))
                {
                    animPlayer.Play(
                        "victory");
                }
            }
        }
    }

    // =========================================================================
    // DAMAGE ANIMATION
    // =========================================================================

    [HarmonyPatch(
        typeof(Hook),
        nameof(Hook.AfterDamageReceived))]
    public static class RydiaDamageAnimationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            Creature target,
            DamageResult result,
            ValueProp props,
            Creature? dealer)
        {
            if (target.Player?.Character
                is not Rydia character)
            {
                return;
            }

            if (dealer == null ||
                dealer.Side != CombatSide.Enemy)
            {
                return;
            }

            if (props.HasFlag(
                    ValueProp.SkipHurtAnim) ||
                props.HasFlag(
                    ValueProp.Unpowered))
            {
                return;
            }

            if (result.WasFullyBlocked &&
                result.BlockedDamage > 0)
            {
                character.PlayAnimation(
                    target,
                    "block", true);

                return;
            }

            if (result.UnblockedDamage <= 0 ||
                target.IsDead)
            {
                return;
            }

            character.PlayAnimation(
                target,
                "hit", true);

            if (target.CurrentHp < 20)
            {
                AudioHelper
                    .PlayRandomDamagedCritical();
            }
            else if (result.UnblockedDamage < 10)
            {
                AudioHelper
                    .PlayRandomDamaged();
            }
            else
            {
                AudioHelper
                    .PlayRandomDamagedHigh();
            }
        }
    }

    // =========================================================================
    // CHOOSE-A-CARD PATCH
    // =========================================================================
    
}
