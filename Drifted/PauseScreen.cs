﻿using System;
using System.Collections.Generic;
using Drifted.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

public class PauseScreen : GameScreen {
    private const float MaxAdditionalSizeHalf = 0.05f;
    private readonly List<Button> buttons = new();
    private readonly Color overlayColorBackground = Color.FromNonPremultiplied(0, 0, 0, 200);
    private readonly ScreenManager screenManager;

    private readonly List<Text> texts = new();

    private readonly GameScreen undelyingScreen;
    private Texture2D blankTexture;
    private double lastSelectedTime;

    private float selectedAdditionalSize;

    private int selectedButton;

    public PauseScreen(ScreenManager screenManager, GameScreen underlyingScreen) {
        this.screenManager = screenManager;
        undelyingScreen = underlyingScreen;

        buttons.Add(new Button(new Vector2(640 * 4, 320 * 4), "Resume"));
        buttons.Add(new Button(new Vector2(640 * 4, 480 * 4), "Exit to menu"));
        texts.Add(new Text(screenManager, new Vector2(640 * 4, 660 * 4),
            "Use `W` and `S` or `Up_Arrow` and `Down_Arrow` to select option"));
        texts.Add(new Text(screenManager, new Vector2(640 * 4, 720 * 4), "Press `Enter` to select option"));
    }

    public override void Activate() {
        base.Activate();
        blankTexture = Content.Load<Texture2D>("Blank");
        foreach (var button in buttons) button.LoadContent(Content, screenManager);
        foreach (var text in texts) text.LoadContent(Content);
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

        if (input.DownPress) selectedButton += 1;
        if (input.UpPress) selectedButton -= 1;

        while (selectedButton < 0) selectedButton += buttons.Count;
        while (selectedButton >= buttons.Count) selectedButton -= buttons.Count;

        if (input.DownPress || input.UpPress) selectedChangeSizeUpdate(gameTime);

        if (input.Action) {
            if (buttons[selectedButton].getText() == "Resume") {
                ExitScreen();
            }
            else {
                screenManager.ReplaceScreen(undelyingScreen, new MenuScreen(screenManager));
                ExitScreen();
            }
        }
    }

    public override void Draw(GameTime gameTime) {
        var spriteBatch = ScreenManager.SpriteBatch;

        spriteBatch.Begin();

        spriteBatch.Draw(blankTexture, new Rectangle(0, 0, 1280, 800), overlayColorBackground);

        for (var i = 0; i < buttons.Count; i++)
            buttons[i].Draw(gameTime, spriteBatch, selectedButton == i ? selectedAdditionalSize : 0);

        foreach (var text in texts) text.Draw(gameTime, spriteBatch);

        spriteBatch.End();
    }
}