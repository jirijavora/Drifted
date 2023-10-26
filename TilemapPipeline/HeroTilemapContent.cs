using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TilemapPipeline
{
    /// <summary>
    /// A class representing a Hero's information
    /// </summary>
    [ContentSerializerRuntimeType("ExampleGame.Hero, ExampleGame")]
    public class HeroContent
    {
        /// <summary>
        /// The hero's texture
        /// </summary>
        public Texture2DContent Texture;

        /// <summary>
        /// The hero's position in the map
        /// </summary>
        public Vector2 Position;
    }

    /// <summary>
    /// A class representing a Tilemap with a hero in it
    /// </summary>
    [ContentSerializerRuntimeType("ExampleGame.HeroTilemap, ExampleGame")]
    public class HeroTilemapContent : OOTilemapContent
    {
        /// <summary>
        /// The hero's starting location (and appearance) in the map
        /// </summary>
        public HeroContent Hero = new();
    }
}
