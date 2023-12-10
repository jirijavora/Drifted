using Drifted.StateManagement;
using Microsoft.Xna.Framework;

namespace Drifted;

public class MenuScreen : GameScreen {
    private readonly ScreenManager screenManager;

    private MenuTilemap menuTilemap;

    public MenuScreen(ScreenManager screenManager) {
        this.screenManager = screenManager;
    }

    public override void Activate() {
        base.Activate();

        menuTilemap = Content.Load<MenuTilemap>("DriftedMenu");
        menuTilemap.LoadContent(Content, screenManager);

        screenManager.MediaVolume = 0.55f;
    }

    public override void Unload() {
        Content.Unload();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
        if (otherScreenHasFocus) return;

        menuTilemap.Update(gameTime);
    }

    public override void HandleInput(GameTime gameTime, InputState input) {
        if (input.Escape) ScreenManager.AddScreen(new MenuPauseScreen(screenManager));
        if (input.Action) ScreenManager.ReplaceScreen(this, new Level(menuTilemap.getLevelName()));

        if (input.RightPress) menuTilemap.selectedUp(gameTime);
        if (input.LeftPress) menuTilemap.selectedDown(gameTime);
    }

    public override void Draw(GameTime gameTime) {
        var spriteBatch = ScreenManager.SpriteBatch;

        var scaleMatrix = Matrix.CreateScale(0.25f);
        spriteBatch.Begin(transformMatrix: scaleMatrix);

        menuTilemap.Draw(gameTime, spriteBatch);
        menuTilemap.DrawText(gameTime, spriteBatch);

        spriteBatch.End();
    }
}