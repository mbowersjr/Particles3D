using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Particles3D;
internal enum VertexCorner
{
    TopLeft = 0, 
    BottomLeft = 1, 
    BottomRight = 2, 
    TopRight = 3
}

public struct ParticleGeometry
{
    public VertexPositionColor[] Vertices;
    public short[] Indices;
    public Color Color;
    public Vector3 Position;
    public float Size;

    public ParticleGeometry(Vector3 position, float size, Color color, RasterizerState rasterizerState = null)
    {
        Position = position;
        Size = size;
        Color = color;

        // Vector3.Up.Y == 1 (positive)
        // 0: X - size/2  Y + size/2    Left Top 
        // 1: X - size/2  Y - size/2    Left Bottom 
        // 2: X + size/2  Y - size/2    Right Bottom 
        // 3: X + size/2  Y + size/2    Right Top 

        //RectangleF rect = new RectangleF(position.X - size * 0.5f, position.Y - size * 0.5f, size, size);
        RectangleF rect = new RectangleF(-0.5f, -0.5f, 1f, 1f);
        
        Vertices = new VertexPositionColor[4];
        Vertices[(int)VertexCorner.TopLeft] = new VertexPositionColor(new Vector3(rect.TopLeft, 0f), color);
        Vertices[(int)VertexCorner.BottomLeft] = new VertexPositionColor(new Vector3(rect.BottomLeft, 0f), color);
        Vertices[(int)VertexCorner.BottomRight] = new VertexPositionColor(new Vector3(rect.BottomRight, 0f), color);
        Vertices[(int)VertexCorner.TopRight] = new VertexPositionColor(new Vector3(rect.TopRight, 0f), color);

        Indices = new short[6];
        if (rasterizerState == null || rasterizerState == RasterizerState.CullClockwise)
        {
            /*
            
            0       5___4
            |\       \  |
            | \       \ |
            |__\       \|
            1   2       3
            
            */

            Indices[0] = (int)VertexCorner.TopLeft; 
            Indices[1] = (int)VertexCorner.BottomLeft; 
            Indices[2] = (int)VertexCorner.BottomRight;

            Indices[3] = (int)VertexCorner.BottomRight; 
            Indices[4] = (int)VertexCorner.TopRight; 
            Indices[5] = (int)VertexCorner.TopLeft;
        }
        else
        {
            /*

            0       4___5
            |\       \  |
            | \       \ |
            |__\       \|
            2   1       3

            */
            Indices[0] = (int)VertexCorner.TopLeft; 
            Indices[1] = (int)VertexCorner.BottomRight; 
            Indices[2] = (int)VertexCorner.BottomLeft;

            Indices[3] = (int)VertexCorner.BottomRight; 
            Indices[4] = (int)VertexCorner.TopLeft; 
            Indices[5] = (int)VertexCorner.TopRight;
        }
    }

    public void Translate(Matrix translation)
    {
        for (int i = 0; i < Vertices.Length; i++)
        {
            Vertices[i].Position = Vector3.Transform(Vertices[i].Position, translation);
        }
    }
}