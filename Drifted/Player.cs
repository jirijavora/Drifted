using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Drifted.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Drifted;

// Added here just for reordering prevention
public class IgnoreTypeMemberReorderingAttribute : Attribute { }

public struct PlayerReconstructionPos {
    public TimeSpan time { get; }
    public Vector2 position { get; }
    public float rotation { get; }

    public string getString() {
        return $"{time.TotalSeconds} {position}:{rotation};";
    }

    public PlayerReconstructionPos(TimeSpan time, Vector2 position, float rotation) {
        this.time = time;
        this.position = position;
        this.rotation = rotation;
    }

    public PlayerReconstructionPos(string info) {
        var spaceIndex = info.IndexOf(' ');
        var XIndex = info.IndexOf('X');
        var YIndex = info.IndexOf('Y');
        var closeCurlyIndex = info.IndexOf('}');

        var timeString = info.Substring(0, spaceIndex);
        var XString = info.Substring(XIndex + 2, YIndex - (XIndex + 2));
        var YString = info.Substring(YIndex + 2, closeCurlyIndex - (YIndex + 2));
        var rotationString = info.Substring(closeCurlyIndex + 2);

        time = TimeSpan.FromSeconds(double.Parse(timeString));
        position = new Vector2(float.Parse(XString),
            float.Parse(YString));
        rotation = float.Parse(rotationString);
    }
}

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

    private const float MinimumDriftingSteeringAngleSpeedRatio = 3f;
    private const float MaximumDriftingSteeringAngleSpeedRatio = 24f;

    private const float MaximumDriftingMultiplier = 0.95f;

    private Color[] textureData;

    private bool onStartLine = true;

    private bool isInReverse;

    private TimeSpan lapTime = TimeSpan.Zero;
    private TimeSpan? lastLapTime;
    private TimeSpan? bestLapTime;

    private SpriteFont font;

    private bool triedLoadingFromFile;

    private ScreenManager screenManager;

    private TiremarksParticleSystem tiremarkParticleSystem;

    private Checkpoint[] checkpoints;
    private Startline startline;

    private List<bool> hitCheckpoints;

    private bool lastLapValid = true;

    private SoundEffect engineSound;
    private SoundEffectInstance engineSoundInstance;

    private float maxPossibleSpeed;

    private SoundEffect tireSquealSound;
    private SoundEffectInstance tireSquealSoundInstance;
    private bool playTireSqueal = true;

    private const float MinimumDriftMultiplierTireMarks = 0.55f;

    private string playerRecording = "";

    private string bestPlayerGhostString = "";

    private List<PlayerReconstructionPos>? playerGhost;
    private List<PlayerReconstructionPos> playerGhostNew = new();
    private int playerGhostIndex;

    public void LoadContent(ContentManager content, ScreenManager screenManager, Checkpoint[] Checkpoints,
        Startline Startline) {
        font = content.Load<SpriteFont>("MagnetoBold");
        this.screenManager = screenManager;
        tiremarkParticleSystem = new TiremarksParticleSystem(screenManager.game, 8000);
        tiremarkParticleSystem.LoadContent();

        checkpoints = Checkpoints;
        startline = Startline;

        hitCheckpoints = new List<bool>();

        for (var i = 0; i < checkpoints.Length; i++) hitCheckpoints.Add(false);

        engineSound = content.Load<SoundEffect>("loop_5_0");
        engineSoundInstance = engineSound.CreateInstance();

        engineSoundInstance.IsLooped = true;
        engineSoundInstance.Pan = 0;
        engineSoundInstance.Pitch = -1;
        engineSoundInstance.Volume = 0.2f;
        engineSoundInstance.Play();

        maxPossibleSpeed = Acceleration / (1 - ResistanceMultiplier);


        tireSquealSound = content.Load<SoundEffect>("tire_squeal");
        tireSquealSoundInstance = tireSquealSound.CreateInstance();

        tireSquealSoundInstance.IsLooped = true;
        tireSquealSoundInstance.Pan = 0;
        tireSquealSoundInstance.Pitch = -1;
        tireSquealSoundInstance.Volume = 0.2f;
        tireSquealSoundInstance.Play();
        tireSquealSoundInstance.Pause();
    }

    private float getPlayerOffTrackPercentage(Vector2 position, float rotation, bool[] outsideTrackArr,
        Vector2 trackDims) {
        if (textureData == null) {
            textureData = new Color[Texture.Height * Texture.Width];
            Texture.GetData(textureData);
        }

        var transformMatrix = Matrix.CreateTranslation(-Texture.Width / 2f, -Texture.Height / 2f, 0) *
                              Matrix.CreateRotationZ(SpriteRotation + rotation) *
                              Matrix.CreateTranslation(position.X, position.Y, 0);

        var overlappingPoints = 0;
        var totalPoints = 0;

        for (var i = 0; i < Texture.Height; i++)
        for (var i2 = 0; i2 < Texture.Width; i2++)
            if (textureData[i * Texture.Width + i2].A > 0) {
                totalPoints++;
                var actualPointPos = Vector2.Transform(new Vector2(i2, i), transformMatrix);

                if (actualPointPos.X >= trackDims.X || actualPointPos.X < 0 || actualPointPos.Y >= trackDims.Y ||
                    actualPointPos.Y < 0)
                    return -1f;

                if (!outsideTrackArr[(int)actualPointPos.Y * (int)trackDims.X + (int)actualPointPos.X])
                    overlappingPoints++;
            }


        return overlappingPoints / (float)totalPoints;
    }

    public void StopSoundEffects() {
        engineSoundInstance.Pause();
        tireSquealSoundInstance.Pause();
        playTireSqueal = false;
    }

    public void ResumeSoundEffects() {
        engineSoundInstance.Resume();
        playTireSqueal = true;
    }

    private List<PlayerReconstructionPos> createPlayerGhost(string ghostString) {
        var ghostInfo = ghostString.Split(';');
        var ghost = new List<PlayerReconstructionPos>();

        foreach (var ghostRecord in ghostInfo)
            if (ghostRecord.Length > 0)
                ghost.Add(new PlayerReconstructionPos(ghostRecord));

        return ghost;
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
                        reader.ReadLine();

                        bestPlayerGhostString = reader.ReadLine();

                        playerGhost = createPlayerGhost(bestPlayerGhostString);
                    }
                }
                finally {
                    if (stream != null)
                        stream.Dispose();
                }
            }


            triedLoadingFromFile = true;
        }


        var newOnStartline = Position.X >= startline.X && Position.X <= startline.X + startline.Width &&
                             Position.Y >= startline.Y && Position.Y <= startline.Y + startline.Height;

        lapTime += gameTime.ElapsedGameTime;
        var newReconstructionPos = new PlayerReconstructionPos(lapTime, Position, _rotation);
        playerRecording += newReconstructionPos.getString();
        playerGhostNew.Add(newReconstructionPos);

        if (!onStartLine && newOnStartline) {
            lastLapTime = lapTime;
            lapTime = TimeSpan.Zero;

            lastLapValid = !hitCheckpoints.Exists(x => x == false);
            for (var i = 0; i < hitCheckpoints.Count; i++) hitCheckpoints[i] = false;

            if (lastLapValid) {
                var prevMedal = "";
                if (bestLapTime < goldTime) prevMedal = "Gold";
                else if (bestLapTime < silverTime) prevMedal = "Silver";
                else if (bestLapTime < bronzeTime) prevMedal = "Bronze";

                if (bestLapTime.HasValue) {
                    if (bestLapTime.Value.TotalSeconds > lastLapTime.Value.TotalSeconds) {
                        bestLapTime = lastLapTime;
                        bestPlayerGhostString = playerRecording;
                        playerGhost = playerGhostNew;
                    }
                }
                else {
                    bestLapTime = lastLapTime;
                    bestPlayerGhostString = playerRecording;
                    playerGhost = playerGhostNew;
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
                        writer.WriteLine(bestPlayerGhostString);
                    }
                }
                finally {
                    if (stream != null)
                        stream.Dispose();
                }

                if (prevMedal != medal)
                    screenManager.AddScreen(new MedalAchievedScreen(screenManager, medal));
            }

            playerRecording = "";
            playerGhostIndex = 0;
            playerGhostNew = new List<PlayerReconstructionPos>();
        }

        onStartLine = newOnStartline;

        for (var i = 0; i < checkpoints.Length; i++) {
            var checkpoint = checkpoints[i];

            if (Position.X >= checkpoint.X && Position.X <= checkpoint.X + checkpoint.Width &&
                Position.Y >= checkpoint.Y && Position.Y <= checkpoint.Y + checkpoint.Height)
                hitCheckpoints[i] = true;
        }

        var heading = new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation));

        var playerOffTrackPercentage = getPlayerOffTrackPercentage(Position, _rotation, outsideTrackArr, trackDims);

        var curSpeed = _direction.Length();
        engineSoundInstance.Pitch = Math.Clamp((curSpeed - maxPossibleSpeed / 2) / (maxPossibleSpeed / 2), -1, 1);

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


        var steeringAngleSpeedRatio = Math.Abs(_steeringAngle * curSpeed);
        var offsetSteeringAngleSpeedRatio = steeringAngleSpeedRatio - MinimumDriftingSteeringAngleSpeedRatio;
        var driftDirectionMultiplier = offsetSteeringAngleSpeedRatio > 0
            ? Math.Clamp((float)Math.Sqrt(offsetSteeringAngleSpeedRatio / MaximumDriftingSteeringAngleSpeedRatio), 0,
                MaximumDriftingMultiplier)
            : 0;
        var gripDirectionMultiplier = 1 - driftDirectionMultiplier;

        Debug.WriteLine(steeringAngleSpeedRatio);

        // Add forward movement
        if (forward) {
            var directionChange = heading * (Acceleration + ResistanceConstant) *
                                  (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (directionChange.LengthSquared() >= _direction.LengthSquared()) isInReverse = false;

            _direction += directionChange;
        }


        if (backwards) {
            var directionChange = heading * (Acceleration + ResistanceConstant) *
                                  (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (directionChange.LengthSquared() >= _direction.LengthSquared()) isInReverse = true;

            _direction -= directionChange;
        }

        curSpeed = _direction.Length();

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

            curSpeed = _direction.Length();

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
            var newPosition = Position + _direction * (float)gameTime.ElapsedGameTime.TotalSeconds;
            var newRotation = (float)Math.Atan2(newGripDirection.Y, newGripDirection.X);
            if (isInReverse)
                newRotation = newRotation - Math.Sign(newRotation - _rotation) * (float)Math.PI;
            else
                newRotation = newRotation;
            if (getPlayerOffTrackPercentage(newPosition, newRotation, outsideTrackArr, trackDims) != -1) {
                Position = newPosition;
                _rotation = newRotation;
            }
            else {
                _direction = Vector2.Zero;
            }


            if (driftDirectionMultiplier >= MinimumDriftMultiplierTireMarks) {
                var carRotationMatrix =
                    Matrix.CreateRotationZ(_rotation);
                tiremarkParticleSystem.AddTiremark(Position +
                                                   Vector2.Transform(new Vector2(-Center.X, -Center.Y / 3),
                                                       carRotationMatrix),
                    (driftDirectionMultiplier - MinimumDriftMultiplierTireMarks) /
                    (MaximumDriftingMultiplier - MinimumDriftMultiplierTireMarks));
                tiremarkParticleSystem.AddTiremark(Position +
                                                   Vector2.Transform(new Vector2(-Center.X, Center.Y / 3),
                                                       carRotationMatrix),
                    (driftDirectionMultiplier - MinimumDriftMultiplierTireMarks) /
                    (MaximumDriftingMultiplier - MinimumDriftMultiplierTireMarks));

                if (playTireSqueal) {
                    tireSquealSoundInstance.Resume();
                    tireSquealSoundInstance.Pitch =
                        Math.Clamp(
                            -1.5f + (driftDirectionMultiplier - MinimumDriftMultiplierTireMarks) /
                            (MaximumDriftingMultiplier - MinimumDriftMultiplierTireMarks), -1, 0);

                    tireSquealSoundInstance.Volume =
                        Math.Clamp(
                            (driftDirectionMultiplier - MinimumDriftMultiplierTireMarks) /
                            (MaximumDriftingMultiplier - MinimumDriftMultiplierTireMarks)
                            * 0.15f, 0, 0.15f);
                }
            }
            else {
                tireSquealSoundInstance.Pause();
            }
        }
        else {
            tireSquealSoundInstance.Pause();
        }

        tiremarkParticleSystem.Update(gameTime);
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        tiremarkParticleSystem.Draw(gameTime, spriteBatch);

        if (playerGhost != null) {
            var playerGhostRecord = playerGhost[playerGhostIndex];

            while (playerGhostRecord.time <= lapTime && playerGhostIndex + 1 < playerGhost.Count)
                playerGhostRecord = playerGhost[++playerGhostIndex];

            // Draw ghost
            spriteBatch.Draw(Texture, playerGhostRecord.position, null, new Color(100, 100, 100, 100),
                SpriteRotation + playerGhostRecord.rotation, Center, Vector2.One,
                SpriteEffects.None,
                1);
        }

        spriteBatch.Draw(Texture, Position, null, Color.White, SpriteRotation + _rotation, Center, Vector2.One,
            SpriteEffects.None,
            1);

        spriteBatch.DrawString(font,
            $"{lapTime.TotalSeconds:00.00}s",
            new Vector2(50, 3050),
            Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);

        var lastLapString = lastLapTime.HasValue
            ? $"Last lap time: {lastLapTime.Value.TotalSeconds:0.00}s {(lastLapValid ? "" : "[Invalid]")}"
            : "[No last lap time]";

        spriteBatch.DrawString(font,
            $"{lastLapString}   {(bestLapTime.HasValue ? $"(best: {bestLapTime.Value.TotalSeconds:0.00}s)" : "")}",
            new Vector2(50, 50),
            Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);
    }
}