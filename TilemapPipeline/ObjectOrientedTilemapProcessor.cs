using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace TilemapPipeline;

[ContentProcessor(DisplayName = "OOTilemap Processor")]
internal class OOTilemapProcessor : ContentProcessor<TiledMapContent, OOTilemapContent> {
    public override OOTilemapContent Process(TiledMapContent input, ContentProcessorContext context) {
        // Create our output object
        OOTilemapContent output = new();

        // Convert the tilesets into TileInfo structs
        var tiles = ProcessTilesets(input.TileWidth, input.TileHeight, input.Tilesets, context);

        // Process the layers using the processed tiles
        output.Layers = ProcessLayers(input.TileLayers, tiles, input.TileWidth, input.TileHeight, context).ToArray();

        output.Size = new Vector2(input.Width, input.Height);
        output.Tilesize = new Vector2(input.TileWidth, input.TileHeight);
        output.Texture = context.BuildAsset<TextureContent, Texture2DContent>(
            new ExternalReference<TextureContent>(input.Tilesets[0].ImageFilename), "TextureProcessor");

        return output;
    }

    /// <summary>
    ///     This helper method processes the tilesets in a Tiled map,
    ///     returning an array of TileInfo objects populated with thier
    ///     data.
    /// </summary>
    /// <param name="tileWidth"></param>
    /// <param name="tileHeight"></param>
    /// <param name="tilesets"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    private List<TileInfo> ProcessTilesets(int tileWidth, int tileHeight, List<TilesetContent> tilesets,
        ContentProcessorContext context) {
        // Create a list 
        List<TileInfo> processedTiles = new();

        foreach (var tileset in tilesets) {
            // Load the texture as an external reference, so we can embed it
            // into our tiles
            var texture =
                context.BuildAsset<TextureContent, Texture2DContent>(
                    new ExternalReference<TextureContent>(tileset.ImageFilename), "TextureProcessor");

            // Also load the texture directly so we can access its dimensions
            var image = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                new ExternalReference<TextureContent>(tileset.ImageFilename), "TextureProcessor");

            // Now we can determine the width and height of the texture
            // The first mipmap will be the original texture.
            var textureWidth = image.Mipmaps[0].Width;
            var textureHeight = image.Mipmaps[0].Height;

            // With the width and height, we can determine the number of tiles in the texture
            // Note that Tiled allows for tilesets with both margins and spacing between tiles
            var tilesetColumns = (textureWidth - 2 * tileset.Margin) / (tileWidth + tileset.Spacing);
            var tilesetRows = (textureHeight - 2 * tileset.Margin) / (tileHeight + tileset.Spacing);

            // We need to create the bounds for each tile in the tileset image
            for (var y = 0; y < tilesetRows; y++)
            for (var x = 0; x < tilesetColumns; x++)
                // The Tiles array provides the source rectangle for a tile
                // within the tileset texture, and the texture
                processedTiles.Add(new TileInfo {
                    Texture = texture,
                    SourceRect = new Rectangle(
                        x * (tileWidth + tileset.Spacing) + tileset.Margin,
                        y * (tileHeight + tileset.Spacing) + tileset.Margin,
                        tileWidth,
                        tileHeight
                    ),
                    Center = new Vector2(tileWidth / 2f, tileHeight / 2f)
                });
        }

        // We can then return the processed tiles
        return processedTiles;
    }

    private List<OOTilemapLayerContent> ProcessLayers(List<TiledLayerContent> layers, List<TileInfo> tileInfoList,
        int tileWidth, int tileHeight, ContentProcessorContext context) {
        List<OOTilemapLayerContent> processedLayers = new();

        foreach (var layer in layers) {
            // Create a list to hold the tiles we will be generating
            List<OOTilemapTileContent> tiles = new();

            // For each tile in our layer, we need to create a corresponding
            // object to store in our new layers
            for (var y = 0; y < layer.Height; y++)
            for (var x = 0; x < layer.Width; x++) {
                // Calculate the 1d index for our 2d position
                var index = y * layer.Width + x;

                // Find the specific tile index for this tile
                // in the map. Remember, a 0 stands for no tile, 
                // so we shift by 1.
                var tileIndex = layer.TileIndices[index] - 1;

                // If the tileIndex is -1, then there is no tile
                // at this spot in the map. We need to have *something*
                // there, so we'll fill it with a basic tile
                // and move on to the next iteration
                if (tileIndex == -1) {
                    tiles.Add(new OOTilemapTileContent());
                    continue;
                }

                // Otherwise, we need to create a textured tile for this
                // tile location.
                var tileInfo = tileInfoList[tileIndex];
                tiles.Add(new OOTexturedTileContent {
                    Texture = tileInfo.Texture,
                    SourceRect = tileInfo.SourceRect,
                    Center = tileInfo.Center,
                    WorldRect = new Rectangle {
                        X = x * tileWidth,
                        Y = y * tileHeight,
                        Width = tileWidth,
                        Height = tileHeight
                    },
                    SpriteEffects = layer.SpriteEffects[index],
                    Rotation = layer.Rotation[index]
                });
            }

            // Add the processed layer to the collection
            processedLayers.Add(new OOTilemapLayerContent {
                Tiles = tiles.ToArray(),
                Opacity = layer.Opacity
            });
        }

        return processedLayers;
    }

    private struct TileInfo {
        public ExternalReference<Texture2DContent> Texture;

        public Rectangle SourceRect;

        public Vector2 Center;
    }
}