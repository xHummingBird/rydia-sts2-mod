using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace Rydia.RydiaCode.Extensions;

public static class BattleTheme
{
    private readonly record struct MusicTrack(
        string Path,
        float VolumeDb);

    private static AudioStreamPlayer? _battlePlayer;
    private static AudioStreamPlayer? _fanfarePlayer;
    private static AudioStreamPlayer? _gameOverPlayer;

    private static float _battleVolumeDb;
    private static float _fanfareVolumeDb;
    private static float _gameOverVolumeDb;

    // ============================================================
    // VICTORY / GAME OVER
    // ============================================================

    private static readonly MusicTrack VictoryFanfare =
        new(
            "res://Rydia/music/victory.mp3",
            -8.5f);

    private static readonly MusicTrack VictoryFanfareShort =
        new(
            "res://Rydia/music/victory.mp3",
            -8.5f);

    private static readonly MusicTrack GameOverTheme =
        new(
            "res://Rydia/music/08 Defeat.mp3",
            -8.5f);

    // ============================================================
    // NORMAL
    // Define each track as:
    // new("res://Rydia/music/File.mp3", volumeDb)
    // ============================================================

    private static readonly MusicTrack[] Act1Normal =
    [
        new(
            "res://Rydia/music/08. Battle 1.mp3",
            -8.5f),
    ];

    private static readonly MusicTrack[] Act2Normal =
    [
        new(
            "res://Rydia/music/08. Battle 1.mp3",
            -8.5f),

        new(
            "res://Rydia/music/BATTLE1-dissidia.mp3",
            -8.5f),
    ];

    private static readonly MusicTrack[] Act3Normal =
    [
        new(
            "res://Rydia/music/08. Battle 1.mp3",
            -8.5f),

        new(
            "res://Rydia/music/BATTLE1-dissidia.mp3",
            -8.5f),

        new(
            "res://Rydia/music/13. Battle 2.mp3",
            -8.5f)
    ];

    // ============================================================
    // ELITE
    // ============================================================

    private static readonly MusicTrack[] Act1Elite =
    [
        new(
            "res://Rydia/music/BATTLE1-dissidia.mp3",
            -8.5f),
    ];

    private static readonly MusicTrack[] Act2Elite =
    [
        new(
            "res://Rydia/music/14. Bomb Ring.mp3",
            -8.5f),
        
        new(
            "res://Rydia/music/13. Battle 2.mp3",
            -8.5f),
    ];

    private static readonly MusicTrack[] Act3Elite =
    [
        new(
            "res://Rydia/music/Battle With the Four Fiends - Dissidia.mp3",
            -8.5f),

        new(
            "res://Rydia/music/battle_2_dissidia.mp3",
            -8.5f),
    ];

    // ============================================================
    // BOSS
    // ============================================================

    private static readonly MusicTrack[] Act1Boss =
    [
        new(
            "res://Rydia/music/13. Battle 2.mp3",
            -8.5f)
    ];

    private static readonly MusicTrack[] Act2Boss =
    [
        new(
            "res://Rydia/music/28. Battle With the Four Fiends.mp3",
            -8.5f),
        
        new(
            "res://Rydia/music/2-07. Battling Culex 3D.mp3",
            -8.5f),
    ];

    private static readonly MusicTrack[] Act3Boss =
    [
        new(
            "res://Rydia/music/23. Golbez, Clad In Darkness.mp3",
            -8.5f),

        new(
            "res://Rydia/music/43. The Final Battle.mp3",
            -8.5f),
    ];

    // ============================================================
    // VANILLA MUSIC CONTROL
    // ============================================================

    public static void StopVanillaMusicOnly()
    {
        var controller =
            NRunMusicController.Instance;

        if (controller == null)
            return;

        var proxyField =
            AccessTools.Field(
                typeof(NRunMusicController),
                "_proxy");

        if (proxyField == null)
            return;

        var proxy =
            proxyField.GetValue(controller) as Node;

        proxy?.Call("stop_music");
    }

    public static void ResumeVanillaMusic()
    {
        var controller =
            NRunMusicController.Instance;

        if (controller == null)
            return;

        var proxyField =
            AccessTools.Field(
                typeof(NRunMusicController),
                "_proxy");

        var currentTrackField =
            AccessTools.Field(
                typeof(NRunMusicController),
                "_currentTrack");

        if (proxyField == null ||
            currentTrackField == null)
        {
            return;
        }

        var proxy =
            proxyField.GetValue(controller) as Node;

        string? currentTrack =
            currentTrackField.GetValue(controller)
                as string;

        proxy?.Call("stop_music");

        if (proxy == null ||
            string.IsNullOrEmpty(currentTrack))
        {
            return;
        }

        proxy.Call(
            "update_music",
            currentTrack);

        proxy.Call(
            "update_global_parameter",
            "Progress",
            7);
    }

