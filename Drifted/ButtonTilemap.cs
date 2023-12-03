using Drifted.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

[IgnoreTypeMemberReorderingAttribute]
public class ButtonTilemap : OOTilemap {
    public Vector2 textCenter;
    private SpriteFont font;


    public void LoadContent(ContentManager content, ScreenManager screenManager) {
        font = screenManager.Font;
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 center, string text, float sizeMultiplier) {
        var drawCenter = new Vector2(center.X - Size.X * Tilesize.X / 2, center.Y - Size.Y * Tilesize.Y / 2);

        // Draw the level background
        var scaleMatrix = Matrix.CreateScale(0.25f);
        var transformMatrix =
            Matrix.CreateScale(1 + sizeMultiplier) * Matrix.CreateTranslation(
                                                       -sizeMultiplier * Size.X * Tilesize.X / 2,
                                                       -sizeMultiplier * Size.Y * Tilesize.Y / 2, 0)
                                                   *
                                                   Matrix.CreateTranslation(drawCenter.X, drawCenter.Y, 0) *
                                                   scaleMatrix;
        spriteBatch.End();
        spriteBatch.Begin(transformMatrix: transformMatrix);
        base.Draw(gameTime, spriteBatch);

        var textDims = font.MeasureString(text);

        spriteBatch.DrawString(font, text, textCenter - textDims / 2f, Color.White);

        spriteBatch.End();
        spriteBatch.Begin(transformMatrix: scaleMatrix);
    }
}