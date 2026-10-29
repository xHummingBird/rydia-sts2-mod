using MegaCrit.Sts2.Core.Commands;

namespace Rydia.RydiaCode.Extensions;

public static class AudioHelper
{
    private static readonly Random rng = new Random();
    
    private static readonly string[] AttackSfx =
    {
        "res://Rydia/sounds/attack (1).wav",
        "res://Rydia/sounds/attack (2).wav",
        "res://Rydia/sounds/attack (3).wav",
    };
    
    private static readonly string[] DamagedSfx =
    {
        "res://Rydia/sounds/damage_low (1).wav",
        "res://Rydia/sounds/damage_low (2).wav",
        
    };
    
    private static readonly string[] HighDamagedSfx =
    {
        "res://Rydia/sounds/damage_high (1).wav",
        "res://Rydia/sounds/damage_high (2).wav",
    };
    
    private static readonly string[] CriticalDamagedSfx =
    {
        "res://Rydia/sounds/damage_critical (1).wav",
        "res://Rydia/sounds/damage_critical (2).wav",
    };
    
    private static readonly string[] VictorySfx =
    {
        "res://Rydia/sounds/victory (1).wav",
        "res://Rydia/sounds/victory (2).wav",
        "res://Rydia/sounds/victory (3).wav",
        "res://Rydia/sounds/victory (4).wav",
    };

    private static readonly string[] GameoverSfx =
    {
        "res://Rydia/sounds/gameover.wav"
    };
    
    private static readonly string[] MagicSfx =
    {
        "res://Rydia/sounds/magic (1).wav",
        "res://Rydia/sounds/magic (2).wav",
        "res://Rydia/sounds/magic (3).wav",
        "res://Rydia/sounds/magic (4).wav",
    };

    private static readonly string[] SummonSfx =
    {
        "res://Rydia/sounds/summon (1).wav",
        "res://Rydia/sounds/summon (2).wav",
        "res://Rydia/sounds/summon (3).wav",
        "res://Rydia/sounds/summon (4).wav",
        "res://Rydia/sounds/summon (5).wav",
        "res://Rydia/sounds/summon (6).wav",
        "res://Rydia/sounds/summon (7).wav",
    };
    
    
    
    public static void PlayRandomAttack()
    {
        PlayRandom(AttackSfx);
    }
    
    public static void PlayRandomSummon()
    {
        PlayRandom(SummonSfx);
    }

    public static void PlayRandomMagic()
    {
        PlayRandom(MagicSfx);
    }
    
    public static void PlayRandomDamaged()
    {
        PlayRandom(DamagedSfx);
    }

    public static void PlayRandomDamagedHigh()
    {
        PlayRandom(HighDamagedSfx);
    }

    public static void PlayRandomGameover()
    {
        PlayRandom(GameoverSfx);
    }

    public static void PlayRandomDamagedCritical()
    {
        PlayRandom(CriticalDamagedSfx);
    }
    
    public static void PlayRandomVictory()
    {
        PlayRandom(VictorySfx);
    }
    

    public static void PlayRandom(string[] pool)
    {
        int index = rng.Next(pool.Length);
        SfxCmd.Play(pool[index]);
    }
}