using Godot;
using Godot.Collections;
using System;
using static Minesweeper3D.Object.Cell;

namespace Minesweeper3D.Object;
public partial class Cube : Item
{
    MeshInstance3D Screen;
    static Dictionary<SignState, Material> SignMaterial { get; set; }
    public SignState SignState
    {
        set
        {
            if (Screen.Visible = value != SignState.None)
                Screen.SetSurfaceOverrideMaterial(0, SignMaterial[value]);
        }
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SignMaterial ??= new()
        {
            [SignState.Flag] = ResourceLoader.Load<Material>("res://Object/Material/CubeFlag.tres"),
            [SignState.Sign0] = ResourceLoader.Load<Material>("res://Object/Material/CubeSign0.tres"),
            [SignState.Sign1] = ResourceLoader.Load<Material>("res://Object/Material/CubeSign1.tres"),
            [SignState.Sign2] = ResourceLoader.Load<Material>("res://Object/Material/CubeSign2.tres"),
        };
        Screen = GetNode<MeshInstance3D>("Model/Screen");
        base._Ready();
        SignState = SignState.None;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
          base._Process(delta);
    }
}
