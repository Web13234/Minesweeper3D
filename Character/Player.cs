using Godot;
using Minesweeper3D.Object;
using System;

namespace Minesweeper3D.Character;
public partial class Player : CharacterBody3D
{
    [Export]
    private float AcceleratedSpeed;
    [Export]
    private float PreciseMoveSpeed;
    [Export]
    private float AirDrag;
    [Export]
    private float MaxSpeed;
    [Export]
    private float MouseDBX, MouseDBY;
    [Export]
    private float RayLength;

    [Signal]
    public delegate void SignModeChangedEventHandler(Cell.SignState signState);
    [Signal]
    public delegate void XRayLevelChangedEventHandler(int level);

    public Camera3D Camera { get; set; }
    private RayCast3D Ray { get; set; }

    private Vector3 MoveVector = new();
    private Object.Item TargetItem = null;
    private Object.Item preTarget = null;

    private Cell.SignState signState = 0;
    public Cell.SignState SignState
    {
        get => signState;
        set
        {
            signState = value;
            EmitSignal(nameof(SignModeChanged), (int)value);
            GD.Print("SignChange");
        }
    }
    private int xrayLevel;
    public int XrayLevel
    {
        get => xrayLevel;
        set
        {
            xrayLevel = Math.Max(0, Math.Min(value, 3));
            EmitSignal(nameof(XRayLevelChanged), xrayLevel);
            GD.Print("XrayChange");
        }
    }

