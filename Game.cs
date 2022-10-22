using Godot;
using Godot.Collections;
using Minesweeper3D.Character;
using Minesweeper3D.Object;
using Minesweeper3D.Script;
using Minesweeper3D.UI;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Minesweeper3D;
public partial class Game : Node
{
    [Signal]
    public delegate void XrayLevelChangedEventHandler(int level);

    bool IsInGame { get; set; } = false;
    Cell testCell;
    PackedScene PackedCell;

    Grid<Cell> GameGrid;

    Node3D WorldBox, GridNode;
    Player Player;
    Camera3D MapCamera;

    SubViewport SubViewport;
    GUI GUI;
    Menu Menu;

    DateTime time0 = DateTime.MinValue;
    DateTime? Time0
    {
        get => time0 == DateTime.MinValue ? null : time0;
        set
        {
            if (value == null)
                time0 = DateTime.MinValue;
            else
                time0 = (DateTime)value;
        }
    }
    TimeSpan CurrentTime;

    DateTime PauseTime;

    WorldEnvironment WorldEnvironment;
    int CellsLeft, BombsLeft, BombsCount;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        PackedCell = ResourceLoader.Load<PackedScene>("res://Object/Cell.tscn");
        WorldBox = GetNode<Node3D>("WorldBox");
        GridNode = GetNode<Node3D>("Grid");
        Player = GetNode<Player>("Player");
        Menu = GetNode<Menu>("Menu");
        GUI = GetNode<GUI>("GUI");
        SubViewport = GetNode<SubViewport>("SubViewport");
        MapCamera = GetNode<Camera3D>("SubViewport/MapCamera");
        WorldEnvironment = GetNode<WorldEnvironment>("WorldEnvironment");

        Pause();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Control();

