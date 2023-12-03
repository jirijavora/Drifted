using System;
using System.IO;
using Drifted.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

[IgnoreTypeMemberReorderingAttribute]
public class LevelScreen {
    private LevelScreenTilemap tilemap;

    private readonly Vector2 center;
    private readonly string name;

    private readonly TimeSpan? bestLapTime;
    private readonly string? medal;

    public LevelScreen(Vector2 center, string name) {
        this.center = center;
        this.name = name;


        if (File.Exists($"{name}-savefile.txt")) {
            Stream stream = null;
            try {
                stream = new FileStream($"{name}-savefile.txt", FileMode.Open);
                using (var reader = new StreamReader(stream)) {
                    stream = null;
                    reader.ReadLine();
                    bestLapTime = TimeSpan.FromSeconds(double.Parse(reader.ReadLine()));
                    medal = reader.ReadLine();
                }
            }
            finally {
                if (stream != null)
                    stream.Dispose();
            }
        }
    }


    public void LoadContent(ContentManager content, ScreenManager screenManager) {
        tilemap = content.Load<LevelScreenTilemap>("LevelScreen");
        tilemap.LoadContent(content, screenManager);
    }

    public void Update(GameTime gameTime) {
        tilemap.Update(gameTime);
    }

    public string getName() {
        return name;
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch, float sizeMultiplier) {
        tilemap.Draw(gameTime, spriteBatch, center, name, bestLapTime, medal, sizeMultiplier);
    }
}