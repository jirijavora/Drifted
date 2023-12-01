using Drifted.StateManagement;
using Microsoft.Xna.Framework;

namespace Drifted;

[IgnoreTypeMemberReorderingAttribute]
public class TextRecord {
    public Color? color;
    public Vector2 position;
    public string value;

    public virtual Text Create(ScreenManager screenManager) {
        return new Text(screenManager, this);
    }
}