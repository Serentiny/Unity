using System.Collections.Generic;
using System;
using UnityEngine;


public static class Constants
{
    public const int DefaultTileSize = 10;
    public const int DefaultDungeonDim = 10;

    public const int OneWay = 75;
    public const int TwoWay = 35;
    public const int TriWay = 10;
}

public class Dungeon
{
    public static List<DungeonStage> dungeon;
    public static Player player;

    static Dungeon()
    {
        dungeon = new List<DungeonStage>();
        player = new Player();
    }

    public static void AddStage(int dim)
    {
        dungeon.Add(new DungeonStage(dim));
    }
    public static DungeonStage GetStage()
    {
        return GetStage(player.GetDungeonStage());
    }
    public static DungeonStage GetStage(int stageNo)
    {
        if (0 <= stageNo && stageNo < dungeon.Count)
            return dungeon[stageNo];
        throw new Exception(string.Format("Out of bound exception: stageNo = {0} is not in [0, {1})", stageNo, dungeon.Count));
    }
    public static void SetPlayerToLastStage()
    {
        player.SetDungeonStage(dungeon.Count - 1);
    }

    public static bool CanMovePlayer(Options.MoveDirection dir)
    {
        return GetStage()[player.GetX(), player.GetY()].GetWall(dir).CanPass(dir);
    }
    public static void MovePlayer(Options.MoveDirection dir)
    {
        switch (dir)
        {
            case Options.MoveDirection.Forward:
                player.MoveUp();
                break;
            case Options.MoveDirection.Right:
                player.MoveRight();
                break;
            case Options.MoveDirection.Down:
                player.MoveDown();
                break;
            case Options.MoveDirection.Left:
                player.MoveLeft();
                break;
            default:
                break;
        }
    }

    public class Coord
    {
        public int x, y;

        public Coord()
        {
            x = 0;
            y = 0;
        }
        public Coord(int dim)
        {
            x = UnityEngine.Random.Range(0, dim);
            y = UnityEngine.Random.Range(0, dim);
        }
        public Coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
        public Coord(Coord c)
        {
            x = c.x;
            y = c.y;
        }
        public Vector2 GetVector2()
        {
            return new Vector2(x, y);
        }
        public Vector3 GetVector3()
        {
            return new Vector3(x, 0, y);
        }
        public Coord UpdateByDir(Options.MoveDirection dir)
        {
            switch (dir)
            {
                case Options.MoveDirection.Forward:
                    return new Coord(x, y + 1);
                case Options.MoveDirection.Right:
                    return new Coord(x + 1, y);
                case Options.MoveDirection.Down:
                    return new Coord(x, y - 1);
                case Options.MoveDirection.Left:
                    return new Coord(x - 1, y);
                default:
                    return this;
            }
        }
        public bool IsInBound(int from, int to)
        {
            return from <= x && x < to
                && from <= y && y < to;
        }

        public static bool operator ==(Coord a, Coord b)
        {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator !=(Coord a, Coord b)
        {
            return !(a == b);
        }
        public static bool operator <(Coord a, Coord b)
        {
            return a.x <  b.x
                || a.x == b.x && a.y < b.y;
        }
        public static bool operator >(Coord a, Coord b)
        {
            return a.x > b.x
                || a.x == b.x && a.y > b.y;
        }
        public static Coord operator +(Coord a, Coord b)
        {
            return new Coord(a.x + b.x, a.y + b.y);
        }
        public static Coord operator -(Coord a, Coord b)
        {
            return new Coord(a.x - b.x, a.y - b.y);
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Coord a = (Coord)obj;
            return (x == a.x && y == a.y);
        }
        public override int GetHashCode()
        {
            return x * 387 ^ y;
        }
    }
    public class Player
    {
        private Coord pos;
        private int dungeonStage;

        public Player()
        {
            pos = new Coord();
            dungeonStage = -1;
        }

        public void SetPosition(Coord pos)
        {
            this.pos = pos;
        }
        public int GetDungeonStage()
        {
            return dungeonStage;
        }
        public void SetDungeonStage(int stageNo)
        {
            dungeonStage = stageNo;
        }

        public Coord GetPos()
        {
            return pos;
        }
        public int GetX()
        {
            return pos.x;
        }
        public int GetY()
        {
            return pos.y;
        }
        public void MoveUp()
        {
            pos.y++;
        }
        public void MoveRight()
        {
            pos.x++;
        }
        public void MoveDown()
        {
            pos.y--;
        }
        public void MoveLeft()
        {
            pos.x--;
        }
    }
    public class DungeonStage
    {
        private List<List<Cell>> stage;
        private Coord way_in = null, way_out = null;

