using Drifted.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Drifted;

[IgnoreTypeMemberReorderingAttribute]
public class LevelRecord {
    public Vector2 position;
    public string value;

    public virtual LevelScreen Create(ContentManager content, ScreenManager screenManager) {
        var levelScreen = new LevelScreen(position, value);
        levelScreen.LoadContent(content, screenManager);
        return levelScreen;
    }
}