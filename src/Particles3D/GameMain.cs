using System;
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
    private const bool DrawAsSprites = false;
    private const bool DrawAsQuads = true;

    private SpriteBatch _spriteBatch;

    public Matrix View => _view;
    public Matrix Projection => _projection;
    public Matrix World => _world;
    public bool IsCameraOrbiting { get; set; } = false;

    
    private GraphicsDeviceManager _graphics;
    private ParticleManager _particleManager;
    
    private OrthographicCamera _camera;
    private ViewportAdapter _viewportAdapter;
    private Vector3 _cameraPosition = new Vector3(0f, 25f, 150f);
    private Vector3 _cameraTarget = Vector3.Zero;
    private Vector3 _cameraDirection = Vector3.Forward;
    private float _cameraTargetDistance;
    private float _aspectRatio;
    private float _fieldOfView = MathHelper.ToRadians(45f);
    private Matrix _world;
    private Matrix _view;
    private Matrix _projection;
    private bool _cameraViewDirty = false;
    private BasicEffect _effect;
    private ParticleGeometry[] _particleGeometries;
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private Texture2D _pixelTexture;
    private readonly Vector2 _pixelOrigin = new Vector2(0.5f, 0.5f);
    private float _nearPlane = 0.01f;
    private float _farPlane = 100000f;
    private Quaternion _orbitRotation = Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(1f));

    public GameMain()
    {
        _graphics = new GraphicsDeviceManager(this);
        
        Content.RootDirectory = "Content";
        IsFixedTimeStep = false;
        IsMouseVisible = true;
        Window.AllowUserResizing = false;

        _particleManager = new ParticleManager(this, maxParticles: 10000);
        Services.AddService(_particleManager);
    }

    private void InitializeGraphics()
    {
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.SynchronizeWithVerticalRetrace = false;
        _graphics.PreferMultiSampling = true;
        
        _graphics.ApplyChanges();

        _aspectRatio = (float)_graphics.GraphicsDevice.PresentationParameters.BackBufferWidth / (float)_graphics.GraphicsDevice.PresentationParameters.BackBufferHeight;
    }


    private void UpdateMatrices()
    {
        _cameraDirection = Vector3.Normalize(_cameraTarget - _cameraPosition);
        _cameraTarget = _cameraDirection * _cameraTargetDistance;

        _world = Matrix.CreateWorld(
            position: _cameraPosition, 
            forward: _cameraDirection, 
            up: Vector3.Up
        );
        _view = Matrix.CreateLookAt(
            cameraPosition: _cameraPosition, 
            cameraTarget: _cameraDirection, 
            cameraUpVector: Vector3.Up
        );
        _projection = Matrix.CreatePerspectiveFieldOfView(
            fieldOfView: _fieldOfView,
            aspectRatio: _aspectRatio,
            nearPlaneDistance: _nearPlane,
            farPlaneDistance: _farPlane
        );
    }

    protected override void Initialize()
    {
        base.Initialize();

        InitializeGraphics();

        //_viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
        _viewportAdapter = new WindowViewportAdapter(Window, GraphicsDevice);
        _camera = new OrthographicCamera(_viewportAdapter);
        
        _effect = new BasicEffect(GraphicsDevice);

        UpdateMatrices();
        
        _particleManager.Initialize();
    }

    private Effect UpdateEffect(ref ParticleGeometry particleGeometry)
    {
        Matrix translation = Matrix.CreateTranslation(particleGeometry.Position);
        Matrix scale = Matrix.CreateScale(particleGeometry.Size * 0.5f);
        
        _effect.World = scale * translation;
        _effect.View = _view;
        _effect.Projection = _projection;
        _effect.Alpha = particleGeometry.Color.A / 255f;

        _effect.VertexColorEnabled = true;
        _effect.TextureEnabled = false;
        _effect.LightingEnabled = false;

        return _effect;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
        _pixelTexture.SetData<Color>(new Color[] { Color.White });

        _particleManager.LoadContent();
    }

    private void HandleInput(GameTime gameTime)
    {
        KeyboardExtended.Update();
        MouseExtended.Update();

        var keyboard = KeyboardExtended.GetState();

        if (keyboard.WasKeyPressed(Keys.Escape) || keyboard.WasKeyPressed(Keys.Q))
        {
            Exit();
        }
        
        if (keyboard.WasKeyPressed(Keys.R))
        {
            IsCameraOrbiting = !IsCameraOrbiting;
        }

        HandleCameraMovement(gameTime);

    }

    private void HandleCameraMovement(GameTime gameTime)
    {
        KeyboardStateExtended keyboard = KeyboardExtended.GetState();
        //MouseStateExtended mouse = MouseExtended.GetState();


        //Vector3 cameraPositionMovement = Vector3.Zero;
        //Vector3 cameraTargetMovement = Vector3.Zero;
        float movementSpeed = 40f * (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        Matrix cameraTranslation = Matrix.Identity;

        if (keyboard.IsKeyDown(Keys.W))
        {
            cameraTranslation *= Matrix.CreateTranslation(_world.Forward * movementSpeed);
            //cameraPositionMovement += Vector3.Forward;
            //cameraTargetMovement += Vector3.Forward;
        }

        if (keyboard.IsKeyDown(Keys.S))
        {
            cameraTranslation *= Matrix.CreateTranslation(_world.Backward * movementSpeed);
            //cameraPositionMovement += Vector3.Backward;
            //cameraTargetMovement += Vector3.Backward;
        }
        
        if (keyboard.IsKeyDown(Keys.A))
        {
            cameraTranslation *= Matrix.CreateTranslation(_world.Left * movementSpeed);
            //cameraPositionMovement += Vector3.Left;
            //cameraTargetMovement += Vector3.Left;
        }
        
        if (keyboard.IsKeyDown(Keys.D))
        {
            cameraTranslation *= Matrix.CreateTranslation(_world.Right * movementSpeed);
            //cameraPositionMovement += Vector3.Right;
            //cameraTargetMovement += Vector3.Right;
        }
        
        if (keyboard.IsKeyDown(Keys.Space))
        {
            cameraTranslation *= Matrix.CreateTranslation(_world.Up * movementSpeed);
            //cameraPositionMovement += Vector3.Up;
            //cameraTargetMovement += Vector3.Up;
        }
        
        if (keyboard.IsControlDown())
        {
            cameraTranslation *= Matrix.CreateTranslation(_world.Down * movementSpeed);
            //cameraPositionMovement += Vector3.Down;
            //cameraTargetMovement += Vector3.Down;
        }

        if (cameraTranslation != Matrix.Identity)
        {
            Vector3.Transform(ref _cameraPosition, ref cameraTranslation, out _cameraPosition);
        }

        //_cameraPosition += cameraPositionMovement * movementSpeed;
        //_cameraTarget += cameraTargetMovement * movementSpeed;
    }

    protected override void Update(GameTime gameTime)
    {
        HandleInput(gameTime);

        if (IsCameraOrbiting)
        {
            float degrees = 10f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Quaternion rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(degrees));
            Vector3.Transform(ref _cameraPosition, ref rotation, out _cameraPosition);
        }

        _particleManager.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _particleManager.Draw(gameTime, ref _particleGeometries);

        GraphicsDevice.Clear(Color.Gray);

        UpdateMatrices();

        if (DrawAsQuads)
        {
            DrawParticleGeometry();
        }

        if (DrawAsSprites)
        {
            DrawParticlesAsSprites();
        }

        base.Draw(gameTime);
    }

    private void DrawParticlesAsSprites()
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        for (int i = 0; i < _particleGeometries.Length; i++)
        {
            ParticleGeometry particle = _particleGeometries[i];

            Vector3 position = _viewportAdapter.Viewport.Project(_particleGeometries[i].Position, _projection, _view, _world);

            if (position.Z > 1)
                continue;

            _spriteBatch.Draw(
                texture: _pixelTexture,
                position: new Vector2(position.X, position.Y),
                sourceRectangle: null,
                color: _particleGeometries[i].Color,
                rotation: 0f,
                origin: _pixelOrigin,
                scale: new Vector2(particle.Size),
                layerDepth: 1.0f,
                effects: SpriteEffects.None
            );
        }

        _spriteBatch.End();
    }
    
    
    private ushort[] _indexData;
    private VertexPositionColor[] _vertexData;
    private int _vertexBufferSize = 0;
    private int _indexBufferSize = 0;
    private unsafe void UpdateBuffers()
    {
        if (_particleGeometries.Length == 0)
            return;

        int totalVtxCount = 0;
        int totalIdxCount = 0;

        for (int i = 0; i < _particleGeometries.Length; i++)
        {
            if (!_particleGeometries[i].IsActive)
                continue;

            totalVtxCount += _particleGeometries[i].Vertices.Length;
            totalIdxCount += _particleGeometries[i].Indices.Length;
        }

        if (totalVtxCount > _vertexBufferSize)
        {
            _vertexBuffer?.Dispose();

            _vertexBufferSize = (int)(totalVtxCount * 1.5f);
            _vertexBuffer = new DynamicVertexBuffer(GraphicsDevice, VertexPositionColor.VertexDeclaration, _vertexBufferSize, BufferUsage.None);
            _vertexData = new VertexPositionColor[_vertexBufferSize];
        }

        if (totalIdxCount > _indexBufferSize)
        {
            _indexBuffer?.Dispose();

            _indexBufferSize = (int)(totalIdxCount * 1.5f);
            _indexBuffer = new DynamicIndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, _indexBufferSize, BufferUsage.None);
            _indexData = new ushort[_indexBufferSize];
        }

        int vtxOffset = 0;
        int idxOffset = 0;
        
        for (int i = 0; i < _particleGeometries.Length; i++)
        {
            if (!_particleGeometries[i].IsActive)
                continue;

            Span<VertexPositionColor> vtxSrc = _particleGeometries[i].Vertices;
            Span<VertexPositionColor> vtxDest = _vertexData.AsSpan().Slice(vtxOffset, vtxSrc.Length);
            vtxSrc.CopyTo(vtxDest);

            Span<ushort> idxSrc = _particleGeometries[i].Indices;
            Span<ushort> idxDest = _indexData.AsSpan().Slice(idxOffset, idxSrc.Length);
            idxSrc.CopyTo(idxDest);

            vtxOffset += vtxSrc.Length;
            idxOffset += idxSrc.Length;
        }

        _vertexBuffer.SetData(_vertexData, 0, _vertexData.Length);
        _indexBuffer.SetData(_indexData, 0, _indexData.Length);
    }


    private void DrawParticleGeometry()
    {
        UpdateBuffers();

        GraphicsDevice.SetVertexBuffer(_vertexBuffer);
        GraphicsDevice.Indices = _indexBuffer;
        
        GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.BlendState = BlendState.AlphaBlend;

        int idxOffset = 0;
        int vtxOffset = 0;
        
        for (int i = 0; i < _particleGeometries.Length; i++)
        {
            if (!_particleGeometries[i].IsActive)
                continue;

            Effect effect = UpdateEffect(ref _particleGeometries[i]);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawIndexedPrimitives(
                    primitiveType: PrimitiveType.TriangleList,
                    baseVertex: vtxOffset,
                    startIndex: idxOffset,
                    primitiveCount: _indexData.Length / 3
                );
            }

            idxOffset += _particleGeometries[i].Indices.Length;
            vtxOffset += _particleGeometries[i].Vertices.Length;
        }
    }

}
