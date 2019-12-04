using System.Collections.Generic;
using System;

public class Dungeon
{
    public static Map dungeon_map;

    public enum CellType
    {
        Empty,
        Wall,
        Way_Out,
        Way_In,
        Treasure,
        Monster,
    }
    public struct Percentage
    {
        public const int OneWay = 50;
        public const int TwoWay = 30;
        public const int TriWay = 10;
    }

    public class Coord
    {
        public int x, y;

        public Coord()
        {
            x = 0;
            y = 0;
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
        public void SetRand(int dim)
        {
            x = UnityEngine.Random.Range(0, dim);
            y = UnityEngine.Random.Range(0, dim);
        }
        public static bool operator ==(Coord a, Coord b)
        {
            return (a.x == b.x && a.y == b.y);
        }
        public static bool operator !=(Coord a, Coord b)
        {
            return !(a == b);
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
    public class Cell
    {
        public Coord pos;
        public CellType inside;
        public List<Cell> cell_list;

        public Cell()
        {
            inside = CellType.Wall;
            pos = new Coord(-1, -1);
            cell_list = new List<Cell>();
            for (int i = 0; i < 4; i++)
                cell_list.Add(null);
        }
    }
    public struct Map
    {
        private static List<List<Cell>> dungeon;
        private static Coord way_in, way_out, player;
        
        public Map(int dim)
        {
            InitDungeonDim(out dungeon, dim);
            int floor = CreateDungeon(ref dungeon, ref way_in, ref way_out);
            player = way_in;
            if (floor == 0)
                floor = 0;
        }
        public Cell this[int x, int y]
        {
            get
            {
                return dungeon[x][y];
            }
            set
            {
                dungeon[x][y] = value;
            }
        }
        public static bool CanMove(Options.MoveDirection dir)
        {
            switch (dir)
            {
                case Options.MoveDirection.Forward:
                    if (player.y + 1 == dungeon.Count)
                        return false;
                    return dungeon[player.x][player.y + 1].inside == CellType.Wall ? false : true;
                case Options.MoveDirection.Right:
                    if (player.x + 1 == dungeon.Count)
                        return false;
                    return dungeon[player.x + 1][player.y].inside == CellType.Wall ? false : true;
                case Options.MoveDirection.Down:
                    if (player.y == 0)
                        return false;
                    return dungeon[player.x][player.y - 1].inside == CellType.Wall ? false : true;
                case Options.MoveDirection.Left:
                    if (player.x == 0)
                        return false;
                    return dungeon[player.x - 1][player.y].inside == CellType.Wall ? false : true;
                default:
                    return false;
            }
        }
        public static void MovePlayer(Options.MoveDirection dir)
        {
            switch (dir)
            {
                case Options.MoveDirection.Forward:
                    player.y++;
                    break;
                case Options.MoveDirection.Right:
                    player.x++;
                    break;
                case Options.MoveDirection.Down:
                    player.y--;
                    break;
                case Options.MoveDirection.Left:
                    player.x--;
                    break;
                default:
                    break;
            }
        }

        private static void InitDungeonDim(out List<List<Cell>> dungeon, int dim)
        {
            dungeon = new List<List<Cell>>();
            for (int i = 0; i < dim; i++)
            {
                dungeon.Add(new List<Cell>());
                for (int j = 0; j < dim; j++)
                {
                    dungeon[i].Add(new Cell());
                    dungeon[i][j].pos = new Coord(i, j);
                }
            }

            for (int i = 0; i < dim - 1; i++)
                for (int j = 0; j < dim; j++)
                {
                    dungeon[i][j].cell_list[1]     = dungeon[i + 1][j];
                    dungeon[i + 1][j].cell_list[3] = dungeon[i][j];
                    dungeon[j][i].cell_list[2]     = dungeon[j][i + 1];
                    dungeon[j][i + 1].cell_list[0] = dungeon[j][i];
                }
        }
        private static int CreateDungeon(ref List<List<Cell>> dungeon, ref Coord way_in, ref Coord way_out)
        {
            int empty = 0, dim = dungeon.Count;
            bool reached = false;
            SetEntrances(out way_in, out way_out, dim);
            dungeon[way_in.x][way_in.y].inside = CellType.Way_In;
            dungeon[way_out.x][way_out.y].inside = CellType.Way_Out;

            Coord nearest = way_in;
            Queue<Coord> q = new Queue<Coord>();
            for (int i = 0; i < 4; i++)
                if (dungeon[way_in.x][way_in.y].cell_list[i] != null)
                {
                    if ((dungeon[way_in.x][way_in.y].cell_list[i]).inside == CellType.Wall)
                    {
                        dungeon[way_in.x][way_in.y].cell_list[i].inside = CellType.Empty;
                        empty++;
                    }
                    q.Enqueue(dungeon[way_in.x][way_in.y].cell_list[i].pos);
                }

            while (!(q.Count == 0 && reached))
            {
                if (q.Count == 0 && !reached)
                    q.Enqueue(nearest);

                //отвечает клетки, в которых мы еще не были
                List<Coord> into = new List<Coord>();
                Coord c = q.Peek();

                //проверим, дошли ли мы до выхода из подземелья
                nearest = GetNearby(ref way_out, ref c, ref nearest);
                if (c == way_out)
                    reached = true;
                q.Dequeue();

                //у нас есть клетка, и до трех не занятых выходов из нее. Узнаем точно, сколько у нас проходов
                for (int i = 0; i < 4; i++)
                {
                    if (dungeon[c.x][c.y].cell_list[i] != null && dungeon[c.x][c.y].cell_list[i].inside != CellType.Empty && dungeon[c.x][c.y].cell_list[i].inside != CellType.Way_In)
                        into.Add(dungeon[c.x][c.y].cell_list[i].pos);
                }

                switch (into.Count)
                {
                    case 0: //у нас нет больше свободных клеток снаружи - тупик - выходим
                        continue;
                    case 1: //у нас целая одна клетка снаружи - идем в нее с вероятностью ONEWAY %
                        if (UnityEngine.Random.Range(0, 100) < Percentage.OneWay)
                        {
                            switch (dungeon[into[0].x][into[0].y].inside)
                            {
                                case CellType.Wall:
                                    {
                                        dungeon[into[0].x][into[0].y].inside = CellType.Empty;
                                        q.Enqueue(into[0]);
                                        break;
                                    }
                                case CellType.Way_Out:
                                    {
                                        q.Enqueue(into[0]);
                                        break;
                                    }
                            }
                            continue;
                        }
                        else
                            continue;
                    case 2: //снаружи целых две свободных клетки, если прокнет TWOWAY % то идем в обе стороны
                        if (UnityEngine.Random.Range(0, 100) < Percentage.TwoWay)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                switch (dungeon[into[i].x][into[i].y].inside)
                                {
                                    case CellType.Wall:
                                        {
                                            dungeon[into[i].x][into[i].y].inside = CellType.Empty;
                                            q.Enqueue(into[i]);
                                            break;
                                        }
                                    case CellType.Way_Out:
                                        {
                                            q.Enqueue(into[i]);
                                            break;
                                        }
                                }
                            }
                            continue;
                        }
                        else if (UnityEngine.Random.Range(0, 100) < Percentage.OneWay) //иначе см выше
                        {
                            int i = UnityEngine.Random.Range(0, 2);
                            switch (dungeon[into[i].x][into[i].y].inside)
                            {
                                case CellType.Wall:
                                    {
                                        dungeon[into[i].x][into[i].y].inside = CellType.Empty;
                                        q.Enqueue(into[i]);
                                        break;
                                    }
                                case CellType.Way_Out:
                                    {
                                        q.Enqueue(into[i]);
                                        break;
                                    }
                            }
                            continue;
                        }
                        else
                            continue;
                    case 3: //все дороги для нас, но только с проком в triway % мы идем во все
                        if (UnityEngine.Random.Range(0, 100) < Percentage.TriWay)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                switch (dungeon[into[i].x][into[i].y].inside)
                                {
                                    case CellType.Wall:
                                        {
                                            dungeon[into[i].x][into[i].y].inside = CellType.Empty;
                                            q.Enqueue(into[i]);
                                            break;
                                        }
                                    case CellType.Way_Out:
                                        {
                                            q.Enqueue(into[i]);
                                            break;
                                        }
                                }
                            }
                            continue;
                        }
                        else if (UnityEngine.Random.Range(0, 100) < Percentage.TwoWay) //иначе только в две
                        {
                            int j = UnityEngine.Random.Range(0, 3); //так ячейка, в которую мы НЕ пойдем
                            for (int i = 0; i < 3; i++)
                            {
                                if (i == j)
                                    continue;
                                switch (dungeon[into[i].x][into[i].y].inside)
                                {
                                    case CellType.Wall:
                                        {
                                            dungeon[into[i].x][into[i].y].inside = CellType.Empty;
                                            q.Enqueue(into[i]);
                                            break;
                                        }
                                    case CellType.Way_Out:
                                        {
                                            q.Enqueue(into[i]);
                                            break;
                                        }
                                }
                            }
                            continue;
                        }
                        else if (UnityEngine.Random.Range(0, 100) < Percentage.OneWay) //иначе в одну
                        {
                            int i = UnityEngine.Random.Range(0, 3); //так ячейка, в которую мы пойдем
                            switch (dungeon[into[i].x][into[i].y].inside)
                            {
                                case CellType.Wall:
                                    {
                                        dungeon[into[i].x][into[i].y].inside = CellType.Empty;
                                        q.Enqueue(into[i]);
                                        break;
                                    }
                                case CellType.Way_Out:
                                    {
                                        q.Enqueue(into[i]);
                                        break;
                                    }
                            }
                            continue;
                        }
                        else
                            continue;
                }
            }

            return empty;
        }
        private static void SetEntrances(out Coord way_in, out Coord way_out, int dim)
        {
            way_in = new Coord();
            way_out = new Coord();
            way_in.SetRand(dim);
            do
                way_out.SetRand(dim);
            while (way_in == way_out);
        }
        private static Coord GetNearby(ref Coord to, ref Coord from_1, ref Coord from_2)
        {
            return (CalcDistance(ref from_1, ref to) < CalcDistance(ref from_2, ref to) ? from_1 : from_2);
        }
        static int CalcDistance(ref Coord from, ref Coord to)
        {
            return (Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y));
        }
    }
}