using UnityEngine;

public class LevelCreator : MonoBehaviour
{
    const int dim = 20;   // должно изменяться со стороны, а не константа

    public GameObject Player;
    public GameObject Cell_Empty;
    public GameObject Cell_Wall;
    public GameObject Cell_Exit;

    void Start ()
    {
        // Создаем подземелье требуемой размерности
        Dungeon.dungeon_map = new Dungeon.Map(dim);
        
        // Создаем пол и стены в подземелье
        int step = SceneOptions.GetTileLength();
        for (int i = -1; i <= dim; i++)
            for (int j = -1; j <= dim; j++)
                if (i == -1 || i == dim || j == -1 || j == dim)
                {
                    if (CheckDungeonWallNecessity(i, j, 0, dim - 1))
                        CreateObject(ref Cell_Wall, i, j, "Wall");
                }
                else
                {
                    switch (Dungeon.dungeon_map[i, j].inside)
                    {
                        case Dungeon.CellType.Empty:
                            CreateObject(ref Cell_Empty, i, j, "Floor");
                            continue;
                        case Dungeon.CellType.Wall:
                            if (CheckDungeonWallNecessity(i, j, 0, dim - 1))
                                CreateObject(ref Cell_Wall, i, j, "Wall");
                            continue;
                        case Dungeon.CellType.Way_In:
                            CreateObject(ref Cell_Empty, i, j, "Floor");
                            CreateObject(ref Player, i, j, "Player", isCoordInName: false);
                            continue;
                        case Dungeon.CellType.Way_Out:
                            CreateObject(ref Cell_Exit, i, j, "Cell_Exit", isCoordInName: false);
                            continue;
                        default:
                            continue;
                    }
                }
    }
    private static void CreateObject(ref GameObject gObject, int i, int j, string name, bool isCoordInName = true)
    {
        int step = SceneOptions.GetTileLength();
        GameObject newObject = Instantiate(gObject, new Vector3(i * step, 0, j * step), Quaternion.identity);
        if (isCoordInName)
            newObject.name = string.Format("{0} ({1}; {2})", name, i.ToString(), j.ToString());        
        else
            newObject.name = name;
    }
    private static bool CheckDungeonWallNecessity(int i, int j, int min, int max)
    {
        // Проверка на то, стоит ли создавать стену или рядом ничего интересного нет
        return (IsInRange(i + 1, min, max) && IsInRange(j, min, max) && Dungeon.dungeon_map[i + 1, j].inside != Dungeon.CellType.Wall ||
                IsInRange(i, min, max) && IsInRange(j + 1, min, max) && Dungeon.dungeon_map[i, j + 1].inside != Dungeon.CellType.Wall ||
                IsInRange(i - 1, min, max) && IsInRange(j, min, max) && Dungeon.dungeon_map[i - 1, j].inside != Dungeon.CellType.Wall ||
                IsInRange(i, min, max) && IsInRange(j - 1, min, max) && Dungeon.dungeon_map[i, j - 1].inside != Dungeon.CellType.Wall);
    }
    private static bool IsInRange(int i, int min, int max)
    {
        return min <= i && i <= max;
    }
	
	void Update ()
    {
        NextDungeon();
    }
    static void NextDungeon()
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

