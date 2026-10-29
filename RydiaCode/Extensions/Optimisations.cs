using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Rydia.RydiaCode.Extensions;

public static class RydiaAssets
{
    private static PackedScene? _rydiaScene;
    private static PackedScene? _vfxScene;

    private const string RydiaScenePath = "res://Rydia/scenes/rydia.tscn";
    private const string VfxPath = "res://Rydia/scenes/vfx.tscn";
    
    
    public static bool IsRydiaInRun(IRunState runState)
    {
        return runState?.Players?.Any(
            p => p?.Character is Rydia.RydiaCode.Character.Rydia
        ) ?? false;
    }

    public static PackedScene? RydiaScene
    {
        get
        {
            _rydiaScene = LoadOrReload(_rydiaScene, RydiaScenePath, "Rydia scene");
            return _rydiaScene;
        }
    }

    public static PackedScene? VfxScene
    {
        get
        {
            _vfxScene = LoadOrReload(_vfxScene, VfxPath, "VFX");
            return _vfxScene;
        }
    }
    
    private static PackedScene? LoadOrReload(PackedScene? cachedScene, string path, string label)
    {
        if (cachedScene != null && GodotObject.IsInstanceValid(cachedScene))
            return cachedScene;

        GD.Print($"RydiaAssets: Loading {label} from {path}");

        var scene = GD.Load<PackedScene>(path);

        if (scene == null)
        {
            GD.PrintErr($"RydiaAssets: FAILED to load {label}: {path}");
            return null;
        }

        GD.Print($"RydiaAssets: Loaded {label}");
        return scene;
    }

    public static void EnsurePreloaded()
    {
        _ = RydiaScene;
        _ = VfxScene;

        GD.Print("RydiaAssets: EnsurePreloaded finished");
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterActEntered))]
public static class RydiaAfterActEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState)
    {
        // var player = runState?.Players?.FirstOrDefault();
        //
        // if (player?.Character is not Character.Rydia)
        //     return;
        if (!RydiaAssets.IsRydiaInRun(runState))
            return;
        
        GD.Print("AfterActEntered: Rydia detected → preloading");

        RydiaAssets.EnsurePreloaded();
    }
}


[HarmonyPatch(typeof(Hook), nameof(Hook.AfterRoomEntered))]
public static class RydiaAfterRoomEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState, AbstractRoom room)
    {
        // var player = runState?.Players?.FirstOrDefault();
        //
        // if (player?.Character is not Character.Rydia)
        //     return;
        if (!RydiaAssets.IsRydiaInRun(runState))
            return;
        
        GD.Print($"AfterRoomEntered: Rydia detected → preloading. Room = {room.GetType().Name}");

        RydiaAssets.EnsurePreloaded();
    }
}
