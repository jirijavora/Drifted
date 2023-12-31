﻿using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace TilemapPipeline;

/// <summary>
///     A class representing the `<layer>` element from a tiled tilemap.
/// </summary>
public class TiledLayerContent {
    /// <summary>
    ///     The layer height
    /// </summary>
    public int Height;

    /// <summary>
    ///     The layer name
    /// </summary>
    public string Name;

    /// <summary>
    ///     The layer opacity
    /// </summary>
    public float Opacity;

    /// <summary>
    ///     The layer properties
    /// </summary>
    public Dictionary<string, string> Properties;

    /// <summary>
    ///     Rotation applied to tiles in the layer
    /// </summary>
    public float[] Rotation;

    /// <summary>
    ///     SpriteEffects applied to tiles in the layer
    /// </summary>
    public SpriteEffects[] SpriteEffects;

    /// <summary>
    ///     The indices of all tiles in the layer
    /// </summary>
    public int[] TileIndices;

    /// <summary>
    ///     The layer width
    /// </summary>
    public int Width;
}