    public new Vector3 Rotation
    {
        get => Camera.Rotation;
        set => Camera.Rotation = value;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Camera = GetNode<Camera3D>("Camera");
        Ray = GetNode<RayCast3D>("Camera/RayCast");
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
        Control();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            if (Input.IsActionPressed("SwitchCursor"))
            {
                Vector3 cursorPosition = Camera.ProjectRayNormal(mouseMotion.Position) * RayLength;
                Ray.TargetPosition = cursorPosition.Rotated(Vector3.Up, -Camera.Rotation.y).Rotated(Vector3.Right, -Camera.Rotation.x);
            }
            else
            {
                Vector2 rotationVector = new(mouseMotion.Relative.x * -0.01f * MouseDBX, mouseMotion.Relative.y * -0.01f * MouseDBY);
                Camera.Rotation =
                    new(Math.Min(Math.Max((Camera.Rotation.x + rotationVector.y), -Mathf.Pi / 2), Mathf.Pi / 2),
                        Camera.Rotation.y + rotationVector.x, 0);
            }
        }
        if(@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.Pressed)
            {
                if (Input.IsActionPressed("PreciseControl"))
                {
                    switch (mouseButton.ButtonIndex)
                    {
                        case MouseButton.WheelUp:
                            XrayLevel = (XrayLevel + 1) % 4;
                            break;
                        case MouseButton.WheelDown:
                            XrayLevel = (XrayLevel + 3) % 4;
                            break;
                    }
                }
                else
                {
                    switch (mouseButton.ButtonIndex)
                    {
                        case MouseButton.WheelUp:
                            SignState = (Cell.SignState)(((int)SignState + 3) % 4);
                            break;
                        case MouseButton.WheelDown:
                            SignState = (Cell.SignState)(((int)SignState + 1) % 4);
                            break;
                    }
                }
            }
        }
        //if (@event is InputEventAction action)
        //{
        //    switch (action.Action)
        //    {
        //        case "XrayUp":
        //            XrayLevel = (XrayLevel + 1) % 4;
        //            break;
        //        case "XrayDown":
        //            XrayLevel = (XrayLevel + 3) % 4;
        //            break;
        //        case "SignUp":
        //            SignState = (Cell.SignState)(((int)SignState + 1) % 4);
        //            break;
        //        case "SignDown":
        //            SignState = (Cell.SignState)(((int)SignState + 3) % 4);
        //            break;
        //    }
        //}
    }
    public void Control()
    {
        if (Input.IsActionJustPressed("SwitchCursor"))
            Input.MouseMode = Input.MouseModeEnum.ConfinedHidden;
        if (Input.IsActionJustReleased("SwitchCursor"))
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            Ray.TargetPosition = Vector3.Forward * RayLength;
        }
        if (Input.IsActionJustPressed("Sign"))
        {
            TargetItem?.RmbClick(SignState);
        }
        if (Input.IsActionJustPressed("Mine"))
        {
            TargetItem?.LmbClick();
        }

        //if (Input.IsActionPressed("PreciseControl"))
        //{
        //    if (Input.IsMouseButtonPressed(MouseButton.WheelUp))
        //    {
        //        XrayLevel = (XrayLevel + 3) % 4;
        //    }
        //    if (Input.IsMouseButtonPressed(MouseButton.WheelDown))
        //    {
        //        XrayLevel = (XrayLevel + 1) % 4;
        //    }
        //}
        //else
        //{
        //    if (Input.IsMouseButtonPressed(MouseButton.WheelUp))
        //    {
        //        SignState = (Cell.SignState)(((int)SignState + 3) % 4);
        //    }
        //    if (Input.IsMouseButtonPressed(MouseButton.WheelDown))
        //    {
        //        SignState = (Cell.SignState)(((int)SignState + 1) % 4);
        //    }
        //}

        //if (Input.IsActionJustPressed("XrayUp"))
        //{
        //    XrayLevel = (XrayLevel + 3) % 4;
        //}
        //if (Input.IsActionJustPressed("XrayDown"))
        //{
        //    XrayLevel = (XrayLevel + 1) % 4;
        //}
        //if (Input.IsActionJustPressed("SignUp"))
        //{
        //    SignState = (Cell.SignState)(((int)SignState + 3) % 4);
        //}
        //if (Input.IsActionJustPressed("SignDown"))
        //{
        //    SignState = (Cell.SignState)(((int)SignState + 3) % 4);
        //}
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Move(delta);
        var targetBody = Ray.GetCollider();
        if(targetBody is Item item)
        {
            TargetItem = item;
            if (!Item.ReferenceEquals(TargetItem, preTarget))
            {
                preTarget?.SetChosen(false);
                TargetItem.SetChosen();
            }
        }
        else
        {
            TargetItem = null;
            try
            {
                preTarget?.SetChosen(false);
            }
            //so bad a idea,but no choice
            catch (ObjectDisposedException) { }
        }
        preTarget = TargetItem;
    }

    private void Move(double delta)
    {
        Vector3 moveDirection = new();
        if (Input.IsActionPressed("MoveFront"))
            moveDirection.z -= 1;
        if (Input.IsActionPressed("MoveBack"))
            moveDirection.z += 1;
        if (Input.IsActionPressed("MoveLeft"))
            moveDirection.x -= 1;
        if (Input.IsActionPressed("MoveRight"))
            moveDirection.x += 1;
        if (Input.IsActionPressed("MoveDown"))
            moveDirection.y -= 1;
        if (Input.IsActionPressed("MoveUp"))
            moveDirection.y += 1;
        moveDirection = moveDirection.Rotated(Vector3.Up, Camera.Rotation.y);
        moveDirection = moveDirection.Normalized();

        if (Input.IsActionPressed("PreciseControl"))
        {
            MoveVector = moveDirection * PreciseMoveSpeed;
        }
        else
        {
            MoveVector *= (float)Math.Pow(1 / (AirDrag + 1), delta);
            MoveVector += moveDirection * AcceleratedSpeed * (float)delta;
            MoveVector = MoveVector.LimitLength(MaxSpeed);

            //MoveVector = moveDirection * AcceleratedSpeed;
        }

        //MoveAndSlide(MoveVector);
        Velocity = MoveVector;
        MoveAndSlide();
        MoveVector = Velocity;
    }
    public void Reset()
    {
        TargetItem = null;
        preTarget = null;
        SignState = 0;
        XrayLevel = 0;
    }
}
