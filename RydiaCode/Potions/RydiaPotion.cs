using BaseLib.Abstracts;
using BaseLib.Utils;
using Rydia.RydiaCode.Character;

namespace Rydia.RydiaCode.Potions;

[Pool(typeof(RydiaPotionPool))]
public abstract class RydiaPotion : CustomPotionModel;