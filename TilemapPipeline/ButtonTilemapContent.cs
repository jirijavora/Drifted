using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace TilemapPipeline;

/// <summary>
///     A class representing an button tilemap
/// </summary>
[ContentSerializerRuntimeType("Drifted.ButtonTilemap, Drifted")]
public class ButtonTilemapContent : OOTilemapContent {
    public Vector2 textCenter;
}