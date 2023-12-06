using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Drifted.StateManagement;

/// <summary>
///     The ScreenManager is a component which manages one or more GameScreen instance.
///     It maintains a stack of screens, calls their Update and Draw methods when
///     appropriate, and automatically routes input to the topmost screen.
/// </summary>
public class ScreenManager : DrawableGameComponent {
    public const float MaxMediaVolumeChangeStep = 0.01f;
    private readonly ContentManager content;

    public readonly Game game;
    private readonly InputManager inputManager = new();
    private readonly List<GameScreen> screens = new();
    private readonly List<GameScreen> tmpScreensList = new();

    private Song backgroundMusic;

    private bool isInitialized;
    public float MediaVolume = 0.8f;

    /// <summary>
    ///     Constructs a new ScreenManager
    /// </summary>
    /// <param name="game">The game this ScreenManager belongs to</param>
    public ScreenManager(Game game) : base(game) {
        this.game = game;
        content = new ContentManager(game.Services, "Content");
    }

    /// <summary>
    ///     A SpriteBatch shared by all GameScreens
    /// </summary>
    public SpriteBatch SpriteBatch { get; private set; }

    /// <summary>
    ///     A SpriteFont shared by all GameScreens
    /// </summary>
    public SpriteFont Font { get; private set; }

    /// <summary>
    ///     A SpriteFont shared by all GameScreens
    /// </summary>
    public SpriteFont FontLarge { get; private set; }

    /// <summary>
    ///     A SpriteFont shared by all GameScreens
    /// </summary>
    public SpriteFont FontSmall { get; private set; }

    /// <summary>
    ///     A blank texture that can be used by the screens.
    /// </summary>
    public Texture2D BlankTexture { get; private set; }

    /// <summary>
    ///     Initializes the ScreenManager
    /// </summary>
    public override void Initialize() {
        base.Initialize();
        isInitialized = true;
    }

    /// <summary>
    ///     Loads content for the ScreenManager and its screens
    /// </summary>
    protected override void LoadContent() {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        Font = content.Load<SpriteFont>("MagnetoBold");
        FontLarge = content.Load<SpriteFont>("MagnetoBoldLarge");
        FontSmall = content.Load<SpriteFont>("MagnetoBoldSmall");
        BlankTexture = content.Load<Texture2D>("Blank");

        StartMusic();

        // Tell each of the screens to load their content 
        foreach (var screen in screens) screen.Activate();
    }


    private void StartMusic() {
        backgroundMusic = content.Load<Song>("awake10_megaWall");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(backgroundMusic);
        MediaPlayer.Volume = MediaVolume;
    }

    /// <summary>
    ///     Unloads content for the ScreenManager's screens
    /// </summary>
    protected override void UnloadContent() {
        foreach (var screen in screens) screen.Unload();
    }

    /// <summary>
    ///     Updates all screens managed by the ScreenManager
    /// </summary>
    /// <param name="gameTime">An object representing time in the game</param>
    public override void Update(GameTime gameTime) {
        inputManager.Update();

        // Make a copy of the screen list, to avoid confusion if 
        // the process of updating a screen adds or removes others
        tmpScreensList.Clear();
        tmpScreensList.AddRange(screens);

        var otherScreenHasFocus = !Game.IsActive;
        var coveredByOtherScreen = false;

        while (tmpScreensList.Count > 0) {
            // Pop the topmost screen 
            var screen = tmpScreensList[tmpScreensList.Count - 1];
            tmpScreensList.RemoveAt(tmpScreensList.Count - 1);

            if ((screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.Active) &&
                !otherScreenHasFocus)
                screen.HandleInput(gameTime, inputManager.Input);


            screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);


            if (screen.ScreenState == ScreenState.TransitionOn || screen.ScreenState == ScreenState.Active) {
                // if this is the first active screen, let it handle input 
                if (!otherScreenHasFocus) otherScreenHasFocus = true;

                // if this is an active non-popup, all subsequent 
                // screens are covered 
                if (!screen.IsPopup)
                    coveredByOtherScreen = true;
            }
        }
    }

    /// <summary>
    ///     Draws the appropriate screens managed by the SceneManager
    /// </summary>
    /// <param name="gameTime">An object representing time in the game</param>
    public override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.Black);
        MediaPlayer.Volume = MediaVolume;

        foreach (var screen in screens) {
            if (screen.ScreenState == ScreenState.Hidden)
                continue;

            screen.Draw(gameTime);
        }
    }

    /// <summary>
    ///     Adds a screen to the ScreenManager
    /// </summary>
    /// <param name="screen">The screen to add</param>
    public void AddScreen(GameScreen screen) {
        screen.ScreenManager = this;
        screen.IsExiting = false;

        // If we have a graphics device, tell the screen to load content
        if (isInitialized)
            screen.Activate();

        screens.Add(screen);
    }

    public void RemoveScreen(GameScreen screen) {
        // If we have a graphics device, tell the screen to unload its content 
        if (isInitialized)
            screen.Unload();

        screens.Remove(screen);
        tmpScreensList.Remove(screen);
    }

    public void ReplaceScreen(GameScreen currentScreen, GameScreen newScreen) {
        newScreen.ScreenManager = this;
        newScreen.IsExiting = false;

        if (isInitialized) {
            currentScreen.Unload();
            newScreen.Activate();
        }

        var foundIndex = screens.FindIndex(screen => screen == currentScreen);
        if (foundIndex != -1) screens[foundIndex] = newScreen;
    }

    /// <summary>
    ///     Exposes an array holding all the screens managed by the ScreenManager
    /// </summary>
    /// <returns>An array containing references to all current screens</returns>
    public GameScreen[] GetScreens() {
        return screens.ToArray();
    }

    // Helper draws a translucent black fullscreen sprite, used for fading
    // screens in and out, and for darkening the background behind popups.
    public void FadeBackBufferToBlack(float alpha) {
        SpriteBatch.Begin();
        SpriteBatch.Draw(BlankTexture, GraphicsDevice.Viewport.Bounds, Color.Black * alpha);
        SpriteBatch.End();
    }

    public void Deactivate() { }

    public bool Activate() {
        return false;
    }
}