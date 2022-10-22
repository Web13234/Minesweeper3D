using Godot;
using System;
using System.Collections.Generic;
using static Minesweeper3D.Object.Cell;

namespace Minesweeper3D.Object;
public partial class Block : Item
{
    static StandardMaterial3D light, lightChosen;
    List<Face> Faces;
    private Material FaceLight
    {
        set
        {
            foreach (Face face in Faces)
            {
                face.SetSurfaceOverrideMaterial(2, value);
            }
        }
    }

    static Dictionary<SignState, Material> SignMaterial { get; set; }
    public SignState SignState
    {
        set
        {
            foreach (Face face in Faces)
            {
                if (face.ScreenVisible = value != SignState.None)
                    face.ScreenMaterial = SignMaterial[value];
            }
        }
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Faces = new List<Face>()
        {
            GetNode<Face>("Model/FaceFront"),
            GetNode<Face>("Model/FaceBack"),
            GetNode<Face>("Model/FaceLeft"),
            GetNode<Face>("Model/FaceRight"),
            GetNode<Face>("Model/FaceUp"),
            GetNode<Face>("Model/FaceDown"),
        };
        light ??= ResourceLoader.Load<StandardMaterial3D>("res://Object/Material/BlockLight.tres");
        lightChosen ??= ResourceLoader.Load<StandardMaterial3D>("res://Object/Material/BlockLightChosen.tres");

        SignMaterial = new()
        {
            [SignState.Flag] = ResourceLoader.Load<Material>("res://Object/Material/FaceFlag.tres"),
            [SignState.Sign0] = ResourceLoader.Load<Material>("res://Object/Material/FaceSign0.tres"),
            [SignState.Sign1] = ResourceLoader.Load<Material>("res://Object/Material/FaceSign1.tres"),
            [SignState.Sign2] = ResourceLoader.Load<Material>("res://Object/Material/FaceSign2.tres"),
        };

        base._Ready();
        SignState = SignState.None;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    public void OnChosenChanged()
    {
        if (GetNode<Node3D>("Model/Chosen").Visible)
            FaceLight = lightChosen;
        else
            FaceLight = light;
    }
}
