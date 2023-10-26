using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace TilemapPipeline
{
    /// <summary>
    /// A class representing an individual object from an ObjectGroup in Tiled
    /// </summary>
    public class TiledObjectContent
    {
        /// <summary>
        /// The id of the object
        /// </summary>
        public int Id;

        /// <summary>
        /// The name of the object
        /// </summary>
        public string Name;

        /// <summary>
        /// The type of the object 
        /// </summary>
        public string Type;

        /// <summary>
        /// The x position of the object
        /// </summary>
        public int X;

        /// <summary>
        /// The y position of the object
        /// </summary>
        public int Y;

        /// <summary>
        /// The width of the object
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the object
        /// </summary>
        public int Height;

        /// <summary>
        /// A reference to a tile
        /// </summary>
        public int? Gid;

        /// <summary>
        /// If the object is visible
        /// </summary>
        public bool Visible = true;

        /// <summary>
        /// A rotation value for the object
        /// </summary>
        public float Rotation;

        // TODO: Template

        /// <summary>
        /// The properties of the object
        /// </summary>
        public Dictionary<string, string> Properties = new();
    }
}
