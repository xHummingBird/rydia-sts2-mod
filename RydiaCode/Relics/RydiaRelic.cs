using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Rydia.RydiaCode.Character;
using Rydia.RydiaCode.Extensions;
using Godot;

namespace Rydia.RydiaCode.Relics;

[Pool(typeof(RydiaRelicPool))]
public abstract class RydiaRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();

    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".BigRelicImagePath();

    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}