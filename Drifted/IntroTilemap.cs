using System.Collections.Generic;
using Drifted.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

[IgnoreTypeMemberReorderingAttribute]
public class IntroTilemap : OOTilemap {
    public TextRecord[] TextRecords;

    private readonly List<Text> Texts = new();

    public virtual void LoadContent(ContentManager content, ScreenManager screenManager) {
        foreach (var textRecord in TextRecords) Texts.Add(textRecord.Create(screenManager));

        foreach (var text in Texts) text.LoadContent(content);
    }

    public override void Update(GameTime gameTime) {
        foreach (var text in Texts) text.Update(gameTime);

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        // Draw the map
        base.Draw(gameTime, spriteBatch);
    }

    public void DrawText(GameTime gameTime, SpriteBatch spriteBatch) {
        // Draw the texts
        foreach (var text in Texts) text.Draw(gameTime, spriteBatch);
    }
}