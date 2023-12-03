using System;
using System.Collections.Generic;
using Drifted.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

[IgnoreTypeMemberReorderingAttribute]
public class MenuTilemap : IntroTilemap {
    public LevelRecord[] LevelRecords;

    private readonly List<LevelScreen> Levels = new();


    private float selectedAdditionalSize;
    private const float MaxAdditionalSizeHalf = 0.05f;
    private double lastSelectedTime;

    private int selectedIndex;

    public override void LoadContent(ContentManager content, ScreenManager screenManager) {
        foreach (var levelRecord in LevelRecords) Levels.Add(levelRecord.Create(content, screenManager));

        base.LoadContent(content, screenManager);
    }

    private void selectedChangeSizeUpdate(GameTime gameTime) {
        selectedAdditionalSize = MaxAdditionalSizeHalf * 2;
        lastSelectedTime = gameTime.TotalGameTime.TotalSeconds;
    }

    public void selectedUp(GameTime gameTime) {
        selectedIndex = (selectedIndex + 1) % Levels.Count;
        selectedChangeSizeUpdate(gameTime);
    }

    public void selectedDown(GameTime gameTime) {
        selectedIndex = selectedIndex - 1;
        if (selectedIndex < 0) selectedIndex = Levels.Count - 1;
        selectedChangeSizeUpdate(gameTime);
    }

    public string getLevelName() {
        return Levels[selectedIndex].getName();
    }

    public override void Update(GameTime gameTime) {
        selectedAdditionalSize = MaxAdditionalSizeHalf *
                                 (float)(Math.Cos((gameTime.TotalGameTime.TotalSeconds - lastSelectedTime) * 4) + 1);

        foreach (var level in Levels) level.Update(gameTime);
        base.Update(gameTime);
    }


    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        // Draw the map
        base.Draw(gameTime, spriteBatch);

        // Draw the levels
        for (var i = 0; i < Levels.Count; i++)
            Levels[i].Draw(gameTime, spriteBatch, i == selectedIndex ? selectedAdditionalSize : 0);
    }
}