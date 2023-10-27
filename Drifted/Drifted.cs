using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Drifted;

public class Drifted : Game {
    private readonly GraphicsDeviceManager _graphics;
    private DriftedTilemap _driftedMap;
    private SpriteBatch _spriteBatch;
    private SpriteFont _spriteFont;


    public Drifted() {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1280;
        _graphics.PreferredBackBufferHeight = 800;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize() {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent() {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load content items
        _spriteFont = Content.Load<SpriteFont>("Arial");

        _driftedMap = Content.Load<DriftedTilemap>("DriftedTrack");
    }

    protected override void Update(GameTime gameTime) {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();


        _driftedMap.Update(gameTime);


        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        var scaleMatrix = Matrix.CreateScale(0.25f);

        // TODO: Add your drawing code here
        _spriteBatch.Begin(transformMatrix: scaleMatrix);

        _driftedMap.Draw(gameTime, _spriteBatch);

        _spriteBatch.DrawString(_spriteFont, "Drifted Tilemap", new Vector2(50, 50), Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}