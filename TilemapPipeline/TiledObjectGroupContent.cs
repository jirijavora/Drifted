using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilemapPipeline
{
    public class TiledObjectGroupContent
    {
        public string Name;

        public int Width;

        public int Height;

        public int X;

        public int Y;

        public float Opacity;

        public List<TiledObjectContent> Objects = new();

        public Dictionary<string, string> Properties = new();
    }
}
