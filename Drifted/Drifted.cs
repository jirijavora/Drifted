using Drifted.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Drifted;

public class Drifted : Game {
    private const int PreferredWidth = 1280;
    private const int PreferredHeight = 800;
    private readonly GraphicsDeviceManager _graphics;
    private readonly ScreenManager screenManager;


    public Drifted() {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "Drifted";

        var screenFactory = new ScreenFactory();
        Services.AddService(typeof(IScreenFactory), screenFactory);
        screenManager = new ScreenManager(this);
        Components.Add(screenManager);
    }

    private void AddInitialScreens() {
        screenManager.AddScreen(new IntroScreen(screenManager));
    }

    protected override void LoadContent() {
        _graphics.PreferredBackBufferWidth = PreferredWidth;
        _graphics.PreferredBackBufferHeight = PreferredHeight;
        _graphics.ApplyChanges();

        AddInitialScreens();
    }

    protected override void Update(GameTime gameTime) {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }
}