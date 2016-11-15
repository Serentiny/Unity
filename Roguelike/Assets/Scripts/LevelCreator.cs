using UnityEngine;
using System.Collections;

public class LevelCreator : MonoBehaviour
{
    int dim, col_delta;

    public GameObject Player;
    public GameObject WayPoint;
    public GameObject RotatePoint;
    public GameObject Column;
    public GameObject Cell_Empty;
    public GameObject Cell_Wall;
    public GameObject Cell_Exit;

    void Start ()
    {
        dim = 20;
        int step = SceneOptions.GetTile();
        col_delta = SceneOptions.GetTile() / 2;
        Dungeon.dungeon_map = new Dungeon.map(dim);

        for (int i = col_delta; i < dim * SceneOptions.GetTile(); i += step)
            for (int j = col_delta; j < dim * step; j += step)
                Instantiate(Column, new Vector3(i, 0, j), Quaternion.identity);

        for (int i = 0; i < dim * step - col_delta; i += step)
            for (int j = 0; j < dim * step - col_delta; j += step)
                switch (Dungeon.dungeon_map[i/step,j/step].inside)
                {
                    case Dungeon.Cell.FLOOR:
                        Instantiate(Cell_Empty, new Vector3(i, 0, j), Quaternion.identity);
                        continue;
                    case Dungeon.Cell.WALL:
                        Instantiate(Cell_Wall, new Vector3(i, 0, j), Quaternion.identity);
                        continue;
                    case Dungeon.Cell.WAY_IN:
                        Instantiate(Cell_Empty,  new Vector3(i,        0, j), Quaternion.identity);
                        Instantiate(Player,      new Vector3(i,        0, j), Quaternion.identity);
                        Instantiate(WayPoint,    new Vector3(i,        0, j), Quaternion.identity);
                        Instantiate(RotatePoint, new Vector3(i + step, 0, j), Quaternion.identity);
                        continue;
                    case Dungeon.Cell.WAY_OUT:
                        Instantiate(Cell_Exit, new Vector3(i, 0, j), Quaternion.identity);
                        continue;
                    default:
                        continue;
                }

        for (int i = -step; i < dim * step + step - col_delta; i += step)
            for (int j = -step; j < dim * step + step - col_delta; j += step)
                if (i == -step || i == dim * step || j == -step || j == dim * step)
                    Instantiate(Cell_Wall, new Vector3(i, 0, j), Quaternion.identity);
    }
	
	void Update ()
    {
        NextDungeon();
    }

    public static void NextDungeon()
    {
        if (GameObject.Find("Player(Clone)").transform.position.x < GameObject.Find("Cell_Exit(Clone)").transform.position.x + 1 &&
            GameObject.Find("Player(Clone)").transform.position.x > GameObject.Find("Cell_Exit(Clone)").transform.position.x - 1 &&
            GameObject.Find("Player(Clone)").transform.position.z < GameObject.Find("Cell_Exit(Clone)").transform.position.z + 1 &&
            GameObject.Find("Player(Clone)").transform.position.z > GameObject.Find("Cell_Exit(Clone)").transform.position.z - 1)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Dungeon");
        }
    }
}

