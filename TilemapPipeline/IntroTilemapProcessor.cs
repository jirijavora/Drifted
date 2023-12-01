using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace TilemapPipeline;

[ContentProcessor(DisplayName = "Intro Tilemap Processor")]
public class IntroTilemapProcessor : ContentProcessor<TiledMapContent, OOTilemapContent> {
    public override IntroTilemapContent Process(TiledMapContent input, ContentProcessorContext context) {
        IntroTilemapContent introTilemap = new();

        // Use the OOTilemapProcessor to load the tile layers
        OOTilemapProcessor processor = new();
        var ooMap = processor.Process(input, context);
        introTilemap.Layers = ooMap.Layers;
        introTilemap.Tilesize = ooMap.Tilesize;
        introTilemap.Size = ooMap.Size;
        introTilemap.Texture = ooMap.Texture;


        context.Logger.LogMessage(input.ObjectGroups.Count + " object groups found");
        foreach (var objG in input.ObjectGroups) context.Logger.LogMessage(objG.Name);

        // Get the object objectgroup from the Tiled data
        var objGroup = input.ObjectGroups.Find(group => group.Name == "Objects");
        context.Logger.LogMessage("Objects is " + objGroup.Name + $"({objGroup.Objects.Count})");
        introTilemap.Texts = new TextContent[objGroup.Objects.Count];


        var texts = objGroup.Objects.FindAll(obj => obj.Type == "Text");


        for (var i = 0; i < texts.Count; i++) {
            context.Logger.LogMessage($"Text value: '{texts[i].Properties["value"]}'");
            introTilemap.Texts[i] = new TextContent
                { Color = null, Position = new Vector2(texts[i].X, texts[i].Y), Value = texts[i].Properties["value"] };
        }


        var titles = objGroup.Objects.FindAll(obj => obj.Type == "Title");
        for (var i = 0; i < titles.Count; i++) {
            context.Logger.LogMessage($"Title value: '{titles[i].Properties["value"]}'");
            introTilemap.Texts[i + texts.Count] = new TitleContent {
                Color = null, Position = new Vector2(titles[i].X, titles[i].Y),
                Value = titles[i].Properties["value"]
            };
        }

        // Return the processed map
        return introTilemap;
    }
}