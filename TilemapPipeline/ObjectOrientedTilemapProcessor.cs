using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace TilemapPipeline
{
    [ContentProcessor(DisplayName = "OOTilemap Processor")]
    internal class OOTilemapProcessor : ContentProcessor<TiledMapContent, OOTilemapContent>
    {
        private struct TileInfo
        {
            public ExternalReference<Texture2DContent> Texture;

            public Rectangle SourceRect;
        }

        public override OOTilemapContent Process(TiledMapContent input, ContentProcessorContext context)
        {
            // Create our output object
            OOTilemapContent output = new();
                        
            // Convert the tilesets into TileInfo structs
            List<TileInfo> tiles = ProcessTilesets(input.TileWidth, input.TileHeight, input.Tilesets, context);

            // Process the layers using the processed tiles
            output.Layers = ProcessLayers(input.TileLayers, tiles, input.TileWidth, input.TileHeight, context).ToArray();

            return output;
        }

        /// <summary>
        /// This helper method processes the tilesets in a Tiled map,
        /// returning an array of TileInfo objects populated with thier
        /// data.
        /// </summary>
        /// <param name="tileWidth"></param>
        /// <param name="tileHeight"></param>
        /// <param name="tilesets"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private List<TileInfo> ProcessTilesets(int tileWidth, int tileHeight, List<TilesetContent> tilesets, ContentProcessorContext context)
        {
            // Create a list 
            List<TileInfo> processedTiles = new();

            foreach(TilesetContent tileset in tilesets)
            {
                // Load the texture as an external reference, so we can embed it
                // into our tiles
                ExternalReference<Texture2DContent> texture = context.BuildAsset<TextureContent, Texture2DContent>(new ExternalReference<TextureContent>(tileset.ImageFilename), "TextureProcessor");

                // Also load the texture directly so we can access its dimensions
                Texture2DContent image = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(new ExternalReference<TextureContent>(tileset.ImageFilename), "TextureProcessor");
                
                // Now we can determine the width and height of the texture
                // The first mipmap will be the original texture.
                int textureWidth = image.Mipmaps[0].Width;
                int textureHeight = image.Mipmaps[0].Height;

                // With the width and height, we can determine the number of tiles in the texture
                // Note that Tiled allows for tilesets with both margins and spacing between tiles
                int tilesetColumns = (textureWidth - 2 * tileset.Margin) / (tileWidth + tileset.Spacing);
                int tilesetRows = (textureHeight - 2 * tileset.Margin) / (tileHeight + tileset.Spacing);

                // We need to create the bounds for each tile in the tileset image
                for (int y = 0; y < tilesetRows; y++)
                {
                    for (int x = 0; x < tilesetColumns; x++)
                    {
                        // The Tiles array provides the source rectangle for a tile
                        // within the tileset texture, and the texture
                        processedTiles.Add(new TileInfo()
                        {
                            Texture = texture,
                            SourceRect = new Rectangle(
                                x * (tileWidth + tileset.Spacing) + tileset.Margin,
                                y * (tileHeight + tileset.Spacing) + tileset.Margin,
                                tileWidth,
                                tileHeight
                            )
                        });
                    }
                }
            }

            // We can then return the processed tiles
            return processedTiles;
        }

        private List<OOTilemapLayerContent> ProcessLayers(List<TiledLayerContent> layers, List<TileInfo> tileInfoList, int tileWidth, int tileHeight, ContentProcessorContext context)
        {
            List<OOTilemapLayerContent> processedLayers = new();

            foreach(var layer in layers)
            {
                // Create a list to hold the tiles we will be generating
                List<OOTilemapTileContent> tiles = new();

                // For each tile in our layer, we need to create a corresponding
                // object to store in our new layers
                for(int y = 0; y < layer.Height; y++)
                {
                    for(int x = 0; x < layer.Width; x++)
                    {
                        // Calculate the 1d index for our 2d position
                        int index = y * layer.Width + x;

                        // Find the specific tile index for this tile
                        // in the map. Remember, a 0 stands for no tile, 
                        // so we shift by 1.
                        int tileIndex = layer.TileIndices[index] - 1;
                        
                        // If the tileIndex is -1, then there is no tile
                        // at this spot in the map. We need to have *something*
                        // there, so we'll fill it with a basic tile
                        // and move on to the next iteration
                        if(tileIndex == -1)
                        {
                            tiles.Add(new OOTilemapTileContent());
                            continue;
                        }

                        // Otherwise, we need to create a textured tile for this
                        // tile location.
                        var tileInfo = tileInfoList[tileIndex];
                        tiles.Add(new OOTexturedTileContent()
                        {
                            Texture = tileInfo.Texture,
                            SourceRect = tileInfo.SourceRect,
                            WorldRect = new Rectangle()
                            {
                                X = x * tileWidth,
                                Y = y * tileHeight,
                                Width = tileWidth,
                                Height = tileHeight
                            },
                            SpriteEffects = layer.SpriteEffects[tileIndex]
                        });
                    }
                }

                // Add the processed layer to the collection
                processedLayers.Add(new OOTilemapLayerContent()
                {
                    Tiles = tiles.ToArray(),
                    Opacity = layer.Opacity
                });
            }

            return processedLayers;
        }
    }
}