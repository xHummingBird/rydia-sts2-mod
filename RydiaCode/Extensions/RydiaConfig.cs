using BaseLib.Config;

namespace Rydia.RydiaCode.Extensions;

public sealed class RydiaConfig : SimpleModConfig
{
    [ConfigSection("Audio")]
    public static bool EnableCustomBattleMusic { get; set; } = true;

    public static bool EnableCustomVictoryMusic { get; set; } = true;

    public static bool EnableCustomGameOverMusic { get; set; } = true;
}