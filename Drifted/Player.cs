using System;
using System.IO;
using Drifted.StateManagement;
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

    public TimeSpan bronzeTime;
    public TimeSpan silverTime;
    public TimeSpan goldTime;

    private Vector2 _direction = Vector2.Zero;
    private float _steeringAngle;
    private float _rotation;

    private const float Acceleration = 666;
    private const float MaxSteeringAngle = MathHelper.Pi / 60f;
    private const float SteeringAcceleration = MaxSteeringAngle * 3f;
    private const float SteeringReturnAcceleration = 5f * SteeringAcceleration;
    private const double SteeringLimitationWithSpeed = 0.04;

    private const float ResistanceConstant = 300;
    private const float ResistanceMultiplier = 0.1f;
    private const float MinMoveSpeed = 100f;

    private const float OffTrackResistanceConstant = 3000;

    private const float OffTrackSpeedImmunity = 160;

    private const float MinimumDriftingSteeringAngleSpeedRatio = 0.0000100f;
    private const float MaximumDriftingSteeringAngleSpeedRatio = 0.0000450f;

    private const float MaximumDriftingMultiplier = 0.8f;

    private Color[] textureData;

    private int? startLineX;
    private int? startLineY1;
    private int? startLineY2;

    private bool onStartLine = true;

    private bool isInReverse;

    private TimeSpan lapTime = TimeSpan.Zero;
    private TimeSpan? lastLapTime;
    private TimeSpan? bestLapTime;

    private SpriteFont font;

    private bool triedLoadingFromFile;

    private ScreenManager screenManager;

    private TiremarksParticleSystem tiremarkParticleSystem;


    public void LoadContent(ContentManager content, ScreenManager screenManager) {
        font = content.Load<SpriteFont>("MagnetoBold");
        this.screenManager = screenManager;
        tiremarkParticleSystem = new TiremarksParticleSystem(screenManager.game, 8000);
        tiremarkParticleSystem.LoadContent();
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


    public void Update(GameTime gameTime, bool[] outsideTrackArr, Vector2 trackDims, string levelName) {
        if (triedLoadingFromFile == false) {
            if (File.Exists($"{levelName}-savefile.txt")) {
                Stream stream = null;
                try {
                    stream = new FileStream($"{levelName}-savefile.txt", FileMode.Open);
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

        lapTime += gameTime.ElapsedGameTime;

        if (!onStartLine && newOnStartline) {
            lastLapTime = lapTime;
            lapTime = TimeSpan.Zero;

            var prevMedal = "";
            if (bestLapTime < goldTime) prevMedal = "Gold";
            else if (bestLapTime < silverTime) prevMedal = "Silver";
            else if (bestLapTime < bronzeTime) prevMedal = "Bronze";

            if (bestLapTime.HasValue) {
                if (bestLapTime.Value.TotalSeconds > lastLapTime.Value.TotalSeconds) bestLapTime = lastLapTime;
            }
            else {
                bestLapTime = lastLapTime;
            }

            var medal = "";
            if (bestLapTime < goldTime) medal = "Gold";
            else if (bestLapTime < silverTime) medal = "Silver";
            else if (bestLapTime < bronzeTime) medal = "Bronze";


            Stream stream = null;
            try {
                stream = new FileStream($"{levelName}-savefile.txt", FileMode.OpenOrCreate);
                using (var writer = new StreamWriter(stream)) {
                    stream = null;

                    writer.WriteLine(lastLapTime.Value.TotalSeconds);
                    writer.WriteLine(bestLapTime.Value.TotalSeconds);
                    writer.WriteLine(medal);
                }
            }
            finally {
                if (stream != null)
                    stream.Dispose();
            }

            if (prevMedal != medal)
                screenManager.AddScreen(new MedalAchievedScreen(screenManager, medal));
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

        if ((left && !isInReverse) || (right && isInReverse))
            _steeringAngle -= (SteeringAcceleration + SteeringReturnAcceleration) *
                              (float)gameTime.ElapsedGameTime.TotalSeconds *
                              steeringSpeedMultiplier;
        if ((right && !isInReverse) || (left && isInReverse))
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


        var steeringAngleSpeedRatio = Math.Abs(_steeringAngle / curSpeed);
        var offsetSteeringAngleSpeedRatio = steeringAngleSpeedRatio - MinimumDriftingSteeringAngleSpeedRatio;
        var driftDirectionMultiplier =
            (float)Math.Sqrt(Math.Clamp(offsetSteeringAngleSpeedRatio / MaximumDriftingSteeringAngleSpeedRatio, 0,
                MaximumDriftingMultiplier));
        var gripDirectionMultiplier = 1 - driftDirectionMultiplier;

        // Add forward movement
        if (forward) {
            var directionChange = heading * (Acceleration + ResistanceConstant) *
                                  (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (directionChange.LengthSquared() > _direction.LengthSquared()) isInReverse = false;

            _direction += directionChange;
        }


        if (backwards) {
            var directionChange = heading * (Acceleration + ResistanceConstant) *
                                  (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (directionChange.LengthSquared() > _direction.LengthSquared()) isInReverse = true;

            _direction -= directionChange;
        }

        if (curSpeed > MinMoveSpeed) {
            // Apply steering angle
            var newGripDirection = new Vector2(
                heading.X * (float)Math.Cos(_steeringAngle) - heading.Y * (float)Math.Sin(_steeringAngle),
                heading.X * (float)Math.Sin(_steeringAngle) + heading.Y * (float)Math.Cos(_steeringAngle)
            ) * _direction.Length() * (isInReverse ? -1 : 1);
            _direction = newGripDirection * gripDirectionMultiplier +
                         _direction * driftDirectionMultiplier;

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
            var newRotation = (float)Math.Atan2(newGripDirection.Y, newGripDirection.X);
            if (isInReverse)
                _rotation = newRotation - Math.Sign(newRotation - _rotation) * (float)Math.PI;
            else
                _rotation = newRotation;


            Position += _direction * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (driftDirectionMultiplier >= 0.85f) {
                var carRotationMatrix =
                    Matrix.CreateRotationZ(_rotation);
                tiremarkParticleSystem.AddTiremark(Position +
                                                   Vector2.Transform(new Vector2(-Center.X, -Center.Y / 3),
                                                       carRotationMatrix));
                tiremarkParticleSystem.AddTiremark(Position +
                                                   Vector2.Transform(new Vector2(-Center.X, Center.Y / 3),
                                                       carRotationMatrix));
            }
        }

        tiremarkParticleSystem.Update(gameTime);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        tiremarkParticleSystem.Draw(gameTime, spriteBatch);

        spriteBatch.Draw(Texture, Position, null, Color.White, SpriteRotation + _rotation, Center, Vector2.One,
            SpriteEffects.None,
            1);

        spriteBatch.DrawString(font,
            $"{lapTime.TotalSeconds:00.00}s",
            new Vector2(50, 3050),
            Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);

        if (lastLapTime.HasValue)
            spriteBatch.DrawString(font,
                $"Last lap time: {lastLapTime.Value.TotalSeconds:0.00}s    (best: {bestLapTime.Value.TotalSeconds:0.00}s)",
                new Vector2(50, 50),
                Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);
    }
}