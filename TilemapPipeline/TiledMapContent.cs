using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TilemapPipeline
{
    public class TiledMapContent
    {
        /// <summary>
        /// The width of the map (in tiles)
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the map (in tiles)
        /// </summary>
        public int Height;

        /// <summary>
        /// The width of tiles in the map (in pixels)
        /// </summary>
        public int TileWidth;

        /// <summary>
        /// The height of the tiles in the map (in pixels)
        /// </summary>
        public int TileHeight;

        /// <summary>
        /// A background color to use in the map
        /// </summary>
        public Color BackgroundColor;

        /// <summary>
        /// The tile layers in the map
        /// </summary>
        public List<TiledLayerContent> TileLayers = new();

        /// <summary>
        /// The tilesets used by the map
        /// </summary>
        public List<TilesetContent> Tilesets = new();

        /// <summary>
        /// The object layers in the map
        /// </summary>
        public List<TiledObjectGroupContent> ObjectGroups = new();

        /// <summary>
        /// The properties of the map
        /// </summary>
        public Dictionary<string, string> Properties = new();
    }
}
