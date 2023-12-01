using Drifted.StateManagement;
using Microsoft.Xna.Framework;

namespace Drifted;

public class Level : GameScreen {
    private readonly string levelName;
    private GameTilemap tilemap;

    public Level(string levelName) {
        this.levelName = levelName;
    }

    public override void Activate() {
        base.Activate();

        tilemap = Content.Load<GameTilemap>(levelName);
        tilemap.LoadContent(Content);
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
        tilemap.Update(gameTime);
    }

    public override void Draw(GameTime gameTime) {
        base.Draw(gameTime);

        var spriteBatch = ScreenManager.SpriteBatch;

        var scaleMatrix = Matrix.CreateScale(0.25f);
        spriteBatch.Begin(transformMatrix: scaleMatrix);

        tilemap.Draw(gameTime, spriteBatch);

        spriteBatch.End();
    }
}