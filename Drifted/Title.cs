using Drifted.StateManagement;
using Microsoft.Xna.Framework;

namespace Drifted;

public class Title : Text {
    public Title(ScreenManager screenManager, TitleRecord record) : base(screenManager, record) {
        font = screenManager.FontLarge;
    }

    public Title(ScreenManager screenManager, Vector2 position, string value, Color? colorOverride = null) : base(
        screenManager, position, value, colorOverride) {
        font = screenManager.FontLarge;
    }
}