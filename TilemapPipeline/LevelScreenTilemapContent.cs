using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TilemapPipeline;

/// <summary>
///     A class representing an Intro tilemap
/// </summary>
[ContentSerializerRuntimeType("Drifted.LevelScreenTilemap, Drifted")]
public class LevelScreenTilemapContent : OOTilemapContent {
    public Vector2 nameCenter;
    public Vector2 bronzeTimeCenter;
    public Vector2 silverTimeCenter;
    public Vector2 goldTimeCenter;
}