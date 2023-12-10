using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

public class IntroCar {
    private const float SpriteRotation = MathHelper.PiOver2;

    private const int MinPosY = 1100;
    private const int MaxPosY = 2100;

    private const float XSpeedMin = 300;
    private const float XSpeedMax = 800;

    private const int MaxPosX = 5120;
    private Vector2 _center;
    private Vector2 _direction = Vector2.Zero;
    private Vector2 _position;

    private Texture2D _texture;

    private bool active;


    public void LoadContent(ContentManager content) {
        var carVariant = RandomHelper.Next(1, 6);
        _texture = content.Load<Texture2D>($"car_blue_{carVariant}");
        _center = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
    }


    public void Update(GameTime gameTime) {
        if (!active) return;

        _position += _direction * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_position.X - _center.X > MaxPosX) active = false;
    }

    public void Activate() {
        if (active) return;

        _position.X = -_center.X;
        _position.Y = RandomHelper.Next(MinPosY, MaxPosY);
        _direction = new Vector2(RandomHelper.NextFloat(XSpeedMin, XSpeedMax), 0);
        active = true;
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        if (!active) return;

        spriteBatch.Draw(_texture, _position, null, Color.White, SpriteRotation, _center, Vector2.One,
            SpriteEffects.None,
            0);
    }
}