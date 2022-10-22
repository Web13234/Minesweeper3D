using Godot;
using System;
using System.Collections.Generic;

namespace Minesweeper3D.Object;

public partial class Sphere : Item
{
    private MeshInstance3D TextInstance;
    [Export]
    public Color[] Colors;

    static List<TextMesh> Shapes;

    (Material Normal, Material Highlighted) Material;
    public bool Highlighted
    {
        set => GetNode<MeshInstance3D>("Model/Circle").SetSurfaceOverrideMaterial
            (0, value ? Material.Highlighted : Material.Normal);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (Shapes == null)
        {
            Shapes = new();
            for (int i = 0; i < 26; i++)
            {
                StandardMaterial3D material = new()
                {
                    BillboardMode = BaseMaterial3D.BillboardModeEnum.Enabled,
                    EmissionEnabled = true,
                    Emission = Colors[i],
                    EmissionEnergyMultiplier = 1.5f,
                    AlbedoColor = Colors[i],
                };
                TextMesh textMesh = new()
                {
                    Font = ResourceLoader.Load<FontFile>("res://Font/SourceHanSansHWSC-Regular.otf"),
                    FontSize = 24,
                    CurveStep = 1,
                    Depth = 0,
                    Material = material,
                    Text = $"{i + 1}",
                };
                Shapes.Add(textMesh);
            }
        }
        base._Ready();
    }

    public void Initialize(int number)
    {

        TextInstance = GetNode<MeshInstance3D>("Model/Text");
        TextInstance.Mesh = Shapes[number - 1];

        Material = new(
            ResourceLoader.Load<Material>("res://Object/Material/Sphere.tres"),
            ResourceLoader.Load<Material>("res://Object/Material/SphereHignlighted.tres"));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}
