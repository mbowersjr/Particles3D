using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using MonoGame.Extended.ViewportAdapters;
using Particles3D.Entities;
using Particles3D.Managers;

namespace Particles3D;

public class GameMain : Game
{
    private const bool UseIndexedVertices = true;
    private const bool UseDynamicBuffers = false;
    private const bool DrawGeometrySeparately = true;

    public Matrix View => _view;
    public Matrix Projection => _projection;
    public Matrix World => _world;
    public Vector3 CameraPosition => _cameraPosition;
    public Vector3 CameraTarget => _cameraTarget;
    public bool IsCameraOrbiting { get; set; } = false;

    
    private GraphicsDeviceManager _graphics;
    private ParticleManager _particleManager;


    private Vector3 _cameraPosition = new Vector3(0f, 0f, -100f);
    private Vector3 _cameraTarget = new Vector3(0f, 0f, 0f);
    private Matrix _view;
    private Matrix _projection;
    private Matrix _world;
    private bool _cameraViewDirty = false;
    private BasicEffect _effect;
    private ParticleGeometry[] _particleGeometries;

    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    
    public GameMain()
    {
        _graphics = new GraphicsDeviceManager(this);
        
        Content.RootDirectory = "Content";
        IsFixedTimeStep = false;
        IsMouseVisible = true;
        Window.AllowUserResizing = false;

        _particleManager = new ParticleManager(this, maxParticles: 1000);
        Services.AddService(_particleManager);
    }

    private void InitializeGraphics()
    {
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.PreferMultiSampling = true;
        
        _graphics.ApplyChanges();
    }

    private void InitMatrices()
    {
        float aspectRatio = (float)_graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight;
        float fieldOfView = MathHelper.ToRadians(45f);

        _world = Matrix.CreateWorld(
            position: Vector3.Zero, 
            forward: Vector3.Forward, 
            up: Vector3.Up
        );
        _view = Matrix.CreateLookAt(
            cameraPosition: _cameraPosition, 
            cameraTarget: _cameraTarget, 
            cameraUpVector: Vector3.Up
        );
        _projection = Matrix.CreatePerspectiveFieldOfView(
            fieldOfView: fieldOfView,
            aspectRatio: aspectRatio,
            nearPlaneDistance: 0.01f,
            farPlaneDistance: 1000f
        );
    }

    protected override void Initialize()
    {
        base.Initialize();

        InitializeGraphics();

        InitMatrices();
        
        _effect = new BasicEffect(GraphicsDevice);
        _effect.VertexColorEnabled = true;
        _effect.LightingEnabled = false;

        _particleManager.Initialize();

    }

    protected override void LoadContent()
    {
        _particleManager.LoadContent();
    }

    private void HandleInput(GameTime gameTime)
    {
        KeyboardExtended.Refresh();
        var keyboardState = KeyboardExtended.GetState();

        if (keyboardState.IsKeyPressed(Keys.Escape) || keyboardState.IsKeyPressed(Keys.Q))
        {
            Exit();
        }
        
        if (keyboardState.IsKeyPressed(Keys.R))
        {
            // Toggle camera rotation
            IsCameraOrbiting = !IsCameraOrbiting;
        }

        Vector3 cameraPositionMovement = Vector3.Zero;
        Vector3 cameraTargetMovement = Vector3.Zero;

        if (keyboardState.IsKeyDown(Keys.W))
        {
            cameraPositionMovement += Vector3.Forward;
            cameraTargetMovement += Vector3.Forward;
        }
        if (keyboardState.IsKeyDown(Keys.S))
        {
            cameraPositionMovement += Vector3.Backward;
            cameraTargetMovement += Vector3.Backward;
        }
        if (keyboardState.IsKeyDown(Keys.A))
        {
            cameraPositionMovement += Vector3.Left;
            cameraTargetMovement += Vector3.Left;
        }
        if (keyboardState.IsKeyDown(Keys.D))
        {
            cameraPositionMovement += Vector3.Right;
            cameraTargetMovement += Vector3.Right;
        }
        if (keyboardState.IsKeyDown(Keys.Space))
        {
            cameraPositionMovement += Vector3.Up;
            cameraTargetMovement += Vector3.Up;
        }
        if (keyboardState.IsControlDown())
        {
            cameraPositionMovement += Vector3.Down;
            cameraTargetMovement += Vector3.Down;
        }

        float movementSpeed = 2f; // * (float)gameTime.ElapsedGameTime.TotalSeconds;

        _cameraPosition += cameraPositionMovement * movementSpeed;
        _cameraTarget += cameraTargetMovement * movementSpeed;

    }

