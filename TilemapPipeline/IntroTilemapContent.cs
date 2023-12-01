using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TilemapPipeline;

/// <summary>
///     A class representing an on screen text
/// </summary>
[ContentSerializerRuntimeType("Drifted.TextRecord, Drifted")]
public class TextContent {
    public Color? Color;
    public Vector2 Position;
    public string Value;
}

/// <summary>
///     A class representing an on screen title
/// </summary>
[ContentSerializerRuntimeType("Drifted.TitleRecord, Drifted")]
public class TitleContent : TextContent { }

/// <summary>
///     A class representing an Intro tilemap
/// </summary>
[ContentSerializerRuntimeType("Drifted.IntroTilemap, Drifted")]
public class IntroTilemapContent : OOTilemapContent {
    public TextContent[] Texts;
}