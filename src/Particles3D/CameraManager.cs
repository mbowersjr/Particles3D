using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Particles3D.Managers;

public class CameraManager
{
    private Matrix _world;
    public Matrix World
    {
        get => _world;
        set => _world = value;
    }

    private Matrix _view;
    public Matrix View
    {
        get => _view;
        set => _view = value;
    }

    private Matrix _projection;
    public Matrix Projection
    {
        get => _projection;
        set => _projection = value;
    }

    private Vector3 _direction;
    public Vector3 Direction
    {
        get => _direction;
        set => _direction = value;
    }

    public Vector3 Position
    {
        get => _world.Translation;
        set => _world.Translation = value;
    }

    private Vector3 _target;
    public Vector3 Target
    {
        get => _target;
        set
        {
            _target = value;
            World = Matrix.CreateWorld(
                position: Position,
                forward: _target - Position,
                up: World.Up
            );
        }
    }

    public Vector3 Forward
    {
        get => World.Forward;
        set
        {
            World = Matrix.CreateWorld(
                position: Position,
                forward: Vector3.Normalize(value),
                up: Vector3.Up
            );
        }
    }
    
    public float FieldOfView { get; set; }
    public float AspectRatio { get; set; }
    public float NearPlane { get; set; } = 1f;
    public float FarPlane { get; set; } = 100000f;


    private GameMain _game;
    private GraphicsDevice _graphicsDevice;

    public CameraManager(GameMain game)
    {
        _game = game;
        _graphicsDevice = _game.GraphicsDevice;

        AspectRatio = (float)_graphicsDevice.PresentationParameters.BackBufferWidth / (float)_graphicsDevice.PresentationParameters.BackBufferHeight;
        FieldOfView = MathHelper.ToRadians(45f);

        World = Matrix.CreateWorld(
            position: Position, 
            forward: Target - Position, 
            up: Vector3.Up
        );
        View = Matrix.CreateLookAt(
            cameraPosition: Position,
            cameraTarget: Target,
            cameraUpVector: Vector3.Up
        );
        Projection = Matrix.CreatePerspectiveFieldOfView(
            fieldOfView: FieldOfView, 
            aspectRatio: AspectRatio, 
            nearPlaneDistance: NearPlane, 
            farPlaneDistance: FarPlane
        );
    }
    
    public void OrbitTargetLeftRight(Vector3 target, float distance, float angle)
    {
        var rotation = Matrix.CreateRotationY(angle);
        World *= rotation;
        Position = Vector3.Normalize(Position - target) * distance;
        Target = target;
    }

    public void OrbitTargetUpDown(Vector3 target, float distance, float angle)
    {
        var rotation = Matrix.CreateRotationX(angle);
        World *= rotation;
        Position = Vector3.Normalize(Position - target) * distance;
        Target = target;
    }

    public void MoveTowardsAway(Vector3 target, float distance)
    {
        Position = Vector3.Normalize(Position - target) * distance;
        Target = target;
    }
}