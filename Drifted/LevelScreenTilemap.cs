using System;
using System.Collections.Generic;
using Drifted.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

[IgnoreTypeMemberReorderingAttribute]
public class LevelScreenTilemap : OOTilemap {
    public Vector2 nameCenter;
    public Vector2 bronzeTimeCenter;
    public Vector2 silverTimeCenter;
    public Vector2 goldTimeCenter;

    private readonly List<Text> texts = new();

    private SpriteFont font;
    private SpriteFont fontSmall;


    public void LoadContent(ContentManager content, ScreenManager screenManager) {
        // texts.Add(new SmallText(screenManager, bronzeTimeCenter, "Bronze"));
        // texts.Add(new SmallText(screenManager, silverTimeCenter, "Silver"));
        // texts.Add(new SmallText(screenManager, goldTimeCenter, "Gold"));

        font = screenManager.Font;
        fontSmall = screenManager.FontSmall;
    }

    public override void Update(GameTime gameTime) {
        foreach (var text in texts) text.Update(gameTime);
        base.Update(gameTime);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Vector2 center, string name, TimeSpan? bestLapTime,
        string? medal,
        float sizeMultiplier) {
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
        foreach (var text in texts) text.Draw(gameTime, spriteBatch);

        var textDims = font.MeasureString(name);

        spriteBatch.DrawString(font, name, nameCenter - textDims / 2f, Color.White);

        var bestLapText = "[No lap set]";

        if (bestLapTime.HasValue)
            bestLapText = $"Best: {bestLapTime.Value.TotalSeconds:0.00}s" +
                          (medal != null && medal.Length > 0 ? $" ({medal})" : "");

        textDims = fontSmall.MeasureString(bestLapText);
        spriteBatch.DrawString(fontSmall, bestLapText, silverTimeCenter - textDims / 2f, Color.White);


        spriteBatch.End();
        spriteBatch.Begin(transformMatrix: scaleMatrix);
    }
}