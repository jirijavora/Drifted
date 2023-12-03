using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace TilemapPipeline;

/// <summary>
///     A class representing a Player's information
/// </summary>
[ContentSerializerRuntimeType("Drifted.Player, Drifted")]
public class PlayerContent {
    /// <summary>
    ///     The hero's position in the map
    /// </summary>
    public Vector2 Position;

    /// <summary>
    ///     The hero's texture
    /// </summary>
    public Texture2DContent Texture;

    public Vector2 Center;

    public float Rotation;

    public TimeSpan bronzeTime;
    public TimeSpan silverTime;
    public TimeSpan goldTime;
}

/// <summary>
///     A class representing a Tilemap with a hero in it
/// </summary>
[ContentSerializerRuntimeType("Drifted.GameTilemap, Drifted")]
public class GameTilemapContent : OOTilemapContent {
    /// <summary>
    ///     The hero's starting location (and appearance) in the map
    /// </summary>
    public PlayerContent Player = new();
}