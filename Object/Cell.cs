using Godot;
using Minesweeper3D.Script;
using System;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace Minesweeper3D.Object;
public partial class Cell : Node3D
{
	[Signal]
	public delegate void SweepAroundEventHandler();
	[Signal]
	public delegate void VisibleLevelSignalEventHandler(int currentVisibleLevel);
	[Signal]
	public delegate void BoomEventHandler();
	[Signal]
	public delegate void SweepedEventHandler();
	[Signal]
	public delegate void SignedEventHandler(bool signed);

	Cube Cube;
	Block Block;
	Sphere Sphere;
	public int Inside { get; private set; }

    private int aroundSignedNumber = 0;
	private SignState sign = SignState.None;
	public SignState Sign
	{
		get => sign;
		set
		{
			if (value == sign)
			{
				if (value == SignState.Flag)
					EmitSignal(nameof(Signed), false);
				sign = SignState.None;
			}
			else
			{
				if (value == SignState.Flag)
					EmitSignal(nameof(Signed), true);
				else if (sign == SignState.Flag)
					EmitSignal(nameof(Signed), false);
				sign = value;
			}
			Block.SignState = sign;
			Cube.SignState = sign;
		}
	}

	private CellState state;
	public CellState State
	{
		get => state;
		set => SwitchState(value);
	}

	private int visibleLevel = 4;
	private int VisibleLevel 
	{
		get => visibleLevel;
		set
		{
			visibleLevel = value;
			if (value <= watchLevel && State == CellState.Block)
			{
				SwitchState(CellState.Cube);
			}
			else if (value >= watchLevel && State == CellState.Cube)
			{
				SwitchState(CellState.Block);
			}
            if (visibleLevel > watchLevel + 1)
                Visible = false;
            else
                Visible = true;
        }
	}
	private int watchLevel = 0;

	private void SwitchState(CellState value,bool instant = false)
	{
		state = value;
		bool cube, block, sphere;
		cube = block = sphere = false;
		switch (value)
		{
			case CellState.Null:
				break;
			case CellState.Number:
				sphere = true;
				break;
			case CellState.Block:
				block = true;
				break;
			case CellState.Cube:
				cube = true;
				break;
			default:
				throw new ArgumentOutOfRangeException($"Unknown CellState:{value}");
		}
		Sphere.SetEnable(sphere,instant);
		Block.SetEnable(block,instant);
		Cube.SetEnable(cube, instant);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Cube = GetNode<Cube>("Cube");
		Block = GetNode<Block>("Block");
		Sphere = GetNode<Sphere>("Sphere");
		Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Initialize(Vector3i position, int inside, Cell[] nearCells, Cell[] aroundCells, Game game, int visibleLevel, bool disabled = false)
	{
		Position = position;
		Inside = inside;
		foreach(Cell cell in nearCells)
		{
			VisibleLevelSignal += cell.OnVisibleLevelSignal;
		}
		foreach(Cell cell in aroundCells)
		{
			SweepAround += cell.OnSweep;
			Signed += cell.OnAroundSigned;
		}
		Boom += game.GameLose;
		Sweeped += game.OnOneSweeped;
		Signed += game.OnOneSigned;
		game.XrayLevelChanged += OnVisibleLevelChanged;

		this.visibleLevel = visibleLevel;

		if (!disabled)
			SwitchState(CellState.Block, true);
		if (Inside > 0 && Inside < 27)
			Sphere.Initialize(Inside);
	}

	public enum SignState
	{
		None = -1,
		Flag,
		Sign0,
		Sign1,
		Sign2,
	}
	public enum CellState
	{
		Block,
		Cube,
		Number,
		Null,
	}

	//Signal
	public void OnNumberSweep()
	{
		if (aroundSignedNumber == Inside && State == CellState.Number)
		{
			EmitSignal(nameof(SweepAround));
		}
	}
	public void OnSweep()
	{
        if ((State == CellState.Block || State == CellState.Cube) && Sign != SignState.Flag)
		{
			switch (Inside)
			{
				case 0:
					SwitchState(CellState.Null);
					EmitSignal(nameof(SweepAround));
					VisibleLevel = 0;
					break;
				case 27:
					EmitSignal(nameof(Boom));
					break;
				default:
					SwitchState(CellState.Number);
					VisibleLevel = 0;
					break;
			}
			EmitSignal(nameof(VisibleLevelSignal), VisibleLevel + 1);
			EmitSignal(nameof(Sweeped));
		}
	}
	public void OnSign(SignState signState)
	{
		Sign = signState;
	}
	public void OnVisibleLevelSignal(int level)
	{
		if (VisibleLevel > level)
		{
			VisibleLevel = level;
			EmitSignal(nameof(VisibleLevelSignal), VisibleLevel + 1);
		}
	}
	public void OnVisibleLevelChanged(int level)
	{
		if (level == 3) level = 4;
		watchLevel = level;
		if (VisibleLevel <= level)
		{
			if(State == CellState.Block)
			{
				SwitchState(CellState.Cube);
			}
		}
		else
		{
			if(State == CellState.Cube)
			{
				SwitchState(CellState.Block);
			}
		}
		if (VisibleLevel > level + 1)
			Visible = false;
		else
			Visible = true;
	}
	public void OnAroundSigned(bool signed)
	{
		if (signed)
		{
			aroundSignedNumber++;
		}
		else
		{
			aroundSignedNumber--;
		}
		Sphere.Highlighted = aroundSignedNumber == Inside;
	}
}
