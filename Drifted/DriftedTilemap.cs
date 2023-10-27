using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

public class DriftedTilemap : OOTilemap {
    public Player Player { get; init; }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        // Draw the map
        base.Draw(gameTime, spriteBatch);

        // Draw the hero
        Player.Draw(gameTime, spriteBatch);
    }
}