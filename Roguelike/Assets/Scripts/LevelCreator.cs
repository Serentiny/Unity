using UnityEngine;

public class LevelCreator : MonoBehaviour
{
    public GameObject Player, CameraMapFull;
    public GameObject Cell_Empty, Cell_Block, Cell_Exit;

    void Start ()
    {
        Options.Init();
        CreateDungeon(Options.GetDungeonDim());
    }
    private void CreateDungeon(int dim)
    {
        // Создаем подземелье требуемой размерности и ставим туда героя
        Dungeon.AddStage(dim);
        Dungeon.SetPlayerToLastStage();

        // Создаем пол и стены в подземелье
        for (int i = -1; i <= dim; i++)
            for (int j = -1; j <= dim; j++)
                if (i == -1 || i == dim || j == -1 || j == dim)
                {
                    if (CheckDungeonWallNecessity(i, j, 0, dim - 1))
                        CreateObject(ref Cell_Block, i, j, "Block");
                }
                else
                {
                    switch (Dungeon.GetStage()[i, j].GetCellType())
                    {
                        case Dungeon.DungeonStage.CellType.Floor:
                            CreateObject(ref Cell_Empty, i, j, "Floor");
                            continue;
                        case Dungeon.DungeonStage.CellType.None:
                            if (CheckDungeonWallNecessity(i, j, 0, dim - 1))
                                CreateObject(ref Cell_Block, i, j, "Block");
                            continue;
                        case Dungeon.DungeonStage.CellType.Way_In:
                            CreateObject(ref Cell_Empty, i, j, "Floor");
                            CreateObject(ref Player, i, j, "Player", isCoordInName: false);
                            CreateObject(ref CameraMapFull, i, j, "CameraMapFull", isCoordInName: false);
                            continue;
                        case Dungeon.DungeonStage.CellType.Way_Out:
                            CreateObject(ref Cell_Exit, i, j, "Cell_Exit", isCoordInName: false);
                            continue;
                        default:
                            continue;
                    }
                }
    }
    private void CreateObject(ref GameObject gObject, int i, int j, string name, bool isCoordInName = true)
    {
        int step = Constants.DefaultTileSize;
        GameObject newObject = Instantiate(gObject, new Vector3(i * step, 0, j * step), Quaternion.identity);
        if (isCoordInName)
            newObject.name = string.Format("{0} ({1}; {2})", name, i.ToString(), j.ToString());        
        else
            newObject.name = name;
    }
    private bool CheckDungeonWallNecessity(int i, int j, int min, int max)
    {
        // Проверка на то, стоит ли создавать стену или рядом ничего интересного нет
        return (IsInRange(i + 1, min, max) && IsInRange(j, min, max)
             && Dungeon.GetStage()[i + 1, j].GetCellType() != Dungeon.DungeonStage.CellType.None
             || IsInRange(i, min, max) && IsInRange(j + 1, min, max)
             && Dungeon.GetStage()[i, j + 1].GetCellType() != Dungeon.DungeonStage.CellType.None
             || IsInRange(i - 1, min, max) && IsInRange(j, min, max)
             && Dungeon.GetStage()[i - 1, j].GetCellType() != Dungeon.DungeonStage.CellType.None
             || IsInRange(i, min, max) && IsInRange(j - 1, min, max)
             && Dungeon.GetStage()[i, j - 1].GetCellType() != Dungeon.DungeonStage.CellType.None);
    }
    private bool IsInRange(int i, int min, int max)
    {
        return min <= i && i <= max;
    }
}

