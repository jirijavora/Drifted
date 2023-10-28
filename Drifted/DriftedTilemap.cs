﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

public class DriftedTilemap : OOTilemap {
    private bool[] _outsideTrackArr;
    public Player Player { get; init; }


    public override void Update(GameTime gameTime) {
        var dims = new Vector2(Size.X * Tilesize.X, Size.Y * Tilesize.Y);

        if (_outsideTrackArr == null) {
            var textureData = new Color[Texture.Height * Texture.Width];
            Texture.GetData(textureData);
            var trackLayer = Layers[1];
            _outsideTrackArr = new bool[(int)dims.X * (int)dims.Y];
            foreach (var tile in trackLayer.Tiles) tile.FillAreaOnMap(textureData, ref _outsideTrackArr, dims);
        }


        Player.Update(gameTime, _outsideTrackArr, dims);

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        // Draw the map
        base.Draw(gameTime, spriteBatch);

        // Draw the hero
        Player.Draw(gameTime, spriteBatch);
    }
}