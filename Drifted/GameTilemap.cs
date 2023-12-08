using Drifted.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

[IgnoreTypeMemberReorderingAttribute]
public class GameTilemap : OOTilemap {
    private bool[] _outsideTrackArr;
    public Player Player { get; init; }

    public Checkpoint[] Checkpoints;
    public Startline Startline;


    public void LoadContent(ContentManager content, ScreenManager screenManager) {
        Player.LoadContent(content, screenManager, Checkpoints, Startline);
    }

    public void Update(GameTime gameTime, string levelName) {
        var dims = new Vector2(Size.X * Tilesize.X, Size.Y * Tilesize.Y);

        if (_outsideTrackArr == null) {
            var textureData = new Color[Texture.Height * Texture.Width];
            Texture.GetData(textureData);
            var trackLayer = Layers[1];
            _outsideTrackArr = new bool[(int)dims.X * (int)dims.Y];
            foreach (var tile in trackLayer.Tiles) tile.FillAreaOnMap(textureData, ref _outsideTrackArr, dims);
        }


        Player.Update(gameTime, _outsideTrackArr, dims, levelName);

        base.Update(gameTime);
    }

    public void Pause() {
        Player.StopSoundEffects();
    }

    public void Resume() {
        Player.ResumeSoundEffects();
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        // Draw the map
        base.Draw(gameTime, spriteBatch);

        // Draw the hero
        Player.Draw(gameTime, spriteBatch);
    }
}