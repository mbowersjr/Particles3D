using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Particles3D.Managers;

namespace Particles3D.Entities;

public struct Particle : IEquatable<Particle>
{
    public int Id = -1;
    public Vector3 Position = Vector3.Zero;
    public Vector3 Velocity = Vector3.Zero;
    public Vector3 Acceleration = Vector3.Zero;
    public float Rotation = 0.0f, RotationalVelocity = 0.0f;
    public float Mass = 1.0f;
    public double Age = 0.0, Lifetime = 10.0;
    public float SizeStart = 10.0f, SizeEnd = 0.0f;
    public Color ColorStart = Color.Blue, ColorEnd = Color.White;
    public bool IsActive = false;

    public Particle()
        : this(ref ParticleProperties.Default, -1)
    {
    }

    public Particle(ref ParticleProperties properties, int id)
    {
        Init(ref properties, id);
    }

    public void Init(ref ParticleProperties properties, int id = -1)
    {
        Id = id;
        Position = properties.Position.GetValueOrDefault(Vector3.Zero);
        Velocity = properties.Velocity.GetValueOrDefault(Vector3.Zero);
        Acceleration = properties.Acceleration.GetValueOrDefault(Vector3.Zero);
        Rotation = properties.Rotation.GetValueOrDefault(0.0f);
        RotationalVelocity = properties.RotationalVelocity.GetValueOrDefault(0.0f);
        Mass = properties.Mass.GetValueOrDefault(1.0f);
        Age = properties.Age.GetValueOrDefault(0.0);
        Lifetime = properties.Lifetime.GetValueOrDefault(10.0f);
        SizeStart = properties.SizeStart.GetValueOrDefault(1.0f);
        SizeEnd = properties.SizeEnd.GetValueOrDefault(0.0f);
        ColorStart = properties.ColorStart.GetValueOrDefault(Color.Blue);
        ColorEnd = properties.ColorEnd.GetValueOrDefault(Color.White);
        IsActive = properties.IsActive.GetValueOrDefault(false);
    }

    public bool Equals(Particle other)
    {
        return Id == other.Id && 
               Position.Equals(other.Position) && 
               Velocity.Equals(other.Velocity) &&
               Acceleration.Equals(other.Acceleration) && 
               Rotation.Equals(other.Rotation) &&
               RotationalVelocity.Equals(other.RotationalVelocity) && 
               Mass.Equals(other.Mass) &&
               Age.Equals(other.Age) && 
               Lifetime.Equals(other.Lifetime) && 
               IsActive == other.IsActive &&
               SizeStart.Equals(other.SizeStart) && 
               SizeEnd.Equals(other.SizeEnd) &&
               ColorStart.Equals(other.ColorStart) && 
               ColorEnd.Equals(other.ColorEnd);
    }

    public override bool Equals(object obj)
    {
        return obj is Particle other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        hashCode.Add(Position);
        hashCode.Add(Velocity);
        hashCode.Add(Acceleration);
        hashCode.Add(Rotation);
        hashCode.Add(RotationalVelocity);
        hashCode.Add(Mass);
        hashCode.Add(Age);
        hashCode.Add(Lifetime);
        hashCode.Add(IsActive);
        hashCode.Add(SizeStart);
        hashCode.Add(SizeEnd);
        hashCode.Add(ColorStart);
        hashCode.Add(ColorEnd);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(Particle left, Particle right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Particle left, Particle right)
    {
        return !left.Equals(right);
    }
}
