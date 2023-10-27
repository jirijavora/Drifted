using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

public class Player {
    public Vector2 Position { get; init; }

    public Texture2D Texture { get; init; }

    public void Update(GameTime gameTime) { }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        spriteBatch.Draw(Texture, Position, Color.White);
    }
}