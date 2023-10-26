using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace TilemapPipeline
{
    /// <summary>
    /// A game-specific representation for a tilemap adopting object-oriented
    /// design ideas.
    /// Note that tileset information is instead embedded directly in the tiles
    /// in this approach.
    /// </summary>
    [ContentSerializerRuntimeType("ExampleGame.OOTilemap, ExampleGame")]
    public class OOTilemapContent
    {
        /// <summary>
        /// The tile layers, which are composed of tiles
        /// </summary>
        public OOTilemapLayerContent[] Layers { get; set; }
    }

    /// <summary>
    /// A representation of tile layers for the OOTilemapContent
    /// </summary>
    [ContentSerializerRuntimeType("ExampleGame.OOTilemapLayer, ExampleGame")]
    public class OOTilemapLayerContent
    {
        /// <summary>
        /// The tiles in the layer.
        /// </summary>
        public OOTilemapTileContent[] Tiles { get; set; }

        /// <summary>
        /// The opacity of the layer
        /// </summary>
        public float Opacity { get; set; } = 1.0f;

        /// <summary>
        /// The visibility of the layer
        /// </summary>
        public bool Visible { get; set; } = true;
    }

    /// <summary>
    /// The base tile representation for an OOTilemapContent.  
    /// Note that it contains no information - the base tile is
    /// a placeholder for a "no tile here".
    /// </summary>
    [ContentSerializerRuntimeType("ExampleGame.OOTilemapTile, ExampleGame")]
    public class OOTilemapTileContent
    {

    }

    /// <summary>
    /// A representation for a textured tile in the OOTilemapContent 
    /// </summary>
    [ContentSerializerRuntimeType("ExampleGame.OOTexturedTile, ExampleGame")]
    public class OOTexturedTileContent : OOTilemapTileContent
    {
        /// <summary>
        /// The portion of the Texture used for this tile
        /// </summary>
        public Rectangle SourceRect { get; set; }

        /// <summary>
        /// The bounds of the tile in the game world
        /// </summary>
        public Rectangle WorldRect { get; set; }

        /// <summary>
        /// The texture used by this tile
        /// </summary>
        public ExternalReference<Texture2DContent> Texture { get; init; }

        /// <summary>
        /// Indicates if this tile should be flipped in some way
        /// </summary>
        public SpriteEffects SpriteEffects { get; init; }
    }
}
