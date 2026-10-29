using MegaCrit.Sts2.Core.Entities.Powers;

namespace Rydia.RydiaCode.Powers;

public class EvokePower : RydiaPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
}