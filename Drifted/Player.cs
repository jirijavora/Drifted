﻿using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Drifted;

// Added here just for reordering prevention
public class IgnoreTypeMemberReorderingAttribute : Attribute { }

[IgnoreTypeMemberReorderingAttribute]
public class Player {
    public Vector2 Position { get; private set; }

    public Texture2D Texture { get; init; }

    public Vector2 Center { get; init; }

    public float SpriteRotation { get; init; }

    private Vector2 _direction = Vector2.Zero;
    private float _steeringAngle;
    private float _rotation;

    private const float Acceleration = 666;
    private const float MaxSteeringAngle = MathHelper.Pi / 60f;
    private const float SteeringAcceleration = MaxSteeringAngle * 5f;
    private const float SteeringReturnAcceleration = 12f * SteeringAcceleration;
    private const double SteeringLimitationWithSpeed = 0.01;

    private const float ResistanceConstant = 300;
    private const float ResistanceMultiplier = 0.1f;
    private const float MinMoveSpeed = 100f;

    private const float OffTrackResistanceConstant = 3000;

    private const float OffTrackSpeedImmunity = 160;

    private Color[] textureData;

    private int? startLineX;
    private int? startLineY1;
    private int? startLineY2;

    private bool onStartLine = true;

    private TimeSpan lapStartTime = TimeSpan.Zero;
    private TimeSpan? lastLapTime;
    private TimeSpan? bestLapTime;

    private SpriteFont font;

    private bool triedLoadingFromFile;


    public void LoadContent(ContentManager content) {
        font = content.Load<SpriteFont>("MagnetoBold");
    }

    private float getPlayerOffTrackPercentage(bool[] outsideTrackArr, Vector2 trackDims) {
        if (textureData == null) {
            textureData = new Color[Texture.Height * Texture.Width];
            Texture.GetData(textureData);
        }

        var transformMatrix = Matrix.CreateTranslation(-Texture.Width / 2f, -Texture.Height / 2f, 0) *
                              Matrix.CreateRotationZ(_rotation) *
                              Matrix.CreateTranslation(Position.X, Position.Y, 0);

        var overlappingPoints = 0;
        var totalPoints = 0;

        for (var i = 0; i < Texture.Width; i++)
        for (var i2 = 0; i2 < Texture.Height; i2++)
            if (textureData[i * Texture.Width + i2].A > 0) {
                totalPoints++;
                var actualPointPos = Vector2.Transform(new Vector2(i, i2), transformMatrix);

                if (actualPointPos.X > trackDims.X || actualPointPos.X < 0 || actualPointPos.Y > trackDims.Y ||
                    actualPointPos.Y < 0)
                    return -1f;

                if (!outsideTrackArr[(int)actualPointPos.Y * (int)trackDims.X + (int)actualPointPos.X])
                    overlappingPoints++;
            }


        return overlappingPoints / (float)totalPoints;
    }


