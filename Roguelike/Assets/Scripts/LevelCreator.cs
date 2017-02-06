using UnityEngine;
using System.Collections;

public class LevelCreator : MonoBehaviour
{
    int dim, col_delta;

    public GameObject Player;
    public GameObject WayPoint;
    public GameObject RotatePoint;
    public GameObject Cell_Empty;
    public GameObject Cell_Wall;
    public GameObject Cell_Exit;

    void Start ()
    {
        dim = 20;
        int step = SceneOptions.GetTile();
        col_delta = SceneOptions.GetTile() / 2;
        Dungeon.dungeon_map = new Dungeon.map(dim);
        for (int i = 0; i < dim * step - col_delta; i += step)
            for (int j = 0; j < dim * step - col_delta; j += step)
                switch (Dungeon.dungeon_map[i/step,j/step].inside)
                {
                    case Dungeon.Cell.FLOOR:
                        GameObject floor = (GameObject)Instantiate(Cell_Empty, new Vector3(i, 0, j), Quaternion.identity);
                        floor.name = "Floor (" + (i / step).ToString() + "; " + (j / step).ToString() + ")";
                        continue;
                    case Dungeon.Cell.WALL:
                        GameObject wall = (GameObject)Instantiate(Cell_Wall, new Vector3(i, 0, j), Quaternion.identity);
                        wall.name = "Cell_Wall (" + (i / step).ToString() + "_" + (j / step).ToString() + ")";
                        continue;
                    case Dungeon.Cell.WAY_IN:
                        GameObject start = (GameObject)Instantiate(Cell_Empty,  new Vector3(i, 0, j), Quaternion.identity);
                        start.name = "Floor (" + (i / step).ToString() + "; " + (j / step).ToString() + ")";
                        GameObject player = (GameObject)Instantiate(Player,      new Vector3(i, 0, j), Quaternion.identity);
                        player.name = "Player";
                        GameObject waypoint = (GameObject)Instantiate(WayPoint,    new Vector3(i, 0, j), Quaternion.identity);
                        waypoint.name = "WayPoint";
                        GameObject rotpoint = (GameObject)Instantiate(RotatePoint, new Vector3(i + step, 0, j), Quaternion.identity);
                        rotpoint.name = "RotatePoint";
                        continue;
                    case Dungeon.Cell.WAY_OUT:
                        GameObject exit = (GameObject)Instantiate(Cell_Exit, new Vector3(i, 0, j), Quaternion.identity);
                        exit.name = "Cell_Exit";
                        continue;
                    default:
                        continue;
                }

        for (int i = -step; i < dim * step + step - col_delta; i += step)
            for (int j = -step; j < dim * step + step - col_delta; j += step)
                if (i == -step || i == dim * step || j == -step || j == dim * step)
                {
                    GameObject wall = (GameObject)Instantiate(Cell_Wall, new Vector3(i, 0, j), Quaternion.identity);
                    wall.name = "Wall (" + i.ToString() + "_" + j.ToString() + ")";

                }
    }
	
	void Update ()
    {
        NextDungeon();
    }

    public static void NextDungeon()
    {
        if (GameObject.Find("Player").transform.position.x < GameObject.Find("Cell_Exit").transform.position.x + 1 &&
            GameObject.Find("Player").transform.position.x > GameObject.Find("Cell_Exit").transform.position.x - 1 &&
            GameObject.Find("Player").transform.position.z < GameObject.Find("Cell_Exit").transform.position.z + 1 &&
            GameObject.Find("Player").transform.position.z > GameObject.Find("Cell_Exit").transform.position.z - 1)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Dungeon");
        }
    }
}

