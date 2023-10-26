using Microsoft.Xna.Framework.Content.Pipeline;
using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using MonoGame.Framework.Utilities.Deflate;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection.Emit;

namespace TilemapPipeline
{
    /// <summary>
    /// An importer for Tiled tilemaps.  It creates a TiledMapContent object, which is
    /// a near-exact reflection of the structure of a tiled file as C# objects.
    /// This should be paired with a ContentProcessor that uses this information to 
    /// populate a game-specific tilemap implementation.
    /// </summary>
    [ContentImporter(".tmx", DisplayName = "Tiled Importer")]
    public class TiledImporter : ContentImporter<TiledMapContent>
    {
        /// <summary>
        /// Imports a Tiled map file in the .tmx format.  This is an XML-based file format 
        /// that is documented here: https://doc.mapeditor.org/en/stable/reference/tmx-map-format/
        /// </summary>
        /// <param name="filename">The name of the TMX file</param>
        /// <param name="context">A context for the importing</param>
        /// <returns>The loaded TilemapContent object</returns>
        /// <exception cref="Exception"></exception>
        public override TiledMapContent Import(string filename, ContentImporterContext context)
        {
            XmlReaderSettings settings = new();
            settings.DtdProcessing = DtdProcessing.Parse;

            using var stream = System.IO.File.OpenText(filename);
            using XmlReader reader = XmlReader.Create(stream, settings);

            var map = new TiledMapContent();

            while (reader.Read())
            {
                var name = reader.Name;

                switch(reader.NodeType)
                {
                    case XmlNodeType.DocumentType:
                        if (name != "map")
                            throw new Exception("Invalid Map Format");
                        break;
                    case XmlNodeType.Element:
                        switch(name)
                        {
                            case "map":
                                {
                                    map.Width = int.Parse(reader.GetAttribute("width"));
                                    map.Height = int.Parse(reader.GetAttribute("height"));
                                    map.TileWidth = int.Parse(reader.GetAttribute("tilewidth"));
                                    map.TileHeight = int.Parse(reader.GetAttribute("tileheight"));
                                }
                                break;
                            case "tileset":
                                {
                                    using var st = reader.ReadSubtree();
                                    st.Read();
                                    map.Tilesets.Add(LoadTileset(st));
                                }
                                break;
                            case "layer":
                                {
                                    using var st = reader.ReadSubtree();
                                    st.Read();
                                    map.TileLayers.Add(LoadLayer(st));
                                }
                                break;
                            case "objectgroup":
                                {
                                    using var st = reader.ReadSubtree();
                                    st.Read();
                                    map.ObjectGroups.Add(LoadObjectGroup(st));
                                }
                                break;
                            case "properties":
                                {
                                    using var st = reader.ReadSubtree();
                                    st.Read();
                                    map.Properties = LoadProperties(st);
                                    break;
                                }
                            default:
                                context.Logger.LogMessage($"Unhandled XML Element {name}");
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                            break;
                    case XmlNodeType.Whitespace:
                            break;
                    case XmlNodeType.XmlDeclaration:
                            break;
                    default:
                        context.Logger.LogMessage($"Unhandled XML Node {name}");
                        break;
                }
            }

            return map;
        }

        /// <summary>
        /// Constants for flags used by Tiled to indicate flipped tiles
        /// </summary>
        private const uint FlippedHorizontallyFlag = 0x80000000;
        private const uint FlippedVerticallyFlag = 0x40000000;
        private const uint FlippedDiagonallyFlag = 0x20000000;

        /// <summary>
        /// Loads a TilesetContent from a `<tileset>` element
        /// </summary>
        /// <param name="reader">The XML Reader</param>
        /// <returns>The loaded TilesetContent</returns>
        private TilesetContent LoadTileset(XmlReader reader)
        {
            TilesetContent tileset = new();

            // Load required attributes
            tileset.Name = reader.GetAttribute("name");
            tileset.FirstTileId = int.Parse(reader.GetAttribute("firstgid"));
            tileset.TileWidth = int.Parse(reader.GetAttribute("tilewidth"));
            tileset.TileHeight = int.Parse(reader.GetAttribute("tileheight"));
            
            // Load optional attributes
            int.TryParse(reader.GetAttribute("margin"), out tileset.Margin);
            int.TryParse(reader.GetAttribute("spacing"), out tileset.Spacing);

            int currentTileId = -1;

            // Process the nested nodes
            while(reader.Read())
            {
                var name = reader.Name;

                switch(reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch(name)
                        {
                            case "image":
                                tileset.ImageFilename = reader.GetAttribute("source");
                                break;
                            case "tile":
                                currentTileId = int.Parse(reader.GetAttribute("id"));
                                break;
                            case "properties":
                                {
                                    using var st = reader.ReadSubtree();
                                    st.Read();
                                    tileset.Properties = LoadProperties(st);
                                    break;
                                }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        break;
                }
            }

            return tileset;
        }

        /// <summary>
        /// Loads a TiledLayerContent from an `<Layer>` element
        /// </summary>
        /// <param name="reader">The XML Reader</param>
        /// <returns>The loaded TiledLayerContent</returns>
        public TiledLayerContent LoadLayer(XmlReader reader)
        {
            TiledLayerContent layer = new();

            layer.Name = reader.GetAttribute("name");
            int.TryParse(reader.GetAttribute("width"), out layer.Width);
            int.TryParse(reader.GetAttribute("height"), out layer.Height);
            float.TryParse(reader.GetAttribute("opacity"), out layer.Opacity);

            layer.TileIndices = new int[layer.Width * layer.Height];
            layer.SpriteEffects = new SpriteEffects[layer.Width * layer.Height];

            while(reader.Read())
            {
                var name = reader.Name;

                switch(reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch(name)
                        {
                            case "data":
                                {
                                    if(reader.GetAttribute("encoding") != null)
                                    {
                                        var encoding = reader.GetAttribute("encoding");
                                        var compressor = reader.GetAttribute("compression");
                                        switch (encoding)
                                        {
                                            case "base64":
                                                {
                                                    int dataSize = (layer.Width * layer.Height * 4) + 1024;
                                                    var buffer = new byte[dataSize];
                                                    reader.ReadElementContentAsBase64(buffer, 0, dataSize);

                                                    Stream stream = new MemoryStream(buffer, false);
                                                    if (compressor == "gzip")
                                                        stream = new GZipStream(stream, CompressionMode.Decompress, false);

                                                    using (stream)
                                                    using (var br = new BinaryReader(stream))
                                                    {
                                                        for (int i = 0; i < layer.TileIndices.Length; i++)
                                                        {
                                                            uint tileData = br.ReadUInt32();

                                                            SpriteEffects spriteEffects = 0;
                                                            if ((tileData & FlippedHorizontallyFlag) != 0)
                                                            {
                                                                spriteEffects |= SpriteEffects.FlipHorizontally;
                                                            }
                                                            if ((tileData & FlippedVerticallyFlag) != 0)
                                                            {
                                                                spriteEffects |= SpriteEffects.FlipVertically;
                                                            }
                                                            if ((tileData & FlippedDiagonallyFlag) != 0)
                                                            {
                                                                spriteEffects |= SpriteEffects.FlipVertically & SpriteEffects.FlipHorizontally;
                                                            }

                                                            layer.SpriteEffects[i] = spriteEffects;

                                                            // Clear flipped bits before storing tile data
                                                            tileData &= ~(FlippedHorizontallyFlag | FlippedVerticallyFlag | FlippedDiagonallyFlag);

                                                            layer.TileIndices[i] = (int)tileData;
                                                        }
                                                    }
                                                    continue;
                                                }

                                            default:
                                                throw new Exception("Unrecognized encoding.");
                                        }
                                    }
                                    else
                                    {
                                        // TODO: Read tiles directly
                                    }
                                    break;
                                }
                            case "properties":
                                {
                                    using var st = reader.ReadSubtree();
                                    st.Read();
                                    layer.Properties = LoadProperties(st);
                                    break;
                                }

                        }
                        break;
                    case XmlNodeType.EndElement:
                        break;
                }
            }

            // Return the loaded layer
            return layer;
        }

        /// <summary>
        /// Loads a TiledObjectGroupContent from an `<ObjectGroup>` element
        /// </summary>
        /// <param name="reader">The XML Reader</param>
        /// <returns>The loaded TiledObjectGroupContent</returns>
        public TiledObjectGroupContent LoadObjectGroup(XmlReader reader)
        {
            TiledObjectGroupContent group = new();

            group.Name = reader.GetAttribute("name");
            int.TryParse(reader.GetAttribute("x"), out group.X);
            int.TryParse(reader.GetAttribute("y"), out group.Y);
            int.TryParse(reader.GetAttribute("width"), out group.Width);
            int.TryParse(reader.GetAttribute("height"), out group.Height);
            float.TryParse(reader.GetAttribute("opacity"), out group.Opacity);

            while (reader.Read())
            {
                var name = reader.Name;

                switch(reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch(name)
                        {
                            case "object":
                                {
                                    using var st = reader.ReadSubtree();
                                    st.Read();
                                    group.Objects.Add(LoadObject(st));
                                }
                                break;
                            case "properties":
                                {
                                    using var st = reader.ReadSubtree();
                                    st.Read();
                                    group.Properties = LoadProperties(st);
                                }
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        break;
                }
            }

            return group;
        }

        /// <summary>
        /// Loads a TiledObjectContent from an `<object>` element
        /// </summary>
        /// <param name="reader">The XMLReader</param>
        /// <returns>The populated TiledObjectContent</returns>
        public TiledObjectContent LoadObject(XmlReader reader)
        {
            TiledObjectContent obj = new();

            obj.Name = reader.GetAttribute("name");
            obj.Type = reader.GetAttribute("type");
            int.TryParse(reader.GetAttribute("x"), out obj.Y);
            int.TryParse(reader.GetAttribute("y"), out obj.X);
            int.TryParse(reader.GetAttribute("width"), out obj.Width);
            int.TryParse(reader.GetAttribute("height"), out obj.Height);
            float.TryParse(reader.GetAttribute("rotation"), out obj.Rotation);
            bool.TryParse(reader.GetAttribute("visisble"), out obj.Visible);

            while (reader.Read())
            {
                var name = reader.Name;

                if (name == "properties")
                {
                    using var st = reader.ReadSubtree();
                    st.Read();
                    obj.Properties = LoadProperties(st);
                }
            }

            return obj;
        }

        /// <summary>
        /// Loads properties from a `<properties>` element
        /// </summary>
        /// <param name="reader">The XML reader</param>
        /// <returns>The properties as a dictionary</returns>
        public Dictionary<string, string> LoadProperties(XmlReader reader)
        {
            Dictionary<string, string> properties = new();

            while (reader.Read())
            {
                var name = reader.Name;

                if(name == "property")
                {
                    string key = reader.GetAttribute("name");
                    string value = reader.GetAttribute("value");
                    properties.Add(key, value);
                }
            }          


            return properties;
        }
    }
}