    // ============================================================
    // VOLUME
    // ============================================================

    private static float ScaleDbByBgmSetting(
        float baseVolumeDb)
    {
        float bgmSetting =
            SaveManager.Instance?
                .SettingsSave?
                .VolumeBgm
            ?? 1f;

        if (bgmSetting <= 0f)
            return -80f;

        float linearVolume =
            Mathf.DbToLinear(baseVolumeDb);

        linearVolume *= bgmSetting;

        return Mathf.LinearToDb(
            Math.Max(
                linearVolume,
                0.0001f));
    }

    public static void RefreshCustomMusicVolumes()
    {
        SetPlayerVolume(
            _battlePlayer,
            _battleVolumeDb);

        SetPlayerVolume(
            _fanfarePlayer,
            _fanfareVolumeDb);

        SetPlayerVolume(
            _gameOverPlayer,
            _gameOverVolumeDb);
    }

    private static void SetPlayerVolume(
        AudioStreamPlayer? player,
        float baseVolumeDb)
    {
        if (player == null)
            return;

        if (!GodotObject.IsInstanceValid(player))
            return;

        player.VolumeDb =
            ScaleDbByBgmSetting(
                baseVolumeDb);
    }

    // ============================================================
    // BATTLE MUSIC
    // ============================================================

    public static void Play(
        ulong netId,
        int actIndex,
        RoomType roomType)
    {
        if (!RydiaConfig.EnableCustomBattleMusic)
            return;

        if (!IsLocalPlayer(netId))
            return;

        StopBattleMusic();
        StopFanfare();
        StopGameOverMusic();

        StopVanillaMusicOnly();

        MusicTrack[] pool =
            GetPool(
                actIndex,
                roomType);

        if (pool.Length == 0)
        {
            ResumeVanillaMusic();
            return;
        }

        MusicTrack track =
            pool[
                Random.Shared.Next(
                    pool.Length)];

        AudioStream? stream =
            GD.Load<AudioStream>(
                track.Path);

        if (stream == null)
        {
            GD.PrintErr(
                $"Failed to load Rydia battle music: " +
                $"{track.Path}");

            ResumeVanillaMusic();
            return;
        }

        if (stream is AudioStreamMP3 mp3)
        {
            mp3.Loop = true;
        }

        _battleVolumeDb =
            track.VolumeDb;

        var player =
            new AudioStreamPlayer
            {
                Name = "RydiaBattleMusic",
                Stream = stream,
                Bus = "Master",
                VolumeDb =
                    ScaleDbByBgmSetting(
                        track.VolumeDb)
            };

        _battlePlayer =
            player;

        player.Finished += () =>
        {
            if (ReferenceEquals(
                    _battlePlayer,
                    player))
            {
                _battlePlayer = null;
            }

            if (GodotObject.IsInstanceValid(player))
            {
                player.QueueFree();
            }
        };

        Node? parent =
            NRun.Instance;

        if (parent == null)
            parent = NGame.Instance;

        if (parent == null)
        {
            _battlePlayer = null;

            player.QueueFree();

            ResumeVanillaMusic();
            return;
        }

        parent.AddChild(player);
        player.Play();

        GD.Print(
            $"Playing looping Rydia music: " +
            $"{track.Path} at {track.VolumeDb} dB");
    }

    private static void StopBattleMusic()
    {
        StopPlayer(
            ref _battlePlayer);

        _battleVolumeDb = 0f;
    }

    // ============================================================
    // VICTORY FANFARE
    // ============================================================

    public static void PlayVictoryFanfare(
        ulong netId,
        bool isBoss)
    {
        if (!RydiaConfig.EnableCustomVictoryMusic)
            return;

        if (!IsLocalPlayer(netId))
            return;

        StopBattleMusic();
        StopFanfare();
        StopGameOverMusic();

        StopVanillaMusicOnly();

        MusicTrack track =
            isBoss
                ? VictoryFanfare
                : VictoryFanfareShort;

        AudioStream? stream =
            GD.Load<AudioStream>(
                track.Path);

        if (stream == null)
        {
            GD.PrintErr(
                $"Failed to load Rydia victory fanfare: " +
                $"{track.Path}");

            return;
        }

        _fanfareVolumeDb =
            track.VolumeDb;

        var player =
            new AudioStreamPlayer
            {
                Name = "RydiaVictoryFanfare",
                Stream = stream,
                Bus = "Master",
                VolumeDb =
                    ScaleDbByBgmSetting(
                        track.VolumeDb)
            };

        _fanfarePlayer =
            player;

        player.Finished += () =>
        {
            if (ReferenceEquals(
                    _fanfarePlayer,
                    player))
            {
                _fanfarePlayer = null;
            }

            if (GodotObject.IsInstanceValid(player))
            {
                player.QueueFree();
            }
        };

        Node? parent =
            NRun.Instance;

        if (parent == null)
            parent = NGame.Instance;

        if (parent == null)
        {
            _fanfarePlayer = null;

            player.QueueFree();
            return;
        }

        parent.AddChild(player);
        player.Play();

        GD.Print(
            $"Playing Rydia victory fanfare: " +
            $"{track.Path} at {track.VolumeDb} dB");
    }

