using Godot;
using System;
using System.Collections.Generic;

namespace Minesweeper3D.UI;
public partial class Menu : Control
{
    [Signal]
    public delegate void GameStartEventHandler(Vector3i size, int count, int seed);
    [Signal]
    public delegate void GameStartWithoutSeedEventHandler(Vector3i size, int count);
    [Signal]
    public delegate void ExitGameEventHandler();

    (SpinBox X, SpinBox Y, SpinBox Z) SizeBox;
    SliderWithBox CountSlider;

    Control ResultWindow, WinText, LoseText;

    public static List<(Vector3i, int)> Difficulty { get; set; }
        = new()
        {
            (new(8, 8, 8), 16),
            (new(12, 12, 12), 128),
            (new(16, 16, 16), 512),
        };
    Vector3i GameSize
    {
        get => new((int)SizeBox.X.Value, (int)SizeBox.Y.Value, (int)SizeBox.Z.Value);
        set
        {
            (SizeBox.X.Value, SizeBox.Y.Value, SizeBox.Z.Value) = (value.x, value.y, value.z);
            OnGameSizeChanged();
        }
    }
    int Count
    {
        get => (int)CountSlider.Value;
        set => CountSlider.Value = value;
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SizeBox.X = GetNode<SpinBox>("VBoxContainer/Size/X");
        SizeBox.Y = GetNode<SpinBox>("VBoxContainer/Size/Y");
        SizeBox.Z = GetNode<SpinBox>("VBoxContainer/Size/Z");
        CountSlider = GetNode<SliderWithBox>("VBoxContainer/Count/SliderWithBox");
        ResultWindow = GetNode<Control>("Result");
        WinText = GetNode<Control>("Result/Win");
        LoseText = GetNode<Control>("Result/Lose");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
    public enum GameResult
    {
        Lose,
        Win,
    }
    public void ShowResult(TimeSpan time, GameResult result)
    {
        GetNode<Label>("Result/VBoxContainer/Time/Value").Text = time.ToString();
        ResultWindow.Visible = true;
        LoseText.Visible = !(WinText.Visible = result == GameResult.Win);
    }
    public void HideResult()
    {
        ResultWindow.Visible = false;
    }

    //Signal
    public void OnNewGameButtonPressed()
    {
        string seed = GetNode<LineEdit>("VBoxContainer/Seed/LineEdit").Text;

        GetNode<LineEdit>("Result/VBoxContainer/Seed/Value").Text = seed;
        GetNode<Label>("Result/VBoxContainer/Count/Value").Text = $"{Count}";
        GetNode<Label>("Result/VBoxContainer/Size/Value").Text = $"{GameSize.x}x{GameSize.y}x{GameSize.z}";
        GetNode<LineEdit>("Result/VBoxContainer/Difficulty/Value").Text =
            Difficulty.IndexOf((GameSize, Count)) switch
            {
                0 => "Easy",
                1 => "Normal",
                2 => "Hard",
                _ => "Custom"
            };

        if (seed != "")
            EmitSignal(nameof(GameStart), GameSize, Count, seed.GetHashCode());
        else
            EmitSignal(nameof(GameStartWithoutSeed), GameSize, Count);
    }
    public void OnExitGameButtonPressed()
    {
        EmitSignal(nameof(ExitGame));
    }
    public void OnGameSizeChanged(int i = 0)
    {
        CountSlider.MaxValue = GameSize.x * GameSize.y * GameSize.z / 8;
        GD.Print($"MaxChanged:{CountSlider.MaxValue}");
    }
    public void OnDifficultyValueChanged(int difficulty)
    {
        SizeBox.X.Editable = SizeBox.Y.Editable
            = SizeBox.Z.Editable = CountSlider.Editable
            = difficulty == 0;

        if(difficulty != 0)
            (GameSize, Count) = Difficulty[difficulty - 1];

        GetNode<LineEdit>("VBoxContainer/Difficulty/LineEdit").Text =
            difficulty switch
            {
                0 => "Custom",
                1 => "Easy",
                2 => "Normal",
                3 => "Hard",
                _ => "ERROR",
            };
    }
}