        public enum CellType
        {
            None,
            Floor,
            Way_In,
            Way_Out,
            Treasure,
            Monster,
        }

        public DungeonStage(int dim = Constants.DefaultDungeonDim)
        {
            // Инициализируем уровень подземелья
            stage = new List<List<Cell>>();
            for (int x = 0; x < dim; x++)
            {
                stage.Add(new List<Cell>());
                for (int y = 0; y < dim; y++)
                    stage[x].Add(new Cell(x, y));
            }

            // Еще раз пробегаемся по готовому массиву, и собираем стены
            for (int x = 0; x < dim; x++)
            {
                for (int y = 0; y < dim; y++)
                {
                    Coord curPos = new Coord(x, y);
                    for (Options.MoveDirection dir = 0; dir < Options.MoveDirection.Size; dir++)
                    {
                        Coord neighbour = curPos.UpdateByDir(dir);
                        if (neighbour.IsInBound(0, stage.Count))
                        {
                            Wall wall = stage[curPos.x][curPos.y].GetWall(dir);
                            if (wall == null)
                            {
                                wall = new Wall(curPos, neighbour);
                                int otherDir = ((int) dir + 2) % (int) Options.MoveDirection.Size;
                                stage[neighbour.x][neighbour.y].SetWall((Options.MoveDirection) otherDir, ref wall);
                            }
                            stage[x][y].SetWall(dir, ref wall);
                        }
                        else
                        {
                            Wall wall = new Wall(curPos, neighbour);
                            stage[x][y].SetWall(dir, ref wall);
                        }
                    }
                }
            }

            // Заполняем подземелье
            SetEntrances();
            CreateDungeon();
        }
        private void SetEntrances()
        {
            way_in = new Coord(stage.Count);
            do
                way_out = new Coord(stage.Count);
            while (way_in == way_out);
            player.SetPosition(way_in);
        }
        private void CreateDungeon()
        {
            bool reached = false;
            stage[way_in.x][way_in.y].SetCellType(CellType.Way_In);
            stage[way_out.x][way_out.y].SetCellType(CellType.Way_Out);

            // Клетка, самая близкая к выходу
            Coord nearest = way_in;
            Queue<Coord> q = new Queue<Coord>();

            // Все 4 клетки вокруг старта (если он не на краю) перекрашиваем в пол и суем в очередь
            for (Options.MoveDirection dir = 0; dir < Options.MoveDirection.Size; dir++)
            {
                Coord neighbour = way_in.UpdateByDir(dir);
                if (neighbour.IsInBound(0, stage.Count))
                {
                    stage[way_in.x][way_in.y].GetWall(dir).SetPass(true);
                    if (stage[neighbour.x][neighbour.y].GetCellType() == CellType.None)
                    {
                        stage[neighbour.x][neighbour.y].SetCellType(CellType.Floor);
                        q.Enqueue(neighbour);
                    }
                }
            }

            // Создаем подземелье до тех пор, пока не дойдем до выхода
            while (!(q.Count == 0 && reached))
            {
                // Если очередь пуста, а мы не дошли до выхода, засовываем в очередь ближайшую клетку к выходу
                if (q.Count == 0 && !reached)
                    q.Enqueue(nearest);

                Coord curCoord = q.Dequeue();
                // Проверим, дошли ли мы до выхода из подземелья
                nearest = GetNearby(way_out, curCoord, nearest);
                if (!reached && curCoord == way_out)
                    reached = true;

                // У нас есть до трех не занятых выходов, засовываем их в freeWays
                List<Options.MoveDirection> freeDirs = new List<Options.MoveDirection>();
                for (Options.MoveDirection dir = 0; dir < Options.MoveDirection.Size; dir++)
                {
                    Coord neighbour = curCoord.UpdateByDir(dir);
                    if (neighbour.IsInBound(0, stage.Count)
                     && stage[neighbour.x][neighbour.y].GetCellType() != CellType.Floor
                     && stage[neighbour.x][neighbour.y].GetCellType() != CellType.Way_In)
                        freeDirs.Add(dir);
                }

                // В зависимости от количества доступных клеток, зависит вероятность расщепления путей
                int rand = UnityEngine.Random.Range(0, 100);
                if (freeDirs.Count >= 3 && rand < Constants.TriWay)
                {
                    // Все дороги для нас
                    for (int i = 0; i < freeDirs.Count; i++)
                    {
                        UpdateFreeCell(ref q, curCoord.UpdateByDir(freeDirs[i]));
                        stage[curCoord.x][curCoord.y].GetWall(freeDirs[i]).SetPass(true);
                    }
                }
                else if (freeDirs.Count >= 2 && rand < Constants.TwoWay)
                {
                    // Выберем ту ячейку, в которую мы НЕ пойдем
                    int j = UnityEngine.Random.Range(0, freeDirs.Count);
                    for (int i = 0; i < freeDirs.Count; i++)
                    {
                        if (i == j)
                            continue;
                        UpdateFreeCell(ref q, curCoord.UpdateByDir(freeDirs[i]));
                        stage[curCoord.x][curCoord.y].GetWall(freeDirs[i]).SetPass(true);
                    }
                }
                else if (freeDirs.Count >= 1 && rand < Constants.OneWay)
                {
                    // Выберем ту ячейку, в которую мы пойдем
                    int i = UnityEngine.Random.Range(0, freeDirs.Count);
                    UpdateFreeCell(ref q, curCoord.UpdateByDir(freeDirs[i]));
                    stage[curCoord.x][curCoord.y].GetWall(freeDirs[i]).SetPass(true);
                }
            }
        }
        private void UpdateFreeCell(ref Queue<Coord> q, Coord c)
        {
            switch (stage[c.x][c.y].GetCellType())
            {
                case CellType.None:
                    {
                        stage[c.x][c.y].SetCellType(CellType.Floor);
                        q.Enqueue(c);
                        break;
                    }
                case CellType.Way_Out:
                    {
                        q.Enqueue(c);
                        break;
                    }
            }
        }
        private Coord GetNearby(Coord to, Coord from_1, Coord from_2)
        {
            return (StepDistance(from_1, to) < StepDistance(from_2, to) ? from_1 : from_2);
        }
        private static int StepDistance(Coord from, Coord to)
        {
            // Считаем расстояние не напрямую, а в шагах
            return (Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y));
        }

