using System;
using System.Collections.Generic;
using Drifted.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

public class MedalAchievedScreen : GameScreen {
    private const float MaxAdditionalSizeHalf = 0.05f;
    private readonly List<Button> buttons = new();
    private readonly Color overlayColorBackground = Color.FromNonPremultiplied(0, 0, 0, 160);
    private readonly ScreenManager screenManager;

    private readonly List<Title> titles = new();


    private Texture2D blankTexture;
    private double lastSelectedTime;

    private float selectedAdditionalSize;

    private int selectedButton;

    public MedalAchievedScreen(ScreenManager screenManager, string medalType) {
        this.screenManager = screenManager;

        titles.Add(new Title(screenManager, new Vector2(640 * 4, 220 * 4), "You got a new medal"));
        titles.Add(new Title(screenManager, new Vector2(640 * 4, 320 * 4), medalType));

        buttons.Add(new Button(new Vector2(640 * 4, 560 * 4), "Ok"));
    }

    public override void Activate() {
        base.Activate();
        blankTexture = Content.Load<Texture2D>("Blank");
        foreach (var button in buttons) button.LoadContent(Content, screenManager);
        foreach (var title in titles) title.LoadContent(Content);
    }

    public override void Unload() {
        Content.Unload();
    }

    public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen) {
        selectedAdditionalSize = MaxAdditionalSizeHalf *
                                 (float)(Math.Cos((gameTime.TotalGameTime.TotalSeconds - lastSelectedTime) * 4) + 1);
    }

    private void selectedChangeSizeUpdate(GameTime gameTime) {
        selectedAdditionalSize = MaxAdditionalSizeHalf * 2;
        lastSelectedTime = gameTime.TotalGameTime.TotalSeconds;
    }

    public override void HandleInput(GameTime gameTime, InputState input) {
        if (input.Escape) ExitScreen();
        if (input.Action) ExitScreen();
    }

    public override void Draw(GameTime gameTime) {
        var spriteBatch = ScreenManager.SpriteBatch;

        spriteBatch.Begin();

        spriteBatch.Draw(blankTexture, new Rectangle(0, 0, 1280, 800), overlayColorBackground);

        for (var i = 0; i < buttons.Count; i++)
            buttons[i].Draw(gameTime, spriteBatch, selectedButton == i ? selectedAdditionalSize : 0);

        foreach (var title in titles) title.Draw(gameTime, spriteBatch);

        spriteBatch.End();
    }
}