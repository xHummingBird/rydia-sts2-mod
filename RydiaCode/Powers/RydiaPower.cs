using BaseLib.Abstracts;
using BaseLib.Extensions;
using Rydia.RydiaCode.Extensions;
using Godot;

namespace Rydia.RydiaCode.Powers;

public abstract class RydiaPower : CustomPowerModel
{
    //Loads from Rydia/images/powers/your_power.png
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}