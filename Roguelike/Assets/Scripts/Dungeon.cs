using UnityEngine;
using System.Collections.Generic;
using System;

public class Dungeon : MonoBehaviour
{
    public static map dungeon_map;

    public struct Cell
    {
        public const int FLOOR = 0;
        public const int WALL = 1;
        public const int WAY_OUT = 2;
        public const int WAY_IN = 3;
        public const int TREASURE = 4;
        public const int MONSTER = 5;
    }
    public struct Percentage
    {
        public const int OneWay = 50;
        public const int TwoWay = 30;
        public const int TriWay = 10;
    }
    public struct Direction
    {
        public const int MOVE_UP    = 0;
        public const int MOVE_RIGHT = 1;
        public const int MOVE_DOWN  = 2;
        public const int MOVE_LEFT  = 3;
    }

    public class coord
    {
        public int x, y;

        public coord()
        {
            x = 0;
            y = 0;
        }
        public coord(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
        public coord(coord c)
        {
            x = c.x;
            y = c.y;
        }
        public void SetRand(int dim)
        {
            x = UnityEngine.Random.Range(0, dim);
            y = UnityEngine.Random.Range(0, dim);
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            coord a = (coord)obj;
            return (x == a.x && y == a.y);
        }
        public override int GetHashCode()
        {
            return x * 387 ^ y;
        }
        public static bool operator ==(coord a, coord b)
        {
            return (a.x == b.x && a.y == b.y);
        }
        public static bool operator !=(coord a, coord b)
        {
            return !(a == b);
        }
    }
    public class cell
    {
        public coord pos;
        public int inside;
        public List<cell> cell_list;

        public cell()
        {
            inside = Cell.WALL;
            pos = new coord(-1, -1);
            cell_list = new List<cell>();
            for (int i = 0; i < 4; i++)
                cell_list.Add(null);
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            cell a = (cell)obj;
            return (inside == a.inside);
        }
        public override int GetHashCode()
        {
            return (inside * 387 ^ pos.x) * 387 ^ pos.y;
        }
    }
    public class map
    {
        public static List<List<cell>> dungeon;
        public static coord way_in, way_out, player;

        public map()
        { }
        public map(int dim)
        {
            InitDungeonDim(out dungeon, dim);
            int floor = CreateDungeon(ref dungeon, ref way_in, ref way_out);
            player = way_in;
            if (floor == 0)
                floor = 0;
        }
        public cell this[int x, int y]
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
        public static bool canMove(int dir)
        {
            switch (dir)
            {
                case 0:
                    if (player.y + 1 == dungeon.Count)
                        return false;
                    return dungeon[player.x][player.y + 1].inside == Cell.WALL ? false : true;
                case 1:
                case -3:
                    if (player.x + 1 == dungeon.Count)
                        return false;
                    return dungeon[player.x + 1][player.y].inside == Cell.WALL ? false : true;
                case 2:
                case -2:
                    if (player.y == 0)
                        return false;
                    return dungeon[player.x][player.y - 1].inside == Cell.WALL ? false : true;
                case 3:
                case -1:
                    if (player.x == 0)
                        return false;
                    return dungeon[player.x - 1][player.y].inside == Cell.WALL ? false : true;
                default:
                    return false;
            }
        }
        public static void movePlayer(int dir)
        {
            switch (dir)
            {
                case 0:
                    player.y++;
                    break;
                case 1:
                case -3:
                    player.x++;
                    break;
                case 2:
                case -2:
                    player.y--;
                    break;
                case 3:
                case -1:
                    player.x--;
                    break;
                default:
                    break;
            }
        }
    }

