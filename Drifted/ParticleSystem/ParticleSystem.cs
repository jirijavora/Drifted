using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted.ParticleSystem;

/// <summary>
///     A class representing a generic particle system
/// </summary>
public abstract class ParticleSystem {
    protected Game game;

    /// <summary>
    ///     Constructs a new instance of a particle system
    /// </summary>
    /// <param name="game"></param>
    /// <param name="maxParticles"></param>
    protected ParticleSystem(Game game, int maxParticles) {
        this.game = game;

        // Create our particles
        particles = new Particle[maxParticles];
        for (var i = 0; i < particles.Length; i++) particles[i].Initialize(Vector2.Zero, 0, active: false);

        // Run the InitializeConstants hook
        InitializeConstants();
    }

    #region AddParticles methods

    /// <summary>
    ///     AddParticles's job is to add an effect somewhere on the screen. If there
    ///     aren't enough particles in the freeParticles queue, it will use as many as
    ///     it can. This means that if there not enough particles available, calling
    ///     AddParticles will have no effect.
    /// </summary>
    /// <param name="where">where the particle effect should be created</param>
    protected void AddParticle(Vector2 where, float value) {
        // the number of particles we want for this effect is a random number
        // somewhere between the two constants specified by the subclasses.
        InitializeParticle(ref particles[++lastUsedParticle % particles.Length], where, value);
    }

    #endregion

    #region public properties

    #endregion

    #region Constants

    /// <summary>
    ///     The draw order for particles using Alpha Blending
    /// </summary>
    /// <remarks>
    ///     Particles drawn using additive blending should be drawn on top of
    ///     particles that use regular alpha blending
    /// </remarks>
    public const int AlphaBlendDrawOrder = 100;

    /// <summary>
    ///     The draw order for particles using Additive Blending
    /// </summary>
    /// <remarks>
    ///     Particles drawn using additive blending should be drawn on top of
    ///     particles that use regular alpha blending
    /// </remarks>
    public const int AdditiveBlendDrawOrder = 200;

    #endregion

    #region static fields

    /// <summary>
    ///     A SpriteBatch to share amongst the various particle systems
    /// </summary>
    protected static SpriteBatch spriteBatch;

    /// <summary>
    ///     A ContentManager to share amongst the various particle systems
    /// </summary>
    protected static ContentManager contentManager;

    #endregion

    #region private fields

    /// <summary>
    ///     The collection of particles
    /// </summary>
    private readonly Particle[] particles;

    /// <summary>
    ///     A Queue containing indices of unused particles in the Particles array
    /// </summary>
    private int lastUsedParticle = -1;

    private struct TextureWithOrigin {
        public readonly Texture2D Texture { get; }
        public readonly Vector2 Origin { get; }

        public TextureWithOrigin(Texture2D texture, Vector2 origin) {
            Texture = texture;
            Origin = origin;
        }
    }

    /// <summary>
    ///     The texture this particle system uses
    /// </summary>
    private TextureWithOrigin texture;

    #endregion

    #region protected fields

    /// <summary>The BlendState to use with this particle system</summary>
    protected BlendState BlendState = BlendState.AlphaBlend;

    /// <summary>The filename of the texture to use for the particles</summary>
    protected string TextureFilename;

    /// <summary>The minimum number of particles to add when AddParticles() is called</summary>
    protected int MinNumParticles;

    /// <summary>The maximum number of particles to add when AddParticles() is called</summary>
    protected int MaxNumParticles;

    #endregion

    #region virtual hook methods

    /// <summary>
    ///     Used to do the initial configuration of the particle engine.  The
    ///     protected constants `textureFilename`, `minNumParticles`, and `maxNumParticles`
    ///     should be set in the override.
    /// </summary>
    protected abstract void InitializeConstants();

    /// <summary>
    ///     InitializeParticle randomizes some properties for a particle, then
    ///     calls initialize on it. It can be overridden by subclasses if they
    ///     want to modify the way particles are created.
    /// </summary>
    /// <param name="p">the particle to initialize</param>
    /// <param name="where">
    ///     the position on the screen that the particle should be
    /// </param>
    protected virtual void InitializeParticle(ref Particle p, Vector2 where, float value) {
        // Initialize the particle with default values
        p.Initialize(where);
    }

    /// <summary>
    ///     Updates the individual particles.  Can be overridden in derived classes
    /// </summary>
    /// <param name="particle">The particle to update</param>
    /// <param name="dt">The elapsed time</param>
    /// <param name="particleBounds">Bounding circle roughly the size of the particle</param>
    /// <param name="impassableObjects">All impassable objects for the particle</param>
    protected virtual void UpdateParticle(ref Particle particle, float dt) {
        if (particle.Velocity.LengthSquared() > 0) {
            // Update particle's linear motion values
            particle.Velocity += particle.Acceleration * dt;
            particle.Position += particle.Velocity * dt;

            // Update the particle's angular motion values
            particle.AngularVelocity += particle.AngularAcceleration * dt;
            particle.Rotation += particle.AngularVelocity * dt;
        }


        // Update the time the particle has been alive 
        particle.TimeSinceStart += dt;
    }

    #endregion

    #region override methods

    /// <summary>
    ///     Override the base class LoadContent to load the texture. once it's
    ///     loaded, calculate the origin.
    /// </summary>
    /// <throws>A InvalidOperationException if the texture filename is not provided</throws>
    public void LoadContent() {
        // create the shared static ContentManager and SpriteBatch,
        // if this hasn't already been done by another particle engine
        if (contentManager == null)
            contentManager = new ContentManager(game.Services, "Content");
        if (spriteBatch == null)
            spriteBatch = new SpriteBatch(game.GraphicsDevice);

        // make sure sub classes properly set textureFilename
        if (TextureFilename == null) {
            var message = "textureFilename wasn't set properly, so the " +
                          "particle system doesn't know what texture to load. Make " +
                          "sure your particle system's InitializeConstants function " +
                          "properly sets textureFilename.";
            throw new InvalidOperationException(message);
        }

        // load the textures....
        var texture2d = contentManager.Load<Texture2D>(TextureFilename);
        var origin = new Vector2(texture2d.Width / 2, texture2d.Height / 2);

        texture = new TextureWithOrigin(texture2d, origin);
    }

    /// <summary>
    ///     Overriden from DrawableGameComponent, Update will update all of the active
    ///     particles.
    /// </summary>
    public void Update(GameTime gameTime) {
        // calculate dt, the change in the since the last frame. the particle
        // updates will use this value.
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // go through all of the particles...
        for (var i = 0; i < particles.Length; i++)
            if (particles[i].Active)
                // ... and if they're active, update them.
                UpdateParticle(ref particles[i], dt);
    }

    /// <summary>
    ///     Overriden from DrawableGameComponent, Draw will use the static
    ///     SpriteBatch to render all of the active particles.
    /// </summary>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch) {
        // tell sprite batch to begin, using the spriteBlendMode specified in
        // initializeConstants


        foreach (var p in particles) {
            // skip inactive particles
            if (!p.Active)
                continue;

            spriteBatch.Draw(texture.Texture, p.Position, null, p.Color,
                p.Rotation, texture.Origin, p.Scale, SpriteEffects.None, 2);
        }
    }

    #endregion
}