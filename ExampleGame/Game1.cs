using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ExampleGame
{
    public enum MapType
    {
        Basic,
        ObjectOriented,
        Hero
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;
        private BasicTilemap _basicMap;
        private OOTilemap _ooMap;
        private HeroTilemap _heroMap;
        private MapType _mapType = MapType.Basic;
        private KeyboardState _priorKeyboardState;
       

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load content items
            _spriteFont = Content.Load<SpriteFont>("Arial");
            _basicMap = Content.Load<BasicTilemap>("BasicMapTest");
            _ooMap = Content.Load<OOTilemap>("MapTest");
            _heroMap = Content.Load<HeroTilemap>("HeroMapTest");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if(_mapType == MapType.ObjectOriented) _ooMap.Update(gameTime);
            if(_mapType == MapType.Hero) _heroMap.Update(gameTime);

            // TODO: Add your update logic here
            var keyboardState = Keyboard.GetState();
            if(keyboardState.IsKeyDown(Keys.Space) && _priorKeyboardState.IsKeyUp(Keys.Space))
            {
                _mapType += 1;
                if ((int)_mapType > 2) _mapType = 0;
            }
            _priorKeyboardState = keyboardState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            switch (_mapType)
            {
                case MapType.Basic:
                    _basicMap.Draw(gameTime, _spriteBatch);
                    break;
                case MapType.ObjectOriented:
                    _ooMap.Draw(gameTime, _spriteBatch);
                    break;
                case MapType.Hero:
                    _heroMap.Draw(gameTime, _spriteBatch);
                    break;
            }

            _spriteBatch.DrawString(_spriteFont, $"{_mapType} Tilemap", new Vector2(50, 50), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}