    private static void StopFanfare()
    {
        StopPlayer(
            ref _fanfarePlayer);

        _fanfareVolumeDb = 0f;
    }

    // ============================================================
    // GAME OVER
    // ============================================================

    public static void PlayGameOverTheme()
    {
        if (!RydiaConfig.EnableCustomGameOverMusic)
            return;

        StopBattleMusic();
        StopFanfare();
        StopGameOverMusic();

        StopVanillaMusicOnly();

        MusicTrack track =
            GameOverTheme;

        AudioStream? stream =
            GD.Load<AudioStream>(
                track.Path);

        if (stream == null)
        {
            GD.PrintErr(
                $"Failed to load Rydia game over music: " +
                $"{track.Path}");

            return;
        }

        _gameOverVolumeDb =
            track.VolumeDb;

        var player =
            new AudioStreamPlayer
            {
                Name = "RydiaGameOverMusic",
                Stream = stream,
                Bus = "Master",
                VolumeDb =
                    ScaleDbByBgmSetting(
                        track.VolumeDb)
            };

        _gameOverPlayer =
            player;

        player.Finished += () =>
        {
            if (ReferenceEquals(
                    _gameOverPlayer,
                    player))
            {
                _gameOverPlayer = null;
            }

            if (GodotObject.IsInstanceValid(player))
            {
                player.QueueFree();
            }
        };

        Node? parent =
            NRun.Instance;

        if (parent == null)
            parent = NGame.Instance;

        if (parent == null)
        {
            _gameOverPlayer = null;

            player.QueueFree();
            return;
        }

        parent.AddChild(player);
        player.Play();

        GD.Print(
            $"Playing Rydia game over theme: " +
            $"{track.Path} at {track.VolumeDb} dB");
    }

    private static void StopGameOverMusic()
    {
        StopPlayer(
            ref _gameOverPlayer);

        _gameOverVolumeDb = 0f;
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    public static void Stop()
    {
        StopBattleMusic();
        StopFanfare();
        StopGameOverMusic();

        ResumeVanillaMusic();

        GD.Print(
            "Stopped all Rydia music");
    }

    private static void StopPlayer(
        ref AudioStreamPlayer? playerField)
    {
        AudioStreamPlayer? player =
            playerField;

        /*
         * Clear the field before stopping the node so any deferred
         * Finished callback cannot treat it as the active player.
         */
        playerField = null;

        if (player == null)
            return;

        if (!GodotObject.IsInstanceValid(player))
            return;

        player.Stop();
        player.QueueFree();
    }

    // ============================================================
    // POOL SELECTION
    // ============================================================

    private static MusicTrack[] GetPool(
        int actIndex,
        RoomType roomType)
    {
        if (roomType == RoomType.Boss)
        {
            return actIndex switch
            {
                1 => Act1Boss,
                2 => Act2Boss,
                _ => Act3Boss
            };
        }

        if (roomType == RoomType.Elite)
        {
            return actIndex switch
            {
                1 => Act1Elite,
                2 => Act2Elite,
                _ => Act3Elite
            };
        }

        return actIndex switch
        {
            1 => Act1Normal,
            2 => Act2Normal,
            _ => Act3Normal
        };
    }

    private static bool IsLocalPlayer(
        ulong netId)
    {
        return RunManager.Instance != null
            && RunManager.Instance.NetService != null
            && netId ==
               RunManager.Instance.NetService.NetId;
    }
}

// ================================================================
// PROCEED CLEANUP
// ================================================================

[HarmonyPatch(
    typeof(NProceedButton),
    "OnRelease")]
public static class StopRydiaMusicOnProceedPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        BattleTheme.Stop();
    }
}

// ================================================================
// MAIN MENU RESTORE
// ================================================================

[HarmonyPatch(
    typeof(NMainMenu),
    "_Ready")]
public static class RestoreRydiaMusicOnMainMenuPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        BattleTheme.Stop();
    }
}

// ================================================================
// BGM SLIDER SYNC
// ================================================================

[HarmonyPatch(
    typeof(NAudioManager),
    nameof(NAudioManager.SetBgmVol))]
public static class RefreshRydiaMusicVolumePatch
{
    [HarmonyPostfix]
    public static void Postfix(
        float __0)
    {
        BattleTheme.RefreshCustomMusicVolumes();
    }
}