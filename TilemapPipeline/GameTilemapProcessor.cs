using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace TilemapPipeline;

[ContentProcessor(DisplayName = "Game Tilemap Processor")]
public class GameTilemapProcessor : ContentProcessor<TiledMapContent, OOTilemapContent> {
    public override GameTilemapContent Process(TiledMapContent input, ContentProcessorContext context) {
        GameTilemapContent gameTilemap = new();

        // Use the OOTilemapProcessor to load the tile layers
        OOTilemapProcessor processor = new();
        var ooMap = processor.Process(input, context);
        gameTilemap.Layers = ooMap.Layers;
        gameTilemap.Tilesize = ooMap.Tilesize;
        gameTilemap.Size = ooMap.Size;
        gameTilemap.Texture = ooMap.Texture;

        // Create our hero object 
        gameTilemap.Player = new PlayerContent();

        context.Logger.LogMessage(input.ObjectGroups.Count + " object groups found");
        foreach (var objG in input.ObjectGroups) context.Logger.LogMessage(objG.Name);

        // Get the object objectgroup from the Tiled data
        var objGroup = input.ObjectGroups.Find(group => group.Name == "Objects");
        context.Logger.LogMessage("Objects is " + objGroup.Name);

        // Get the hero object from the Tiled data 
        var player = objGroup.Objects.Find(obj => obj.Name == "Player");
        context.Logger.LogMessage("Player is " + player.Name);

        // Get the player's image path from the hero object's properties
        var path = player.Properties["image"];
        context.Logger.LogMessage("Image is " + path);

        var rotation = float.Parse(player.Properties["rotation"]);

        // Build the hero texture
        var texture =
            context.BuildAndLoadAsset<TextureContent, Texture2DContent>(new ExternalReference<TextureContent>(path),
                "TextureProcessor");

        var center = new Vector2(texture.Mipmaps[0].Width / 2f, texture.Mipmaps[0].Height / 2f);

        context.Logger.LogMessage("Gold time is " +
                                  double.Parse(player.Properties["gold"], CultureInfo.InvariantCulture));
        context.Logger.LogMessage("Silver time is " +
                                  double.Parse(player.Properties["silver"], CultureInfo.InvariantCulture));
        context.Logger.LogMessage("Bronze time is " +
                                  double.Parse(player.Properties["bronze"], CultureInfo.InvariantCulture));

        // Save the Player in the HeroTilemapContent object 
        gameTilemap.Player = new PlayerContent {
            Position = new Vector2(player.X, player.Y) - center,
            Texture = texture,
            Center = center,
            Rotation = MathHelper.ToRadians(rotation),
            goldTime = TimeSpan.FromSeconds(double.Parse(player.Properties["gold"], CultureInfo.InvariantCulture)),
            silverTime =
                TimeSpan.FromSeconds(double.Parse(player.Properties["silver"], CultureInfo.InvariantCulture)),
            bronzeTime = TimeSpan.FromSeconds(double.Parse(player.Properties["bronze"], CultureInfo.InvariantCulture))
        };

        context.Logger.LogMessage($"Player is on position {player.X}, {player.Y}");

        // Return the processed map
        return gameTilemap;
    }
}