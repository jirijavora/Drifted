using Drifted.StateManagement;
using Microsoft.Xna.Framework;

namespace Drifted;

public class IntroScreen : GameScreen {
    private const int IntroCarInverseActiveChance = 200;
    private readonly ScreenManager screenManager;

    private IntroCar[] introCars;
    private IntroTilemap introTilemap;

    public IntroScreen(ScreenManager screenManager) {
        this.screenManager = screenManager;
    }

    public override void Activate() {
        base.Activate();

        introTilemap = Content.Load<IntroTilemap>("DriftedIntro");
        introTilemap.LoadContent(Content, screenManager);

        introCars = new IntroCar[5];
        for (var i = 0; i < introCars.Length; i++) {
            introCars[i] = new IntroCar();
            introCars[i].LoadContent(Content);
        }
    }

    public override void Unload() {
        Content.Unload();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
        introTilemap.Update(gameTime);
        foreach (var car in introCars) {
            if (RandomHelper.Next(IntroCarInverseActiveChance) == 0) car.Activate();
            car.Update(gameTime);
        }
    }

    public override void HandleInput(GameTime gameTime, InputState input) {
        if (input.Escape) screenManager.game.Exit();
        if (input.Action) ScreenManager.ReplaceScreen(this, new MenuScreen(screenManager));
    }

    public override void Draw(GameTime gameTime) {
        var spriteBatch = ScreenManager.SpriteBatch;

        var scaleMatrix = Matrix.CreateScale(0.25f);
        spriteBatch.Begin(transformMatrix: scaleMatrix);

        introTilemap.Draw(gameTime, spriteBatch);
        foreach (var car in introCars) car.Draw(gameTime, spriteBatch);

        introTilemap.DrawText(gameTime, spriteBatch);

        spriteBatch.End();
    }
}