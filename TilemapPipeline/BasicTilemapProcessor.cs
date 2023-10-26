using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework;
using System.Linq;

namespace TilemapPipeline
{
    /// <summary>
    /// Processes a TiledMapContent into a BasicTilemapContent object, building and linking the associated texture 
    /// and setting up the tile information. The BasicTilemapContent only supports one tile layer and one tileset,
    /// so any additional information in the TiledMapContent is discarded.
    /// </summary>
    [ContentProcessor(DisplayName = "BasicTilemapProcessor")]
    public class BasicTilemapProcessor : ContentProcessor<TiledMapContent, BasicTilemapContent>
    {
        /// <summary>
        /// Processes the TiledMapContent into a BasicTilemapContent
        /// </summary>
        /// <param name="map"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override BasicTilemapContent Process(TiledMapContent map, ContentProcessorContext context)
        {
            // We'll be copying data from the Tiled MapContent object into an output BasicTilemapContent object
            BasicTilemapContent output = new();

            // Copy the map and tile dimensions to the BasicTilemapContent
            output.MapHeight = map.Height;
            output.MapWidth = map.Width;
            output.TileHeight = map.TileHeight;
            output.TileWidth = map.TileWidth;

            // The BasicTilemap only supports one tilemap - if we have more, we'll 
            // want to use a different processor and output type
            if (map.Tilesets.Count() > 0) context.Logger.LogMessage("BasicTilemapProcessor only supports one tileset. Consider using a different processor and output object.");
            TilesetContent tileset = map.Tilesets[0];

            // We need to build the tileset texture associated with this tilemap
            // As it may be used by multiple tilemaps, we build it as an ExternalReference,
            // That way it has its own .xnb file of data that can be shared by all tilemaps.
            output.TilesetTexture = context.BuildAsset<TextureContent, Texture2DContent>(new ExternalReference<TextureContent>(tileset.ImageFilename), "TextureProcessor");

            // However, we *also* need to know the image's size. So we'll also build it directly
            // so we can access that data.  This reference will not be passsed down the pipeline
            Texture2DContent image = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(new ExternalReference<TextureContent>(tileset.ImageFilename), "TextureProcessor");

            // Now we can determine the width and height of the texture
            // The first mipmap will be the original texture.
            int textureWidth = image.Mipmaps[0].Width;
            int textureHeight = image.Mipmaps[0].Height;

            // With the width and height, we can determine the number of tiles in the texture
            // Note that Tiled allows for tilesets with both margins and spacing between tiles
            int tilesetColumns = (textureWidth - 2 * tileset.Margin) / (map.TileWidth + tileset.Spacing);
            int tilesetRows = (textureHeight - 2 * tileset.Margin) / (map.TileHeight + tileset.Spacing);

            // We need to create the bounds for each tile in the tileset image
            // These will be stored in the tiles array
            output.Tiles = new Rectangle[tilesetColumns * tilesetRows];

            for (int y = 0; y < tilesetRows; y++)
            {
                for (int x = 0; x < tilesetColumns; x++)
                {
                    // The Tiles array provides the source rectangle for a tile
                    // within the tileset texture
                    output.Tiles[y * tilesetColumns + x] = new Rectangle(
                        x * (map.TileWidth + tileset.Spacing) + tileset.Margin,
                        y * (map.TileHeight + tileset.Spacing) + tileset.Margin,
                        map.TileWidth,
                        map.TileHeight
                        );
                }
            }

            // The BasicTilemap also supports only one layer
            if(map.TileLayers.Count() > 0) context.Logger.LogMessage("BasicTilemapProcessor only supports one tile layer. Consider using a different processor and output object.");
            
            // Copy the first layer's indices into the BasicTilemapContent
            output.TileIndices = map.TileLayers[0].TileIndices;

            // Return the fully processed tilemap
            return output;
        }
    }
}
