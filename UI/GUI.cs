using Godot;
using Minesweeper3D.Character;
using Minesweeper3D.Object;
using System;
using System.Collections.Generic;

namespace Minesweeper3D.UI;
public partial class GUI : Control
{
    Node2D Cursor, HelpBar, PlayerSign;
    Control LeftUp, LeftDown, RightUp, RightDown;
    AnimatedSprite2D XRayDigit, LeftDownBase;
    Sprite2D ColorL, ColorM, ColorR;
    Label Counter, CoordLab;
    (AnimatedSprite2D H1, AnimatedSprite2D H2, AnimatedSprite2D M1, AnimatedSprite2D M2,
     AnimatedSprite2D S1, AnimatedSprite2D S2, AnimatedSprite2D Ms1, AnimatedSprite2D Ms2) Timer;
    TextureRect Map;

    Player Player;

    #region Attributes
    bool visible;
    public new bool Visible
    {
        get => visible;
        set
        {

            visible = value;
            LeftUp.Visible = LeftDown.Visible =
                RightUp.Visible = RightDown.Visible = value;
            
        }
    }

    public int XrayLevel
    {
        set => XRayDigit.Frame = Math.Min(value, 3);
    }
    static readonly List<Color> FlagColors = new()
    {
        new(0xff8585ff),
        new(0xffff85ff),
        new(0x85ff85ff),
        new(0x85ffffff),
    };
    public Cell.SignState FlagColor
    {
        set
        {
            ColorM.Modulate = FlagColors[(int)value];
            ColorL.Modulate = FlagColors[((int)value + 3) % 4];
            ColorR.Modulate = FlagColors[((int)value + 1) % 4];
        }
    }
    int Difficulty
    {
        set => LeftDownBase.Frame = value;
    }
    public int Count
    {
        set => Counter.Text = $"{value}";
    }
    public Vector2 MapSize { get; set; }
    Vector3 PlayerPosition
    {
        set
        {
            CoordLab.Text = $"{(int)value.x} {(int)value.y} {(int)value.z}";
            Vector2 positionShift = Vector2.Zero;
            if (MapSize.x <= MapSize.y)
            {
                positionShift.x = (1 - (MapSize.x / MapSize.y)) / 2;
            }
            else
            {
                positionShift.y = (1 - (MapSize.y / MapSize.x)) / 2;
            }
            Vector2 position =
                (new Vector2(value.x, value.z) / Math.Max(MapSize.x, MapSize.y)) + positionShift;
            position = new Vector2
                (Math.Max(0.05f, Math.Min(0.95f, position.x)), Math.Max(0.05f, Math.Min(0.95f, position.y))) * 256;
            PlayerSign.Position = position;
        }
    }
    float PlayerRotation { set => PlayerSign.Rotation = (float)Math.PI - value; }
    TimeSpan time = new(0);
    public TimeSpan Time
    {
        get => time;
        set
        {
            time = value;
            Timer.H1.Frame = value.Hours / 10;
            Timer.H2.Frame = value.Hours % 10;
            Timer.M1.Frame = value.Minutes / 10;
            Timer.M2.Frame = value.Minutes % 10;
            Timer.S1.Frame = value.Seconds / 10;
            Timer.S2.Frame = value.Seconds % 10;
            Timer.Ms1.Frame = value.Milliseconds / 100;
            Timer.Ms2.Frame = value.Milliseconds / 10 % 10;
        }
    }
    #endregion

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        {
            Cursor          = GetNode<Node2D>("Cursor");

            LeftUp          = GetNode<Control>("LeftUp");
            LeftDown        = GetNode<Control>("LeftDown");
            RightUp         = GetNode<Control>("RightUp");
            RightDown       = GetNode<Control>("RightDown");

            CoordLab        = GetNode<Label>("LeftUp/Map/Position");
            Map             = GetNode<TextureRect>("LeftUp/Map/Map");
            PlayerSign      = GetNode<Node2D>("LeftUp/Map/PlayerSign");

            HelpBar         = GetNode<Node2D>("LeftDown/Bar/Help");
            LeftDownBase    = GetNode<AnimatedSprite2D>("LeftDown/Bar/Base");
            Counter         = GetNode<Label>("LeftDown/Bar/Counter");

            Timer.H1        = GetNode<AnimatedSprite2D>("RightUp/Time/H1");
            Timer.H2        = GetNode<AnimatedSprite2D>("RightUp/Time/H2");
            Timer.M1        = GetNode<AnimatedSprite2D>("RightUp/Time/M1");
            Timer.M2        = GetNode<AnimatedSprite2D>("RightUp/Time/M2");
            Timer.S1        = GetNode<AnimatedSprite2D>("RightUp/Time/S1");
            Timer.S2        = GetNode<AnimatedSprite2D>("RightUp/Time/S2");
            Timer.Ms1       = GetNode<AnimatedSprite2D>("RightUp/Time/Ms1");
            Timer.Ms2       = GetNode<AnimatedSprite2D>("RightUp/Time/Ms2");

            XRayDigit       = GetNode<AnimatedSprite2D>("RightDown/Slot/Xborder/AnimatedSprite2d");
            ColorL          = GetNode<Sprite2D>("RightDown/Slot/Color/L");
            ColorM          = GetNode<Sprite2D>("RightDown/Slot/Color/M");
            ColorR          = GetNode<Sprite2D>("RightDown/Slot/Color/R");
        }//GetNode
        Visible = false;
        Cursor.Position = GetViewportRect().Size / 2;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Control();


        PlayerPosition = Player?.Position ?? Vector3.Zero;
        PlayerRotation = Player?.Rotation.y ?? 0;
    }

    public override void _Input(InputEvent @event)
    {
    }
    public void Control()
    {
        if (Input.IsActionPressed("SwitchCursor") || GetTree().Paused)
            Cursor.Position = GetGlobalMousePosition();
        else
            Cursor.Position = GetViewportRect().Size / 2;

        if (Input.IsActionJustPressed("Help"))
        {
            SwitchHelpShow();
        }
    }

    public bool SwitchHelpShow()
    {
        if (HelpBar.Visible)
            HelpBar.Visible = false;
        else
            HelpBar.Visible = true;
        return HelpBar.Visible;
    }
    public void Initialize(Player player,int bombCount,Viewport map,int difficulty,Vector2 mapSize)
    {
        Player = player;
        Count = bombCount;
        Map.Texture = map.GetTexture();
        Difficulty = difficulty;
        MapSize = mapSize;
        Visible = true;
    }
}
