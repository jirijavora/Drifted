using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilemapPipeline
{
    [ContentProcessor(DisplayName = "HeroTilemap Processor")]
    public class HeroTilemapProcessor : ContentProcessor<TiledMapContent, OOTilemapContent>
    {
        public override HeroTilemapContent Process(TiledMapContent input, ContentProcessorContext context)
        {

            HeroTilemapContent heroMap = new();

            // Use the OOTilemapProcessor to load the tile layers
            OOTilemapProcessor processor = new();
            OOTilemapContent ooMap = processor.Process(input, context);
            heroMap.Layers = ooMap.Layers;

            // Create our hero object 
            heroMap.Hero = new();

            context.Logger.LogMessage(input.ObjectGroups.Count + " object groups found");
            foreach(var objG in input.ObjectGroups)
            {
                context.Logger.LogMessage(objG.Name);
            }

            // Get the sprites objectgroup from the Tiled data
            var objGroup = input.ObjectGroups.Find(group => (group.Name == "sprites"));
            context.Logger.LogMessage("Sprites is " + objGroup.Name);

            // Get the hero object from the Tiled data 
            var heroObj = objGroup.Objects.Find(obj => obj.Name == "hero");
            context.Logger.LogMessage("Hero is " + heroObj.Name);

            // Get the hero's image path from the hero object's properties
            string path = heroObj.Properties["image"];
            context.Logger.LogMessage("Image is " + path);

            // Build the hero texture
            Texture2DContent texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(new ExternalReference<TextureContent>(path), "TextureProcessor");

            // Save the Hero in the HeroTilemapContent object 
            heroMap.Hero = new()
            {
                Position = new Vector2(heroObj.X, heroObj.Y),
                Texture = texture
            };

            // Return the processed map
            return heroMap;
        }
    }
}
