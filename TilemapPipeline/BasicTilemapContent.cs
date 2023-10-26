using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilemapPipeline
{
    /// <summary>
    /// A game-specific implementation of a tilemap. This is a very limited
    /// representation, which only allows for one layer and one tileset.
    /// </summary>
    [ContentSerializerRuntimeType("ExampleGame.BasicTilemap, ExampleGame")]
    public class BasicTilemapContent
    {
        /// <summary>Map dimensions</summary>
        public int MapWidth, MapHeight;

        /// <summary>Tile dimensions</summary>
        public int TileWidth, TileHeight;

        /// <summary>The tileset texture</summary>
        public ExternalReference<Texture2DContent> TilesetTexture;

        /// <summary>The tileset data</summary>
        public Rectangle[] Tiles;

        /// <summary>The map data</summary>
        public int[] TileIndices;

        /// <summary> The tileset image filename </summary>
        [ContentSerializerIgnore]
        public String TilesetImageFilename;
    }
}
