using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ExampleGame
{
    public class HeroTilemap : OOTilemap
    {
        public Hero Hero { get; init; }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw the map
            base.Draw(gameTime, spriteBatch);

            // Draw the hero
            Hero.Draw(gameTime, spriteBatch);
        }
    }
}
