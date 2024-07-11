using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics.Geometry;
using Particles3D.Entities;

namespace Particles3D.Managers;

public class ParticleManager
{
    private GameMain _game;
    private readonly Vector3 _gravityForce = Vector3.Down * 10f;

    private static Texture2D PixelTexture;
    private static Vector2 PixelOrigin;
    
    private readonly FastRandom _rand = new FastRandom();

    private ParticleGeometry[] _particleGeometries;
    private int _particleGeometriesCount;

    private ParticlePool _particles;

    public ParticleManager(GameMain game, int maxParticles)
    {
        _game = game;

        _particles = new ParticlePool(maxParticles);
    }

    public void Initialize()
    {
    }

    public void Draw(GameTime gameTime, ref ParticleGeometry[] particleGeometries)
    {
        if (_game.GraphicsDevice == null)
            return;

        Span<Particle> particles = _particles.GetSpan();

        if (particleGeometries == null || particleGeometries.Length < _particles.Count)
        {
            particleGeometries = new ParticleGeometry[(int)(_particles.Count * 1.5f)];
        }

        for (int i = 0; i < particleGeometries.Length; i++)
        {
            particleGeometries[i].IsActive = false;
        }

        for (int i = 0; i < _particles.Count; i++)
        {
            float normalizedAge = (float)(particles[i].Age / particles[i].Lifetime);
            
            Color color = Color.Lerp(particles[i].ColorStart, particles[i].ColorEnd, normalizedAge);
            float size = MathHelper.Lerp(particles[i].SizeStart, particles[i].SizeEnd, normalizedAge * normalizedAge * (3-(2 * normalizedAge)));
            
            particleGeometries[i].Init(particles[i].Position, size, color);
        }
    }

    private double _spawnInterval = 1.0 / 100;
    private double _spawnTimer = 0.0;

    public void Update(GameTime gameTime)
    {
        Span<Particle> particles = _particles.GetSpan();
        
        for (int i = 0; i < _particles.Count; i++)
        {
            if (particles[i].IsActive == false)
            {
                _particles.Remove(ref particles[i]);
            }

            UpdateParticle(ref particles[i], ref gameTime);
        }

        _spawnTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_spawnTimer >= _spawnInterval)
        {
            while (_spawnTimer >= _spawnInterval)
            {
                SpawnParticle();
                _spawnTimer -= _spawnInterval;
            }
        }
    }

    public void LoadContent()
    {
        PixelTexture = new Texture2D(_game.GraphicsDevice, 1, 1);
        PixelTexture.SetData([Color.White]);
        PixelOrigin = new Vector2(0.5f, 0.5f);
    }

    private void ApplyForce(ref Particle particle, Vector3 force)
    {
        force /= particle.Mass;
        particle.Acceleration += force;
    }

    private void SpawnParticle()
    {
        //var viewportCenter = _game.GraphicsDevice.Viewport.Bounds.Center;
        //Vector3 position = new Vector3(viewportCenter.X, viewportCenter.Y, 0.0f);

        float force = _rand.NextSingle(min: 25f, max: 40f);

        Vector3 position = new Vector3(0f, 0f, 0f);
        
        Vector3 direction = _rand.OnUnitSphere();
        direction.Y = Math.Abs(direction.Y); // within upper hemisphere

        Vector3 velocity = direction * force;
        
        var properties = new ParticleProperties()
        {
            Position = position,
            Velocity = velocity,
            SizeStart = 3.0f,
            SizeEnd = 0.0f,
            Lifetime = 4.0f,
            RotationalVelocity = 0f,
            ColorStart = Color.Blue,
            ColorEnd = Color.Transparent,
            IsActive = true
        };

        Add(ref properties);
    }

    private void UpdateParticle(ref Particle particle, ref GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        particle.Age += dt;

        if (particle.Age >= particle.Lifetime)
        {
            particle.IsActive = false;
            return;
        }

        ApplyForce(ref particle, _gravityForce);

        particle.Velocity += particle.Acceleration * dt;
        particle.Position += particle.Velocity * dt;
        particle.Rotation += particle.RotationalVelocity * dt;

        particle.Acceleration = Vector3.Zero;
    }

    public ref Particle Add(ref ParticleProperties properties)
    {
        return ref _particles.Add(ref properties);
    }

    public void Remove(ref Particle particle)
    {
        _particles.Remove(ref particle);
    }

    public void Remove(int index)
    {
        _particles.Remove(index);
    }
}