        public Cell this[int x, int y]
        {
            get
            {
                if (0 <= x && x < stage.Count && 0 <= y && y < stage.Count)
                    return stage[x][y];
                return null;
            }
            set
            {
                if (0 <= x && x < stage.Count && 0 <= y && y < stage.Count)
                    stage[x][y] = value;
            }
        }
        public Cell GetCellByDir(int x, int y, int dir)
        {
            switch (dir)
            {
                case (int)Options.MoveDirection.Up:
                    return stage[x][y + 1];
                case (int)Options.MoveDirection.Right:
                    return stage[x + 1][y];
                case (int)Options.MoveDirection.Down:
                    return stage[x][y - 1];
                case (int)Options.MoveDirection.Left:
                    return stage[x - 1][y];
                default:
                    return stage[x][y];
            }
        }
        public int GetDim()
        {
            return stage.Count;
        }

        public class Cell
        {
            private Coord pos;
            private CellType type;
            private List<Wall> wall_list;

            public Cell(int x, int y)
            {
                type = CellType.None;
                pos = new Coord(x, y);
                wall_list = new List<Wall>();
                for (Options.MoveDirection dir = 0; dir < Options.MoveDirection.Size; dir++)
                    wall_list.Add(null);
            }
            public void SetWall(Options.MoveDirection dir, ref Wall wall)
            {
                wall_list[(int)dir] = wall;
            }

            public Coord GetPos()
            {
                return pos;
            }
            public CellType GetCellType()
            {
                return type;
            }
            public void SetCellType(CellType type)
            {
                this.type = type;
            }
            public Wall GetWall(Options.MoveDirection dir)
            {
                if (0 <= (int)dir && (int)dir < (int)Options.MoveDirection.Size)
                    return wall_list[(int)dir];
                throw new Exception(string.Format("Out of bound exception: dir = {0} is not in [0, {1})", (int)dir, (int)Options.MoveDirection.Size));
            }
        }
        public class Wall
        {
            private Coord c1, c2;
            // Доступен ли проход с одной стороны в другую
            private bool open_c1c2 = false, open_c2c1 = false;
            
            public Wall(Coord c1, Coord c2)
            {
                if (StepDistance(c1, c2) != 1)
                    throw new Exception(string.Format("Wrong args: Not neighbour coords in Wall constructor"));
                if (c1 < c2)
                {
                    this.c1 = c1;
                    this.c2 = c2;
                }
                else
                {
                    this.c1 = c2;
                    this.c2 = c1;
                }
            }

