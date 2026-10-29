using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Rydia.RydiaCode.Extensions;

public static class RydiaStaticHoverTips
{
    public static readonly IHoverTip Summon =
        new HoverTip(
            new LocString(
                "static_hover_tips",
                "RYDIA_SUMMON.title"),
            new LocString(
                "static_hover_tips",
                "RYDIA_SUMMON.description"));
    
    public static readonly IHoverTip Eidolon =
        new HoverTip(
            new LocString(
                "static_hover_tips",
                "RYDIA_EIDOLON.title"),
            new LocString(
                "static_hover_tips",
                "RYDIA_EIDOLON.description"));
}