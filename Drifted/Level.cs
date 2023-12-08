using Drifted.StateManagement;
using Microsoft.Xna.Framework;

namespace Drifted;

public class Level : GameScreen {
    private readonly string levelName;
    private GameTilemap tilemap;

    private bool wasHidden;

    public Level(string levelName) {
        this.levelName = levelName;
    }

    public override void Activate() {
        base.Activate();

        tilemap = Content.Load<GameTilemap>(levelName);
        tilemap.LoadContent(Content, ScreenManager);

        ScreenManager.MediaVolume = 0.25f;
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
        if (coveredByOtherScreen) {
            wasHidden = true;
            return;
        }

        if (wasHidden) {
            tilemap.Resume();
            wasHidden = false;
        }

        tilemap.Update(gameTime, levelName);
    }

    public override void HandleInput(GameTime gameTime, InputState input) {
        if (input.Escape) {
            ScreenManager.AddScreen(new PauseScreen(ScreenManager, this));
            tilemap.Pause();
        }

        base.HandleInput(gameTime, input);
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