            public void SetPass(Options.MoveDirection dir, bool open)
            {
                if (dir == Options.MoveDirection.Up || dir == Options.MoveDirection.Right)
                    open_c1c2 = open;
                else if (dir == Options.MoveDirection.Down || dir == Options.MoveDirection.Left)
                    open_c2c1 = open;
            }
            public void SetPass(bool open)
            {
                open_c1c2 = open;
                open_c2c1 = open;
            }
            public bool CanPass(Options.MoveDirection dir)
            {
                if (dir == Options.MoveDirection.Up || dir == Options.MoveDirection.Right)
                    return open_c1c2;
                else if (dir == Options.MoveDirection.Down || dir == Options.MoveDirection.Left)
                    return open_c2c1;
                else
                    return true;
            }
            public bool IsTwoSideOpen()
            {
                return open_c1c2 && open_c2c1;
            }
            public bool IsVertical()
            {
                return c1.y == c2.y;
            }
        }
    }
}

public class Options
{
    public enum DirectionAction
    {
        MoveDirection = 0,
        RotationDirection = 1,
    }
    public enum MoveDirection
    {
        Forward = 0,    // [w] or ( 0, 0,  1)
        Up      = 0,    // same
        Right   = 1,    // [d] or ( 1, 0,  0)
        Back    = 2,    // [s] or ( 0, 0, -1)
        Down    = 2,    // same
        Left    = 3,    // [a] or (-1, 0,  0)
        Size    = 4,
    }
    public enum RotationDirection
    {
        Left = -1,  // [q]
        Right = 1,  // [e]
    }
    public enum KeyAction
    {
        NoAction,

        MoveForward,    // [w]
        MoveRight,      // [d]
        MoveBack,       // [s]
        MoveLeft,       // [a]

        RotateRight,    // [e]
        RotateLeft,     // [q]
    }

    private static bool isInited = false;
    private static string path = null;
    private static int tileSize, dungeonDim, minimapDim;
    private static int player_mov_speed, player_rot_speed;
    
    public static void Init()
    {
        if (isInited)
            return;
        isInited = true;

        path = Application.persistentDataPath;
        // StreamReader sr = new StreamReader(string.Format("{0}/setting.txt", path), true);
        // JSON or XML read
        UpdateOptions();
    }
    private static void UpdateOptions()
    {
        tileSize = 10;
        dungeonDim = 20;
        minimapDim = 5;
        player_mov_speed = 25;
        player_rot_speed = 250;
    }  

    // Enum methods
    public static MoveDirection SumEnumMoveDirection(MoveDirection mDir1, MoveDirection mDir2)
    {
        int result = (int)mDir1 + (int)mDir2;
        return (MoveDirection)(result % (int)MoveDirection.Size);
    }
    public static MoveDirection RotateEnumMoveDirection(MoveDirection mDir, RotationDirection rDir)
    {
        int result = (int)mDir + (int)rDir;
        if (result < 0)
            result += (int)MoveDirection.Size;
        return (MoveDirection)(result % (int)MoveDirection.Size);
    }
    public static Vector3 EnumMoveDirectionToVector3(MoveDirection mDir)
    {
        switch (mDir)
        {
            case MoveDirection.Forward:
                return Vector3.forward;
            case MoveDirection.Right:
                return Vector3.right;
            case MoveDirection.Back:
                return Vector3.back;
            case MoveDirection.Left:
                return Vector3.left;
            default:
                return Vector3.zero;
        }
    }
    public static MoveDirection KeyActionToMoveDirection(KeyAction action)
    {
        switch (action)
        {
            case KeyAction.MoveForward:
                return MoveDirection.Forward;
            case KeyAction.MoveRight:
                return MoveDirection.Right;
            case KeyAction.MoveBack:
                return MoveDirection.Back;
            case KeyAction.MoveLeft:
                return MoveDirection.Left;
            default:
                return MoveDirection.Forward;
        }
    }
    public static RotationDirection KeyActionToRotationDirection(KeyAction action)
    {
        switch (action)
        {
            case KeyAction.RotateLeft:
                return RotationDirection.Left;
            case KeyAction.RotateRight:
                return RotationDirection.Right;
            default:
                return RotationDirection.Right;
        }
    }

    // Options getters
    public static int GetPlayerMoveSpeed()
    {
        return player_mov_speed;
    }
    public static int GetPlayerRotateSpeed()
    {
        return player_rot_speed;
    }
    public static int GetTileSize()
    {
        return tileSize;
    }
    public static int GetDungeonDim()
    {
        return dungeonDim;
    }
    public static int GetMinimapDim()
    {
        return minimapDim;
    }
}
