using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace TilemapPipeline;

[ContentProcessor(DisplayName = "Drifted Tilemap Processor")]
public class DriftedTilemapProcessor : ContentProcessor<TiledMapContent, OOTilemapContent> {
    public override DriftedTilemapContent Process(TiledMapContent input, ContentProcessorContext context) {
        DriftedTilemapContent driftedMap = new();

        // Use the OOTilemapProcessor to load the tile layers
        OOTilemapProcessor processor = new();
        var ooMap = processor.Process(input, context);
        driftedMap.Layers = ooMap.Layers;

        // Create our hero object 
        driftedMap.Player = new PlayerContent();

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

        // Build the hero texture
        var texture =
            context.BuildAndLoadAsset<TextureContent, Texture2DContent>(new ExternalReference<TextureContent>(path),
                "TextureProcessor");

        // Save the Player in the HeroTilemapContent object 
        driftedMap.Player = new PlayerContent {
            Position = new Vector2(player.X, player.Y),
            Texture = texture
        };

        context.Logger.LogMessage($"Player is on position {player.X}, {player.Y}");

        // Return the processed map
        return driftedMap;
    }
}