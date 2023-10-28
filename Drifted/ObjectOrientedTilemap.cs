using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

/// <summary>
///     This class demonstrates a more object-oriented approach to
///     tilemaps.  Each tile is an object, and we can use inheritance
///     to create different tile types.
///     While this will likely be less efficient than a pure struct-
///     and-array based approach, it provides a great deal of flexibility
///     and maintainability.
/// </summary>
public class OOTilemap {
    /// <summary>
    ///     The tilemap's layers
    /// </summary>
    public OOTilemapLayer[] Layers { get; init; }

    public Vector2 Size { get; init; }
    public Vector2 Tilesize { get; init; }
    public Texture2D Texture { get; init; }

    /// <summary>
    ///     Updates the tilemap layers
    /// </summary>
    /// <param name="gameTime">The game time</param>
    public virtual void Update(GameTime gameTime) {
        foreach (var layer in Layers) layer.Update(gameTime);
    }

    /// <summary>
    ///     Draws the tilemap layers
    /// </summary>
    /// <param name="gameTime">The current game time</param>
    /// <param name="spriteBatch">The SpriteBatch to draw with</param>
    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        foreach (var layer in Layers)
            if (layer.Visible)
                layer.Draw(gameTime, spriteBatch);
    }
}

/// <summary>
///     This class represents a layer in an OOTilemap
/// </summary>
public class OOTilemapLayer {
    /// <summary>
    ///     The tiles of the layer
    /// </summary>
    public OOTilemapTile[] Tiles { get; init; }

    /// <summary>
    ///     Applies a translucency to the layer.
    ///     A value of 1 is fully opaque, 0 fully transparent
    /// </summary>
    public float Opacity { get; set; }

    /// <summary>
    ///     Determines if the layer should be drawn
    /// </summary>
    public bool Visible { get; set; }

    /// <summary>
    ///     Updates all tiles in the layer
    /// </summary>
    /// <param name="gameTime">The current game time</param>
    public void Update(GameTime gameTime) {
        foreach (var tile in Tiles) tile.Update(gameTime);
    }

    /// <summary>
    ///     Draws all tiles in the layer
    /// </summary>
    /// <param name="gameTime">The current game time</param>
    /// <param name="spriteBatch">The SpriteBatch to draw with</param>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        if (!Visible) return;

        foreach (var tile in Tiles) tile.Draw(gameTime, spriteBatch);
    }
}

/// <summary>
///     A base class for all tiles - can also be used to
///     represent a "no tile" placeholder in a 2D array
///     of tiles when we are using tile indices to indicate
///     position.
/// </summary>
public class OOTilemapTile {
    /// <summary>
    ///     Updates the tile.  Essentially a no-op that can be overridden
    ///     in derived classes that need to update.
    /// </summary>
    /// <param name="gameTime"></param>
    public virtual void Update(GameTime gameTime) { }

    /// <summary>
    ///     Draws the tile. Essentially a no-op that can be overriden in
    ///     derived classes.
    /// </summary>
    /// <param name="gameTime">The current game time</param>
    /// <param name="spriteBatch">The SpriteBatch to draw the map with</param>
    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }

    public virtual void FillAreaOnMap(Color[] textureData, ref bool[] mapToFill, Vector2 mapDims) { }
}

/// <summary>
///     A tile that consists of a texture in a tileset image,
///     and a location in the world to draw it, as well as
///     SpriteEffects indicating if it should be flipped
/// </summary>
public class OOTexturedTile : OOTilemapTile {
    /// <summary>
    ///     The bounds of the tile in the Texture
    /// </summary>
    public Rectangle SourceRect { get; init; }

    /// <summary>
    ///     The tile center
    /// </summary>
    public Vector2 Center { get; init; }

    /// <summary>
    ///     The bounds of the tile where it is placed in the world
    /// </summary>
    public Rectangle WorldRect { get; init; }

    /// <summary>
    ///     The texture containing the tile's raster data
    /// </summary>
    public Texture2D Texture { get; init; }

    /// <summary>
    ///     SpriteEffects to be applied to the tile (flipped horizontally, vertically, or both)
    /// </summary>
    public SpriteEffects TileSpriteEffects { get; init; }

    /// <summary>
    ///     Rotation of the tile in radians
    /// </summary>
    public float Rotation { get; init; }

    /// <summary>
    ///     Draws the tile using the Texture and SourceRect
    /// </summary>
    /// <param name="gameTime">The current game time</param>
    /// <param name="spriteBatch">The SpriteBatch to draw with</param>
    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        var drawRect = new Rectangle(WorldRect.X + (int)Center.X, WorldRect.Y + (int)Center.Y, WorldRect.Width,
            WorldRect.Height);

        spriteBatch.Draw(Texture, drawRect, SourceRect, Color.White,
            Rotation, Center, TileSpriteEffects,
            1f);
    }


    public override void FillAreaOnMap(Color[] textureData, ref bool[] mapToFill, Vector2 mapDims) {
        var transformMatrix = Matrix.Identity;

        transformMatrix = transformMatrix * Matrix.CreateRotationZ(Rotation);

        if ((TileSpriteEffects & SpriteEffects.FlipHorizontally) != 0)
            transformMatrix = transformMatrix * Matrix.CreateReflection(new Plane(-1, 0, 0, Center.X));
        if ((TileSpriteEffects & SpriteEffects.FlipVertically) != 0)
            transformMatrix = transformMatrix * Matrix.CreateReflection(new Plane(0, -1, 0, Center.Y));

        transformMatrix = transformMatrix * Matrix.CreateTranslation(SourceRect.Left, SourceRect.Top, 0);

        for (var i = WorldRect.Left; i < WorldRect.Right; i++)
        for (var i2 = WorldRect.Top; i2 < WorldRect.Bottom; i2++) {
            var tileCoordinateX = i - WorldRect.Left;
            var tileCoordinateY = i2 - WorldRect.Top;


            var tileTextureCoordinate =
                Vector2.Transform(new Vector2(tileCoordinateX, tileCoordinateY), transformMatrix);

            var tileTextureCoordinateY = (int)tileTextureCoordinate.Y > Texture.Height - 1
                ? Texture.Height - 1
                : (int)tileTextureCoordinate.Y;
            var tileTextureCoordinateX = (int)tileTextureCoordinate.X > Texture.Width - 1
                ? Texture.Width - 1
                : (int)tileTextureCoordinate.X;

            if (tileTextureCoordinateY < 0) tileTextureCoordinateY = 0;
            if (tileTextureCoordinateX < 0) tileTextureCoordinateX = 0;


            mapToFill[i2 * (int)mapDims.X + i] =
                textureData[tileTextureCoordinateY * Texture.Width + tileTextureCoordinateX].A > 0;
        }
    }
}