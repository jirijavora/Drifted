using Drifted.StateManagement;
using Microsoft.Xna.Framework;

namespace Drifted;

public class IntroScreen : GameScreen {
    private readonly ScreenManager screenManager;
    private IntroTilemap introTilemap;

    public IntroScreen(ScreenManager screenManager) {
        this.screenManager = screenManager;
    }

    public override void Activate() {
        base.Activate();

        introTilemap = Content.Load<IntroTilemap>("DriftedIntro");
        introTilemap.LoadContent(Content, screenManager);
    }

    public override void Unload() {
        Content.Unload();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
        introTilemap.Update(gameTime);
    }

    public override void HandleInput(GameTime gameTime, InputState input) {
        if (input.Action) ScreenManager.ReplaceScreen(this, new Level("DriftedTrack"));
    }

    public override void Draw(GameTime gameTime) {
        var spriteBatch = ScreenManager.SpriteBatch;

        var scaleMatrix = Matrix.CreateScale(0.25f);
        spriteBatch.Begin(transformMatrix: scaleMatrix);

        introTilemap.Draw(gameTime, spriteBatch);

        spriteBatch.End();
    }
}