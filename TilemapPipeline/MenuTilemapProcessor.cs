using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace TilemapPipeline;

[ContentProcessor(DisplayName = "Menu Tilemap Processor")]
public class MenuTilemapProcessor : ContentProcessor<TiledMapContent, OOTilemapContent> {
    public override MenuTilemapContent Process(TiledMapContent input, ContentProcessorContext context) {
        MenuTilemapContent menuTilemap = new();

        IntroTilemapProcessor introProcessor = new();
        var introMap = introProcessor.Process(input, context);
        menuTilemap.Layers = introMap.Layers;
        menuTilemap.Tilesize = introMap.Tilesize;
        menuTilemap.Size = introMap.Size;
        menuTilemap.Texture = introMap.Texture;
        menuTilemap.Texts = introMap.Texts;


        context.Logger.LogMessage(input.ObjectGroups.Count + " object groups found");
        foreach (var objG in input.ObjectGroups) context.Logger.LogMessage(objG.Name);

        // Get the object objectgroup from the Tiled data
        var objGroup = input.ObjectGroups.Find(group => group.Name == "Objects");
        context.Logger.LogMessage("Objects is " + objGroup.Name + $"({objGroup.Objects.Count})");


        var levels = objGroup.Objects.FindAll(obj => obj.Type == "LevelTile");
        menuTilemap.Levels = new LevelContent[levels.Count];
        for (var i = 0; i < levels.Count; i++) {
            context.Logger.LogMessage($"Level value: '{levels[i].Properties["value"]}'");
            menuTilemap.Levels[i] = new LevelContent {
                Position = new Vector2(levels[i].X, levels[i].Y),
                Value = levels[i].Properties["value"]
            };
        }

        // Return the processed map
        return menuTilemap;
    }
}