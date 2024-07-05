using System;
using System.Diagnostics;
using Particles3D.Entities;

namespace Particles3D;

public class ParticlePool
{
    private int _count = 0;
    private readonly int _capacity;
    private readonly Particle[] _particles;

    public Span<Particle> GetSpan() => _particles.AsSpan(0, _count);

    public int Count => _count;
    public int Capacity => _capacity;

    public ParticlePool(int maxParticles)
    {
        _capacity = maxParticles;
        _count = 0;

        _particles = new Particle[_capacity];
        for (int i = 0; i < _particles.Length; i++)
        {
            _particles[i] = new Particle();
        }
    }

    public ref Particle ParticleAt(int index)
    {
        if (index < 0 || index >= _count)
            throw new IndexOutOfRangeException();

        return ref _particles[index];
    }

    public ref Particle Add(ref ParticleProperties properties)
    {
        if (properties.Equals(default))
            throw new ArgumentNullException(nameof(properties));

        if (_count == _capacity)
            throw new InvalidOperationException("Collection is full.");

        ref Particle head = ref _particles[_count];
        
        head.Init(ref properties, _count);
        
        _count++;
		
        return ref head;
    }

    public void Remove(ref Particle particle)
    {
        ArgumentNullException.ThrowIfNull(nameof(particle));
		
        Remove(particle.Id);
    }

    public void Remove(int index)
    {
        if (index < 0 || index >= _count)
            throw new IndexOutOfRangeException();

        ref Particle toRemove = ref _particles[index];
        ref Particle head = ref _particles[_count - 1];
		
        toRemove = head;
        toRemove.Id = index;
		
        head.Init(ref ParticleProperties.Default);
		
        _count--;
    }

    public void Clear()
    {
        _count = 0;
        for (int i = 0; i < _particles.Length; i++)
        {
            _particles[i].IsActive = false;
        }
    }
}