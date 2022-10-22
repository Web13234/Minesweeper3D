using Godot;
using System;
using System.Threading;

namespace Minesweeper3D.Object;
public partial class Item : StaticBody3D
{
    [Signal]
    public delegate void OnLmbClickEventHandler();
    [Signal]
    public delegate void OnRmbClickEventHandler(Cell.SignState signState);

    private Node3D Model, ChosenModel;
    private CollisionShape3D CollisionShape3D;
    private AnimationPlayer AnimationPlayer;

    protected bool enabled = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Model = GetNode<Node3D>("Model");
        ChosenModel = GetNode<Node3D>("Model/Chosen");
        CollisionShape3D = GetNode<CollisionShape3D>("CollisionShape3D");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Model.Visible = ChosenModel.Visible = false;
        CollisionShape3D.Disabled = true;
    }

    public override void _Process(double delta)
    {
    }

    public void LmbClick()
    {
        EmitSignal(nameof(OnLmbClick));
    }
    public void RmbClick(Cell.SignState signState)
    {
        EmitSignal(nameof(OnRmbClick),(int)signState);
    }

    public void Enable(bool instant = false)
    {
        if (AnimationPlayer.CurrentAnimation == "Disappear")
        {
            AnimationPlayer.Stop();
            OnEnableSwitchFinished("Disappear");
        }
        if (!enabled)
        {
            Model.Visible = true;
            if (!instant)
            {
                AnimationPlayer.Play("Appear");
            }
            else
            {
                Model.Scale = Vector3.One;
                OnEnableSwitchFinished("Appear");
            }
        }
    }
    public void Disable(bool instant = false)
    {
        if (AnimationPlayer.CurrentAnimation == "Appear")
        {
            AnimationPlayer.Stop();
            OnEnableSwitchFinished("Appear");
        }
        if (enabled)
        {
            CollisionShape3D.Disabled = true;
            if (!instant)
            {
                AnimationPlayer.Play("Disappear");
            }
            else
            {
                Model.Scale = Vector3.Zero;
                OnEnableSwitchFinished("Disappear");
            }
        }
    }
    public void SetEnable(bool enable,bool instant = false)
    {
        if (enable)
            Enable(instant);
        else
            Disable(instant);
    }
    
    public void SetChosen(bool chosen = true)
    {
        ChosenModel.Visible = chosen;
    }

    public void OnEnableSwitchFinished(string animationName)
    {
        switch (animationName)
        {
            case "Appear":
                CollisionShape3D.Disabled = false;
                enabled = true;
                break;
            case "Disappear":
                Model.Visible = false;
                enabled = false;
                break;
        }
    }
}
