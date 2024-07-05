using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Particles3D.Entities;

/// <summary>
/// Custom vertex structure for drawing particles.
/// </summary>
struct ParticleVertex
{
    // Stores which corner of the particle quad this vertex represents.
    public Short2 Corner;

    // Stores the starting position of the particle.
    public Vector3 Position;

    // Stores the starting velocity of the particle.
    public Vector3 Velocity;

    // Four random values, used to make each particle look slightly different.
    public Color Random;

    // The time (in seconds) at which this particle was created.
    public float Time;


    // Describe the layout of this vertex structure.
    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
    (
        elements: [
            new VertexElement(offset: 0,  elementFormat: VertexElementFormat.Short2,  elementUsage: VertexElementUsage.Position,          usageIndex: 0),
            new VertexElement(offset: 4,  elementFormat: VertexElementFormat.Vector3, elementUsage: VertexElementUsage.Position,          usageIndex: 1),
            new VertexElement(offset: 16, elementFormat: VertexElementFormat.Vector3, elementUsage: VertexElementUsage.Normal,            usageIndex: 0),
            new VertexElement(offset: 28, elementFormat: VertexElementFormat.Color,   elementUsage: VertexElementUsage.Color,             usageIndex: 0),
            new VertexElement(offset: 32, elementFormat: VertexElementFormat.Single,  elementUsage: VertexElementUsage.TextureCoordinate, usageIndex: 0)
        ]
    );


    // Describe the size of this vertex structure.
    public const int SizeInBytes = 36;
}