    protected override void Update(GameTime gameTime)
    {
        HandleInput(gameTime);

        if (IsCameraOrbiting)
        {
            //Matrix rotation = Matrix.CreateRotationY(MathHelper.ToRadians(1f));
            Quaternion rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 1f);
            Vector3.Transform(ref _cameraPosition, ref rotation, out _cameraPosition);
        }

        _particleManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _particleManager.Draw(gameTime, out _particleGeometries);

        GraphicsDevice.Clear(Color.Gray);

        InitMatrices();

        //var rasterizerState = new RasterizerState();
        //rasterizerState.CullMode = CullMode.None;
        //GraphicsDevice.RasterizerState = rasterizerState;

        if (DrawGeometrySeparately)
        {
            for (int i = 0; i < _particleGeometries.Length; i++)
            {
                DrawParticleGeometry(ref _particleGeometries[i], _world, _view, _projection);
            }
        }
        else
        {
            DrawAllGeometry(_world, _view, _projection);
        }
        
        base.Draw(gameTime);
    }

    private void InitBuffers()
    {
        int verticesCount = 0;
        int indicesCount = 0;

        for (int i = 0; i < _particleGeometries.Length; i++)
        {
            _particleGeometries[i].Translate(Matrix.CreateTranslation(_particleGeometries[i].Position));
            
            verticesCount += _particleGeometries[i].Vertices.Length;
            indicesCount += _particleGeometries[i].Indices.Length;
        }
        
        VertexPositionColor[] vertices = new VertexPositionColor[verticesCount];
        short[] indices = new short[indicesCount];

        for (int i = 0; i < _particleGeometries.Length; i++)
        {
            _particleGeometries[i].Vertices.CopyTo(vertices, i * 4);

            if (UseIndexedVertices)
            {
                _particleGeometries[i].Indices.CopyTo(indices, i * 6);

                // Move index values up to relative vertices in the new array
                for (int j = 0; j < _particleGeometries[i].Indices.Length; j++)
                {
                    indices[i * 6 + j] += (short)(i * 4);
                }
            }
        }

        if (UseDynamicBuffers)
        {
            _vertexBuffer = new DynamicVertexBuffer(
                graphicsDevice: GraphicsDevice,
                type: typeof(VertexPositionColor),
                vertexCount: vertices.Length,
                bufferUsage: BufferUsage.WriteOnly
            );
            
            if (UseIndexedVertices)
            {
                _indexBuffer = new DynamicIndexBuffer(
                    graphicsDevice: GraphicsDevice,
                    indexType: typeof(short),
                    indexCount: indices.Length,
                    usage: BufferUsage.WriteOnly
                );

            }
        }
        else
        {
            _vertexBuffer = new VertexBuffer(
                graphicsDevice: GraphicsDevice, 
                type: typeof(VertexPositionColor),
                vertexCount: vertices.Length,
                bufferUsage: BufferUsage.WriteOnly
            );
            
            if (UseIndexedVertices)
            {
                _indexBuffer = new IndexBuffer(
                    graphicsDevice: GraphicsDevice, 
                    indexType: typeof(short), 
                    indexCount: indices.Length, 
                    usage: BufferUsage.WriteOnly
                );
            }
        }
        
        if (vertices.Length > 0) 
            _vertexBuffer.SetData<VertexPositionColor>(vertices);
            
        GraphicsDevice.SetVertexBuffer(_vertexBuffer);

        if (UseIndexedVertices)
        {
            if (indices.Length > 0)
                _indexBuffer.SetData(indices);

            GraphicsDevice.Indices = _indexBuffer;
        }
    }

    private void DrawAllGeometry(Matrix world, Matrix view, Matrix projection)
    {
        _effect.World = world;
        _effect.View = view;
        _effect.Projection = projection;

        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.BlendState = BlendState.Opaque;

        InitBuffers();

        foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            if (UseIndexedVertices)
            {
                GraphicsDevice.DrawIndexedPrimitives(
                    primitiveType: PrimitiveType.TriangleList, 
                    baseVertex: 0, 
                    startIndex: 0, 
                    primitiveCount: _indexBuffer.IndexCount / 3
                );
            }
            else
            {
                GraphicsDevice.DrawPrimitives(
                    primitiveType: PrimitiveType.TriangleList, 
                    vertexStart: 0, 
                    primitiveCount: _vertexBuffer.VertexCount / 2
                );
            }
        }
    }

    private void DrawParticleGeometry(ref ParticleGeometry particleGeometry, Matrix world, Matrix view, Matrix projection)
    {
        _effect.World = 
            Matrix.CreateScale(particleGeometry.Size) * 
            Matrix.CreateTranslation(particleGeometry.Position);
        
        _effect.View = view;
        _effect.Projection = projection;

        //_effect.DiffuseColor = particleGeometry.Color.ToVector3();
        _effect.Alpha = particleGeometry.Color.A / 255f;

        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;

        GraphicsDevice.BlendState = 
            particleGeometry.Color.A < 255 ? 
                BlendState.AlphaBlend : 
                BlendState.Opaque;

        if (UseDynamicBuffers)
        {
            _vertexBuffer = new DynamicVertexBuffer(
                graphicsDevice: GraphicsDevice,
                type: typeof(VertexPositionColor),
                vertexCount: particleGeometry.Vertices.Length,
                bufferUsage: BufferUsage.WriteOnly
            );

            if (UseIndexedVertices)
            {
                _indexBuffer = new DynamicIndexBuffer(
                    graphicsDevice: GraphicsDevice,
                    indexType: typeof(ushort),
                    indexCount: particleGeometry.Indices.Length,
                    usage: BufferUsage.WriteOnly
                );
            }
        }
        else
        {
            _vertexBuffer = new VertexBuffer(
                graphicsDevice: GraphicsDevice, 
                type: typeof(VertexPositionColor),
                vertexCount: particleGeometry.Vertices.Length,
                bufferUsage: BufferUsage.WriteOnly
            );
            
            if (UseIndexedVertices)
            {
                _indexBuffer = new IndexBuffer(
                    graphicsDevice: GraphicsDevice, 
                    indexType: typeof(ushort), 
                    indexCount: particleGeometry.Indices.Length, 
                    usage: BufferUsage.WriteOnly
                );
            }
        }

        if (particleGeometry.Vertices.Length > 0)
            _vertexBuffer.SetData(particleGeometry.Vertices);

        GraphicsDevice.SetVertexBuffer(_vertexBuffer);

        if (UseIndexedVertices)
        {
            if (particleGeometry.Indices.Length > 0)
                _indexBuffer.SetData(particleGeometry.Indices);

            GraphicsDevice.Indices = _indexBuffer;
        }

        foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();

            if (UseIndexedVertices)
            {
                GraphicsDevice.DrawUserIndexedPrimitives(
                    primitiveType: PrimitiveType.TriangleList,
                    vertexData: particleGeometry.Vertices,
                    vertexOffset: 0,
                    indexOffset: 0,
                    primitiveCount: particleGeometry.Indices.Length / 3,
                    numVertices: particleGeometry.Vertices.Length,
                    indexData: particleGeometry.Indices,
                    vertexDeclaration: VertexPositionColor.VertexDeclaration
                );

                //GraphicsDevice.DrawIndexedPrimitives(
                //    primitiveType: PrimitiveType.TriangleList, 
                //    baseVertex: 0, 
                //    startIndex: 0,
                //    primitiveCount: particleGeometry.Indices.Length / 3
                //);
            }
            else
            {
                GraphicsDevice.DrawUserPrimitives(
                    primitiveType: PrimitiveType.TriangleList,
                    vertexData: particleGeometry.Vertices,
                    vertexOffset: 0,
                    primitiveCount: particleGeometry.Vertices.Length / 3, 
                    vertexDeclaration: VertexPositionColor.VertexDeclaration
                );

                //GraphicsDevice.DrawPrimitives(
                //    primitiveType: PrimitiveType.TriangleList,
                //    vertexStart: 0,
                //    primitiveCount: particleGeometry.Vertices.Length / 2
                //);
            }
        }
    }

}