    public void Update(GameTime gameTime, bool[] outsideTrackArr, Vector2 trackDims) {
        if (triedLoadingFromFile == false) {
            if (File.Exists("savefile.txt")) {
                Stream stream = null;
                try {
                    stream = new FileStream("savefile.txt", FileMode.Open);
                    using (var reader = new StreamReader(stream)) {
                        stream = null;
                        lastLapTime = TimeSpan.FromSeconds(double.Parse(reader.ReadLine()));
                        bestLapTime = TimeSpan.FromSeconds(double.Parse(reader.ReadLine()));
                    }
                }
                finally {
                    if (stream != null)
                        stream.Dispose();
                }
            }


            triedLoadingFromFile = true;
        }


        if (startLineX == null) startLineX = (int)Position.X;
        if (startLineY1 == null) startLineY1 = (int)Position.Y - 180;
        if (startLineY2 == null) startLineY2 = (int)Position.Y + 180;

        var newOnStartline = Position.X >= startLineX - 10 && Position.X <= startLineX + 10 &&
                             Position.Y >= startLineY1 && Position.Y <= startLineY2;

        if (!onStartLine && newOnStartline) {
            lastLapTime = gameTime.TotalGameTime - lapStartTime;
            lapStartTime = gameTime.TotalGameTime;

            if (bestLapTime.HasValue) {
                if (bestLapTime.Value.TotalSeconds > lastLapTime.Value.TotalSeconds) bestLapTime = lastLapTime;
            }
            else {
                bestLapTime = lastLapTime;
            }

            Stream stream = null;
            try {
                stream = new FileStream("savefile.txt", FileMode.OpenOrCreate);
                using (var writer = new StreamWriter(stream)) {
                    stream = null;

                    writer.WriteLine(lastLapTime.Value.TotalSeconds);
                    writer.WriteLine(bestLapTime.Value.TotalSeconds);
                }
            }
            finally {
                if (stream != null)
                    stream.Dispose();
            }
        }

        onStartLine = newOnStartline;

        var heading = new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation));

        var playerOffTrackPercentage = getPlayerOffTrackPercentage(outsideTrackArr, trackDims);

        var curSpeed = _direction.Length();

        var keyboardState = Keyboard.GetState();
        var forward = keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W);
        var backwards = keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S);
        var left = keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A);
        var right = keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D);


        // Calculate new steering angle
        var steeringSpeedMultiplier =
            _direction.LengthSquared() < 1 ? 1 : 1f / (float)Math.Pow(curSpeed, SteeringLimitationWithSpeed);

        if (left)
            _steeringAngle -= (SteeringAcceleration + SteeringReturnAcceleration) *
                              (float)gameTime.ElapsedGameTime.TotalSeconds *
                              steeringSpeedMultiplier;
        if (right)
            _steeringAngle += (SteeringAcceleration + SteeringReturnAcceleration) *
                              (float)gameTime.ElapsedGameTime.TotalSeconds *
                              steeringSpeedMultiplier;


        // Steering centering
        if (_steeringAngle > 0) {
            _steeringAngle -= SteeringReturnAcceleration * (float)gameTime.ElapsedGameTime.TotalSeconds *
                              steeringSpeedMultiplier;
            if (_steeringAngle <= 0)
                _steeringAngle = 0;
        }
        else if (_steeringAngle < 0) {
            _steeringAngle += SteeringReturnAcceleration * (float)gameTime.ElapsedGameTime.TotalSeconds *
                              steeringSpeedMultiplier;
            if (_steeringAngle >= 0)
                _steeringAngle = 0;
        }


        if (_steeringAngle < -1 * MaxSteeringAngle) _steeringAngle = -1 * MaxSteeringAngle;
        if (_steeringAngle > MaxSteeringAngle) _steeringAngle = MaxSteeringAngle;

        // Add forward movement
        if (forward)
            _direction += heading * (Acceleration + ResistanceConstant) * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (curSpeed > MinMoveSpeed) {
            // Apply steering angle
            var newDirection = new Vector2(
                _direction.X * (float)Math.Cos(_steeringAngle) - _direction.Y * (float)Math.Sin(_steeringAngle),
                _direction.X * (float)Math.Sin(_steeringAngle) + _direction.Y * (float)Math.Cos(_steeringAngle)
            );
            _direction = newDirection;

            // Add resistance
            var normalizedDirection = new Vector2(_direction.X, _direction.Y);
            normalizedDirection.Normalize();

            if (curSpeed < ResistanceConstant * (float)gameTime.ElapsedGameTime.TotalSeconds)
                _direction = Vector2.Zero;
            else
                _direction -= normalizedDirection * ResistanceConstant * (float)gameTime.ElapsedGameTime.TotalSeconds;

            _direction -= (1 - ResistanceMultiplier) * _direction * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Add off track resistance

            if (curSpeed > OffTrackSpeedImmunity) {
                if (curSpeed < playerOffTrackPercentage * OffTrackResistanceConstant *
                    (float)gameTime.ElapsedGameTime.TotalSeconds)
                    _direction = Vector2.Zero;
                else
                    _direction -= normalizedDirection * (playerOffTrackPercentage * OffTrackResistanceConstant) *
                                  (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            // Calculate new rotation and position
            _rotation = (float)Math.Atan2(_direction.Y, _direction.X);

            Position += _direction * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        spriteBatch.Draw(Texture, Position, null, Color.White, SpriteRotation + _rotation, Center, Vector2.One,
            SpriteEffects.None,
            1);

        spriteBatch.DrawString(font,
            $"{(gameTime.TotalGameTime - lapStartTime).TotalSeconds:00.00}s",
            new Vector2(50, 3050),
            Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);

        if (lastLapTime.HasValue)
            spriteBatch.DrawString(font,
                $"Last lap time: {lastLapTime.Value.TotalSeconds:0.00}s    (best: {bestLapTime.Value.TotalSeconds:0.00}s)",
                new Vector2(50, 50),
                Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);
    }
}