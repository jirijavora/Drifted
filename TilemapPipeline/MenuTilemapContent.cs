using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TilemapPipeline;

/// <summary>
///     A class representing level info
/// </summary>
[ContentSerializerRuntimeType("Drifted.LevelRecord, Drifted")]
public class LevelContent {
    public Vector2 Position;
    public string Value;
}

/// <summary>
///     A class representing an Intro tilemap
/// </summary>
[ContentSerializerRuntimeType("Drifted.MenuTilemap, Drifted")]
public class MenuTilemapContent : IntroTilemapContent {
    public LevelContent[] Levels;
}