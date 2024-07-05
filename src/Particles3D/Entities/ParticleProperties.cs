using System;
using Microsoft.Xna.Framework;

namespace Particles3D.Entities;

public struct ParticleProperties
{
    public Vector3? Position = Vector3.Zero;
    public Vector3? Velocity = Vector3.Zero;
    public Vector3? Acceleration = Vector3.Zero;
    public float? Rotation = 0.0f, RotationalVelocity = 0.0f;
    public float? Mass = 1.0f;
    public double? Age = 0.0, Lifetime = 1.0;
    public bool? IsActive = false;
    public float? SizeStart = 10.0f, SizeEnd = 0.0f;
    public Color? ColorStart = Color.Blue, ColorEnd = Color.White;

    public ParticleProperties()
    {
    }

    public static ref ParticleProperties Default => ref _defaultParticle;

    private static ParticleProperties _defaultParticle = new ParticleProperties()
    {
        Position = Vector3.Zero,
        Velocity = Vector3.Zero,
        Acceleration = Vector3.Zero,
        Rotation = 0.0f,
        RotationalVelocity = 0.0f,
        Mass = 1.0f,
        Age = 0.0,
        Lifetime = 1.0,
        IsActive = false,
        SizeStart = 10.0f,
        SizeEnd = 0.0f,
        ColorStart = Color.Blue,
        ColorEnd = Color.White
    };
}