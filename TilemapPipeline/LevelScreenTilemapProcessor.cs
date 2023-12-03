using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace TilemapPipeline;

[ContentProcessor(DisplayName = "Level Screen Tilemap Processor")]
public class LevelScreenTilemapProcessor : ContentProcessor<TiledMapContent, OOTilemapContent> {
    public override LevelScreenTilemapContent Process(TiledMapContent input, ContentProcessorContext context) {
        LevelScreenTilemapContent levelScreenTilemap = new();

        // Use the OOTilemapProcessor to load the tile layers
        OOTilemapProcessor processor = new();
        var ooMap = processor.Process(input, context);
        levelScreenTilemap.Layers = ooMap.Layers;
        levelScreenTilemap.Tilesize = ooMap.Tilesize;
        levelScreenTilemap.Size = ooMap.Size;
        levelScreenTilemap.Texture = ooMap.Texture;


        context.Logger.LogMessage(input.ObjectGroups.Count + " object groups found");
        foreach (var objG in input.ObjectGroups) context.Logger.LogMessage(objG.Name);

        // Get the object objectgroup from the Tiled data
        var objGroup = input.ObjectGroups.Find(group => group.Name == "Objects");
        context.Logger.LogMessage("Objects is " + objGroup.Name + $"({objGroup.Objects.Count})");


        var name = objGroup.Objects.Find(obj => obj.Type == "LevelName");
        context.Logger.LogMessage(name == null ? "Level name not found" : "");
        levelScreenTilemap.nameCenter = new Vector2(name.X, name.Y);
        context.Logger.LogMessage(name != null ? $"Level name X:{name.X} Y:{name.Y}" : "");

        var bronzeTime = objGroup.Objects.Find(obj => obj.Type == "BronzeTime");
        levelScreenTilemap.bronzeTimeCenter = new Vector2(bronzeTime.X, bronzeTime.Y);

        var silverTime = objGroup.Objects.Find(obj => obj.Type == "SilverTime");
        levelScreenTilemap.silverTimeCenter = new Vector2(silverTime.X, silverTime.Y);

        var goldTime = objGroup.Objects.Find(obj => obj.Type == "GoldTime");
        levelScreenTilemap.goldTimeCenter = new Vector2(goldTime.X, goldTime.Y);


        // Return the processed map
        return levelScreenTilemap;
    }
}