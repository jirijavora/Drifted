using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace TilemapPipeline
{
    public class TilesetContent
    {
        public string Name;

        public int FirstTileId;

        public int TileWidth;

        public int TileHeight;

        public int Spacing;

        public int Margin;

        public string ImageFilename;

        public Texture2DContent Texture;

        public Dictionary<string, string> Properties = new();
    }
}
