using Drifted.ParticleSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Drifted;

public class TiremarksParticleSystem : ParticleSystem.ParticleSystem {
    private const float TireMarkMaxAlpha = 18;
    private Texture2D texture;


    public TiremarksParticleSystem(Game game, int tiremarkFrames) : base(game,
        tiremarkFrames * 2) {
        TextureFilename = "Blank";
    }

    protected override void InitializeConstants() {
        BlendState = BlendState.Additive;
    }

    protected override void InitializeParticle(ref Particle p, Vector2 where, float value) {
        var color = Color.Black;
        color.A = (byte)(value * TireMarkMaxAlpha);

        p.Initialize(where, Vector2.Zero, Vector2.Zero, color, -1f, 16);
    }

    public void AddTiremark(Vector2 where, float value) {
        AddParticle(where, value);
    }
}