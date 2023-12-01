using Drifted.StateManagement;
using Microsoft.Xna.Framework;

namespace Drifted;

public class Title : Text {
    public Title(ScreenManager screenManager, TitleRecord record) : base(screenManager, record) {
        font = screenManager.FontLarge;
    }

    public Title(ScreenManager screenManager, Color? colorOverride = null) : base(screenManager, colorOverride) {
        font = screenManager.FontLarge;
    }
}