        if (Time0 != null)
        {
            CurrentTime = DateTime.Now - (DateTime)Time0;
            GUI.Time = CurrentTime;
        }
    }

    public override void _Input(InputEvent @event)
    {
    }
    public void Control()
    {

        if (Input.IsActionJustPressed("FinishGame") && BombsLeft == 0)
        {
            foreach (Cell cell in GameGrid.GetCells())
            {
                cell.OnSweep();
            }
        }

        if (Input.IsActionJustPressed("Exit"))
        {
            //GetTree().Quit();
            if (IsInGame)
                SwitchPause();
        }
        if (Input.IsActionJustPressed("test0"))
        {
            testCell?.QueueFree();
            testCell = PackedCell.Instantiate<Cell>();
            AddChild(testCell);
        }
        if (Input.IsActionJustPressed("test1"))
        {
            //testCell.State = Cell.CellState.Cube;
            Initialation(new(4, 4, 6), 1);
        }
        if (Input.IsActionJustPressed("test2"))
        {
            //testCell.State = Cell.CellState.Block;
            GameOver();
        }
        if (Input.IsActionJustPressed("test3"))
        {
            //testCell.State = Cell.CellState.Null;
        }
    }

    void Exit()
    {
        GetTree().Quit();
    }

    public void Initialation(Vector3i size, int count, int? seed = null)
    {
        GameOver();
        WorldBox.Scale = size;
        MapCamera.Size = Math.Max(size.x, size.z);
        MapCamera.Position = new(size.x / 2, size.y + 1, size.z / 2);

        int difficulty = Minesweeper3D.UI.Menu.Difficulty.IndexOf((size, count));
        GUI.Initialize(Player, count, SubViewport, difficulty, new(size.x, size.z));

        #region Grid
        GridNode?.QueueFree();
        GridNode = null;
        GridNode = new Node3D();
        AddChild(GridNode);

        Grid<Cell>.SpawnSize = Grid<int>.SpawnSize = size;
        Grid<int>.Spawn();
        Grid<Cell>.Spawn();
        Grid<int> gridi = Grid<int>.GetInstance();
        GameGrid = Grid<Cell>.GetInstance();

        Random random;
        if (seed != null)
        {
            random = new Random((int)seed);
        }
        else
        {
            random = new Random();
        }
        List<Vector3i> bombs = new();
        for (int i = 0; i < count; i++)
        {
            Vector3i tempVector;
            do
            {
                tempVector = new(random.Next(size.x), random.Next(size.y), random.Next(size.z));
            }
            while (gridi[tempVector] == 27);
            gridi[tempVector] = 27;
            bombs.Add(tempVector);
        }
        foreach (Vector3i position in bombs)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        if ((i != 0 || j != 0 || k != 0) && gridi.IsInRange(position + new Vector3i(i, j, k)) && gridi[position + new Vector3i(i, j, k)] != 27)
                            gridi[position + new Vector3i(i, j, k)] += 1;
                    }
                }
            }
        }

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int k = 0; k < size.z; k++)
                {
                    Cell cell = PackedCell.Instantiate<Cell>();
                    GridNode.AddChild(cell);
                    GameGrid[i, j, k] = cell;
                }
            }
        }

        Vector3i nullPosition;
        do
        {
            nullPosition = new(random.Next(size.x), random.Next(size.y), random.Next(size.z));
        }
        while (gridi[nullPosition] != 0);

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int k = 0; k < size.z; k++)
                {
                    Vector3i position = new(i, j, k);

                    int visibleLevel = Math.Min(4, Math.Min(Math.Min(Math.Min(i + 1, size.x - i), Math.Min(j + 1, size.y - j)), Math.Min(k + 1, size.z - k)));

                    GameGrid[i, j, k].Initialize
                        (position, gridi[position], GameGrid.GetNear(position),
                        GameGrid.GetAround(position), this, visibleLevel, position == nullPosition);
                }
            }
        }
        CellsLeft = size.x * size.y * size.z;
        BombsCount = BombsLeft = count;

        Player.Position = nullPosition + Vector3.One / 2;
        GameGrid[nullPosition].OnSweep();
        #endregion

        Time0 = DateTime.Now;
        IsInGame = true;
        EmitSignal(nameof(XrayLevelChanged), 0);
        Continue();
    }

    void GameOver()
    {
        if (GameGrid != null)
            foreach(Cell cell in GameGrid.GetCells())
            {
                XrayLevelChanged -= cell.OnVisibleLevelChanged;
            }
        Time0 = null;
        GridNode?.QueueFree();
        GridNode = null;
        Player.Reset();
        GUI.Visible = false;
        IsInGame = false;
        Pause();
    }

    void SwitchPause()
    {
        if (GetTree().Paused)
        {
            Continue();
            #pragma warning disable CS8620
            Time0 += DateTime.Now - PauseTime;
            #pragma warning restore CS8620
        }
        else
        {
            Pause();
            PauseTime = DateTime.Now;
        }
    }
    void Pause()
    {
        Input.MouseMode = Input.MouseModeEnum.Hidden;
        GUI.Visible = false;
        Menu.Visible = true;
        WorldEnvironment.Environment.GlowBlendMode = Godot.Environment.GlowBlendModeEnum.Replace;

        GetTree().Paused = true;
    }
    void Continue()
    {
        GetTree().Paused = false;

        Input.MouseMode = Input.MouseModeEnum.Captured;
        GUI.Visible = true;
        Menu.Visible = false;
        WorldEnvironment.Environment.GlowBlendMode = Godot.Environment.GlowBlendModeEnum.Screen;
    }

    //signal
    public void StartWithoutSeed(Vector3i size, int count) => Initialation(size, count);
    public void Start(Vector3i size, int count, int seed) => Initialation(size, count, seed);
    public void OnOneSweeped()
    {
        CellsLeft--;
        if (CellsLeft == BombsCount)
        {
            GameWin();
        }
    }
    public void OnOneSigned(bool signed)
    {
        if (signed) BombsLeft--;
        else BombsLeft++;
        GUI.Count = BombsLeft;
    }
    public void OnPlayerSignModeChanged(Cell.SignState signState)
    {
        GUI.FlagColor = signState;
    }
    public void OnXrayLevelChanged(int level)
    {
        EmitSignal(nameof(XrayLevelChanged), level);
        GUI.XrayLevel = level;
    }
    public void GameWin()
    {
        GD.Print("Game Win");
        GameOver();
        Menu.ShowResult(CurrentTime, Menu.GameResult.Win);
    }
    public void GameLose()
    {
        GD.Print("Game Lose");
        GameOver();
        Menu.ShowResult(CurrentTime, Menu.GameResult.Lose);
    }
}