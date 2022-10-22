using Godot;
using Minesweeper3D.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper3D.Script;
internal class Grid<TCell>
{
    public static Vector3i SpawnSize { private get; set; }
    private static Grid<TCell> instance;
    public static void Spawn() => instance = new(SpawnSize);
    public static Grid<TCell> GetInstance()
    {
        return instance;
    }
    //public Grid(int x,int y,int z):this(new Vector3i(x, y, z)) { }
    private Grid(Vector3i size)
    {
        Size = size;
        Cells = new TCell[Size.x, Size.y, Size.z];
    }

    public TCell this[int x,int y,int z]
    {
        get => Cells[x, y, z];
        set => Cells[x, y, z] = value;
    }
    public TCell this[Vector3i index]
    {
        get => this[index.x, index.y, index.z];
        set => this[index.x, index.y, index.z] = value;
    }

    public Vector3i Size { get; }
    private TCell[,,] Cells;

    public bool IsInRange(Vector3i index) => IsInRange(index.x, index.y, index.z);
    public bool IsInRange(int x, int y, int z) =>
        (x < Size.x && x >= 0 && y < Size.y && y >= 0 && z < Size.z && z >= 0);

    public TCell[] GetAround(int x, int y, int z) => GetAround(new(x, y, z));
    public TCell[] GetAround(Vector3i center)
    {
        List<TCell> list = new();
        for(int i = -1;i <= 1; i++)
        {
            for(int j = -1;j <= 1; j++)
            {
                for(int k = -1;k <= 1; k++)
                {
                    Vector3i positionShift = new(i, j, k);
                    if(positionShift != Vector3i.Zero && IsInRange(center + positionShift))
                    {
                        list.Add(this[center + positionShift]);
                    }
                }
            }
        }
        return list.ToArray();
    }

    public TCell[] GetNear(int x, int y, int z) => GetNear(new(x, y, z));
    public TCell[] GetNear(Vector3i center)
    {
        Vector3i[] directions =
        {
            Vector3i.Up,
            Vector3i.Down,
            Vector3i.Left,
            Vector3i.Right,
            Vector3i.Forward,
            Vector3i.Back,
        };
        List<TCell> list = new();
        foreach(Vector3i direction in directions)
        {
            if (IsInRange(center + direction))
            {
                list.Add(this[center + direction]);
            }
        }
        return list.ToArray();
    }

    public List<TCell> GetCells()
    {
        List<TCell> list = new();
        foreach(TCell cell in Cells)
        {
            list.Add(cell);
        }
        return list;
    }
}
