using Drifted.StateManagement;
using Microsoft.Xna.Framework;

namespace Drifted;

public class SmallText : Text {
    public SmallText(ScreenManager screenManager, Vector2 position, string value, Color? colorOverride = null) : base(
        screenManager, position, value, colorOverride) {
        font = screenManager.FontSmall;
    }
}