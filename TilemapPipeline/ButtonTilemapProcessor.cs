using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace TilemapPipeline;

[ContentProcessor(DisplayName = "Button Tilemap Processor")]
public class ButtonTilemapProcessor : ContentProcessor<TiledMapContent, OOTilemapContent> {
    public override ButtonTilemapContent Process(TiledMapContent input, ContentProcessorContext context) {
        ButtonTilemapContent buttonTilemap = new();

        // Use the OOTilemapProcessor to load the tile layers
        OOTilemapProcessor processor = new();
        var ooMap = processor.Process(input, context);
        buttonTilemap.Layers = ooMap.Layers;
        buttonTilemap.Tilesize = ooMap.Tilesize;
        buttonTilemap.Size = ooMap.Size;
        buttonTilemap.Texture = ooMap.Texture;


        context.Logger.LogMessage(input.ObjectGroups.Count + " object groups found");
        foreach (var objG in input.ObjectGroups) context.Logger.LogMessage(objG.Name);

        // Get the object objectgroup from the Tiled data
        var objGroup = input.ObjectGroups.Find(group => group.Name == "Objects");
        context.Logger.LogMessage("Objects is " + objGroup.Name + $"({objGroup.Objects.Count})");


        var text = objGroup.Objects.Find(obj => obj.Type == "Text");
        buttonTilemap.textCenter = new Vector2(text.X, text.Y);


        // Return the processed map
        return buttonTilemap;
    }
}