using Drifted.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

[IgnoreTypeMemberReorderingAttribute]
public class Button {
    private ButtonTilemap tilemap;

    private readonly Vector2 center;
    private readonly string text;

    public Button(Vector2 center, string text) {
        this.center = center;
        this.text = text;
    }


    public string getText() {
        return text;
    }

    public void LoadContent(ContentManager content, ScreenManager screenManager) {
        tilemap = content.Load<ButtonTilemap>("Button");
        tilemap.LoadContent(content, screenManager);
    }

    public void Update(GameTime gameTime) {
        tilemap.Update(gameTime);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch, float sizeMultiplier) {
        tilemap.Draw(gameTime, spriteBatch, center, text, sizeMultiplier);
    }
}