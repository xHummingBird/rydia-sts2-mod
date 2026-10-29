using BaseLib.Abstracts;
using Rydia.RydiaCode.Extensions;
using Godot;

namespace Rydia.RydiaCode.Character;

public class RydiaPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Rydia.Color;


    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}