    static void InitDungeonDim(out List<List<cell>> dungeon, int dim)
    {
        dungeon = new List<List<cell>>();
        for (int i = 0; i < dim; i++)
        {
            dungeon.Add(new List<cell>());
            for (int j = 0; j < dim; j++)
            {
                dungeon[i].Add(new cell());
                dungeon[i][j].pos = new coord(i, j);
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

    static int CreateDungeon(ref List<List<cell>> dungeon, ref coord way_in, ref coord way_out)
    {
        int empty = 0, dim = dungeon.Count;
        bool reached = false;
        SetEntrances(out way_in, out way_out, dim);
        dungeon[way_in.x][way_in.y].inside = Cell.WAY_IN;
        dungeon[way_out.x][way_out.y].inside = Cell.WAY_OUT;

        coord nearest = way_in;
        Queue<coord> q = new Queue<coord>();
        for (int i = 0; i < 4; i++)
            if (dungeon[way_in.x][way_in.y].cell_list[i] != null)
            {
                if ((dungeon[way_in.x][way_in.y].cell_list[i]).inside == Cell.WALL)
                {
                    dungeon[way_in.x][way_in.y].cell_list[i].inside = Cell.FLOOR;
                    empty++;
                }
                q.Enqueue(dungeon[way_in.x][way_in.y].cell_list[i].pos);
            }

        while (!(q.Count == 0 && reached))
        {
            if (q.Count == 0 && !reached)
                q.Enqueue(nearest);

            //отвечает клетки, в которых мы еще не были
            List<coord> into = new List<coord>();
            coord c = q.Peek();

            //проверим, дошли ли мы до выхода из подземелья
            nearest = nearby(ref way_out, ref c, ref nearest);
            if (c == way_out)
                reached = true;
            q.Dequeue();

            //у нас есть клетка, и до трех не занятых выходов из нее. Узнаем точно, сколько у нас проходов
            for (int i = 0; i < 4; i++)
            {
                if (dungeon[c.x][c.y].cell_list[i] != null && dungeon[c.x][c.y].cell_list[i].inside != Cell.FLOOR && dungeon[c.x][c.y].cell_list[i].inside != Cell.WAY_IN)
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
                            case Cell.WALL:
                                {
                                    dungeon[into[0].x][into[0].y].inside = Cell.FLOOR;
                                    q.Enqueue(into[0]);
                                    break;
                                }
                            case Cell.WAY_OUT:
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
                                case Cell.WALL:
                                    {
                                        dungeon[into[i].x][into[i].y].inside = Cell.FLOOR;
                                        q.Enqueue(into[i]);
                                        break;
                                    }
                                case Cell.WAY_OUT:
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
                            case Cell.WALL:
                                {
                                    dungeon[into[i].x][into[i].y].inside = Cell.FLOOR;
                                    q.Enqueue(into[i]);
                                    break;
                                }
                            case Cell.WAY_OUT:
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
                                case Cell.WALL:
                                    {
                                        dungeon[into[i].x][into[i].y].inside = Cell.FLOOR;
                                        q.Enqueue(into[i]);
                                        break;
                                    }
                                case Cell.WAY_OUT:
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
                                case Cell.WALL:
                                    {
                                        dungeon[into[i].x][into[i].y].inside = Cell.FLOOR;
                                        q.Enqueue(into[i]);
                                        break;
                                    }
                                case Cell.WAY_OUT:
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
                            case Cell.WALL:
                                {
                                    dungeon[into[i].x][into[i].y].inside = Cell.FLOOR;
                                    q.Enqueue(into[i]);
                                    break;
                                }
                            case Cell.WAY_OUT:
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

    static void SetEntrances(out coord way_in, out coord way_out, int dim)
    {
        way_in = new coord();
        way_out = new coord();
        way_in.SetRand(dim);
        do
            way_out.SetRand(dim);
        while (way_in == way_out);
    }

    static coord nearby(ref coord to, ref coord from_1, ref coord from_2)
    {
        return (distance(ref from_1, ref to) < distance(ref from_2, ref to) ? from_1 : from_2);
    }

    static int distance(ref coord from, ref coord to)
    {
        return (Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y));
    }

    void Start()
    {

    }

    void Update()
    {

    }
}