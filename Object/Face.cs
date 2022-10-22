using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

namespace Minesweeper3D.Object;
public partial class Face : MeshInstance3D
{
	public bool ScreenVisible
	{
		get => GetNode<MeshInstance3D>("Screen").Visible;
		set => GetNode<MeshInstance3D>("Screen").Visible = value;

    }
	public Material ScreenMaterial
	{
		get => GetNode<MeshInstance3D>("Screen").GetSurfaceOverrideMaterial(0);
		set => GetNode<MeshInstance3D>("Screen").SetSurfaceOverrideMaterial(0, value);
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
