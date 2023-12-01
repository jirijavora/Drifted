using Drifted.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

[IgnoreTypeMemberReorderingAttribute]
public class Text {
    public Color? color;
    public Vector2 position;
    public string value;

    protected SpriteFont font;

    public Text(ScreenManager screenManager, TextRecord record) {
        font = screenManager.Font;
        position = record.position;
        value = record.value;
        color = Color.White;
    }

    public Text(ScreenManager screenManager, Color? colorOverride = null) {
        font = screenManager.Font;
        color = colorOverride ?? color ?? Color.White;
    }


    public void LoadContent(ContentManager content) { }

    public void Update(GameTime gameTime) { }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        var textDims = font.MeasureString(value);

        spriteBatch.DrawString(font, value, position - textDims / 2f, color.Value);
    }
}