using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SceneOptions : MonoBehaviour
{
    public InputField inputSize;
    public Text counterAvalanche, counterSand_1, counterSand_2, counterSand_3, counterSand_4;
    public Slider drawSpeedSlider;
    public Canvas mainViewCanvas, optionCanvas, gistoCanvas;
    public Toggle toggleSkipSand, toggleSkipAvalanche;
    public Button start, histogramm;
    public GameObject cell, gistoCollumn;
    public Camera view;
    
    float lastFrameTime;
    bool isStarted, pause, showSand, showAvalanche;
    float drawSpeed;

    static table _table;
    static int size, avalancheNo;
    static GameObject modelTable;
    static List<int> avalancheGisto;
    static List<float> weightCounter;
    static List<GameObject> modelGisto;
    Texture2D tableTexture;

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
        public void SetRand(int a, int b)
        {
            x = UnityEngine.Random.Range(0, a);
            y = UnityEngine.Random.Range(0, b);
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
    public class table
    {
        public List<List<int>> array;
        Queue<coord> queue;

        public table()
        {
            array = new List<List<int>>();
            queue = new Queue<coord>();
            for (int i = 0; i < size; i++)
            {
                array.Add(new List<int>());
                for (int j = 0; j < size; j++)
                {
                    array[i].Add(new int());
                    array[i][j] = 0;
                }
            }
        }
        public table(int a, int b)
        {
            array = new List<List<int>>();
            queue = new Queue<coord>();
            for (int i = 0; i < a; i++)
            {
                array.Add(new List<int>());
                for (int j = 0; j < b; j++)
                {
                    array[i].Add(new int());
                    array[i][j] = 0;
                }
            }
        }

        public int this[int x, int y]
        {
            get
            {
                return array[x][y];
            }
            set
            {
                array[x][y] = value;
            }
        }
        
        public void AddRandom()
        {
            coord cell = new coord();
            cell.SetRand(size, size);

            weightCounter[array[cell.x][cell.y]]--;
            array[cell.x][cell.y]++;
            weightCounter[array[cell.x][cell.y]]++;

            if (array[cell.x][cell.y] == 5)
                queue.Enqueue(new coord(cell.x, cell.y));
        }
        public void AddRandomMax()
        {
            AddRandom();
            if (!CanAvalanche())
                AddRandomMax();
        }
        public bool CanAvalanche()
        {
            return queue.Count != 0;
        }
        public void TryAvalanche()
        {
            int countAvalanche = 0;
            while (queue.Count != 0)
            {
                coord c = queue.Dequeue();
                array[c.x][c.y] -= 4;
                AddSandTo(c.x, c.y + 1);
                AddSandTo(c.x + 1, c.y);
                AddSandTo(c.x, c.y - 1);
                AddSandTo(c.x - 1, c.y);
                avalancheNo++;
                countAvalanche++;

                weightCounter[array[c.x][c.y]+4]--;
                weightCounter[array[c.x][c.y]  ]++;
            }
            while (avalancheGisto.Count < countAvalanche)
                avalancheGisto.Add(0);
            avalancheGisto[countAvalanche - 1]++;

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    if (array[i][j] >= 5)
                        queue.Enqueue(new coord(i, j));
        }
        public void TryAvalancheMax()
        {
            TryAvalanche();
            if (CanAvalanche())
                TryAvalancheMax();
        }
        void AddSandTo(int a, int b)
        {
            if (a < 0 || a >= size || b < 0 || b >= size)
                return;
            array[a][b]++;

            weightCounter[array[a][b]-1]--;
            weightCounter[array[a][b]  ]++;
        }

        public void Clear()
        {
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    array[i][j] = 0;
        }
        public void Full()
        {
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    array[i][j] = 10;
        }
        public int Size()
        {
            return array.Count;
        }
    }
//-------------------------------------------------------------------------------------------------------------------
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        pause = true;

        size = 25;
        inputSize.onEndEdit.AddListener(delegate { SizeChange(inputSize); });
        drawSpeed = 10f;
        drawSpeedSlider.onValueChanged.AddListener(delegate { DrawSpeedChange(); });
        showSand = true;
        showAvalanche = true;

        Init();
    }
    void Init()
    {
        isStarted = false;

        _table         = new table(size, size);
        modelGisto     = new List<GameObject>();
        avalancheGisto = new List<int>();
        weightCounter  = new List<float>();
        tableTexture   = new Texture2D(size, size);

        modelTable = (GameObject)Instantiate(cell, new Vector3(0, 0, 0), Quaternion.identity);
        modelTable.name = "SandPile";
        modelTable.GetComponent<Renderer>().material.mainTexture = tableTexture;

        avalancheGisto.Add(0);
        for (int i = 0; i < 10; i++)
            weightCounter.Add(0);
        Draw();
        lastFrameTime = Time.time;

        avalancheNo = 0;
        counterAvalanche.text = avalancheNo.ToString();
        counterSand_1.text = "00.00 %";
        counterSand_2.text = "00.00 %";
        counterSand_3.text = "00.00 %";
        counterSand_4.text = "00.00 %";
        drawSpeedSlider.value = drawSpeed;
        inputSize.text = size.ToString();

        mainViewCanvas.gameObject.SetActive(true);
        optionCanvas.gameObject.SetActive(false);
        gistoCanvas.gameObject.SetActive(false);
        start.gameObject.SetActive(true);
        histogramm.gameObject.SetActive(false);

        pause = false;
    }
    void ClearScreen()
    {
        Destroy(modelTable.gameObject);
        weightCounter.Clear();
    }

    void Update()
    {
        if (pause)
            return;

        if (isStarted)
        {
            if (_table.CanAvalanche())
            {
                if (!histogramm.gameObject.activeSelf)
                    histogramm.gameObject.SetActive(true);
                //Обрушили лавину
                if (showAvalanche && IsTimeToDraw())
                {
                    _table.TryAvalanche();
                    Redraw();
                    lastFrameTime = Time.time;
                }
                if (!showAvalanche)
                    _table.TryAvalancheMax();
            }
            else
            {
                //Добавили песчинку
                if (showSand && IsTimeToDraw())
                {
                    _table.AddRandom();
                    Redraw();
                    lastFrameTime = Time.time;
                }
                if (!showSand)
                    _table.AddRandomMax();
            }
            counterAvalanche.text = avalancheNo.ToString();
            
            counterSand_1.text = (weightCounter[1] / size / size * 100).ToString("00.00") + " %";
            counterSand_2.text = (weightCounter[2] / size / size * 100).ToString("00.00") + " %";
            counterSand_3.text = (weightCounter[3] / size / size * 100).ToString("00.00") + " %";
            counterSand_4.text = (weightCounter[4] / size / size * 100).ToString("00.00") + " %";

        }
    }
//-------------------------------------------------------------------------------------------------------------------
    void Draw()
    {
        for (int i = 0; i < _table.Size(); i++)
            for (int j = 0; j < _table.Size(); j++)
                tableTexture.SetPixel(i, j, Color.green);
        tableTexture.Apply();
        SetCameraToTable();
    }
    void Redraw()
    {
        for (int i = 0; i < _table.array.Count; i++)
            for (int j = 0; j < _table.array[i].Count; j++)
                switch (_table[i, j])
                {
                    case 0:
                        tableTexture.SetPixel(j, i, Color.green);
                        break;
                    case 1:
                        tableTexture.SetPixel(j, i, Color.yellow);
                        break;
                    case 2:
                        tableTexture.SetPixel(j, i, Color.Lerp(Color.yellow, Color.red, 0.125f));
                        break;
                    case 3:
                        tableTexture.SetPixel(j, i, Color.Lerp(Color.yellow, Color.red, 0.25f));
                        break;
                    case 4:
                        tableTexture.SetPixel(j, i, Color.Lerp(Color.yellow, Color.red, 0.375f));
                        break;
                    default:
                        tableTexture.SetPixel(j, i, Color.Lerp(Color.yellow, Color.red, 0.5f));
                        break;
                }
        tableTexture.Apply();
    }
    void DrawGisto()
    {
        for (int i = 0; i < avalancheGisto.Count; i++)
        {
            GameObject var = Instantiate(gistoCollumn);
            GameObject varLn = Instantiate(gistoCollumn);
            var.transform.SetParent(gistoCanvas.gameObject.transform);
            varLn.transform.SetParent(gistoCanvas.gameObject.transform);
            var.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            varLn.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            float width = 1000f / avalancheGisto.Count;
            float heigth = 250f / avalancheGisto[0] * avalancheGisto[i];
            float heigthLn = 250f / Mathf.Log(avalancheGisto[0] + 1) * Mathf.Log(avalancheGisto[i] + 1);

            var.GetComponent<RectTransform>().localPosition = new Vector3(-500f + (width * i) + (width / 2), heigth / 2, 0f);
            var.GetComponent<RectTransform>().sizeDelta = new Vector2(width, heigth);
            varLn.GetComponent<RectTransform>().localPosition = new Vector3(-500f + (width * i) + (width / 2), -heigthLn / 2, 0f);
            varLn.GetComponent<RectTransform>().sizeDelta = new Vector2(width, heigthLn);

            modelGisto.Add(var);
            modelGisto.Add(varLn);
        }
    }
    void ClearGisto()
    {
        for (int i = 0; i < modelGisto.Count; i++)
            Destroy(modelGisto[i].gameObject);
        modelGisto.Clear();
    }
    bool IsTimeToDraw()
    {
        return Time.time - lastFrameTime > 1 / drawSpeed;
    }
    void SetCameraToTable()
    {
        view.transform.position = new Vector3(0, 0, -10);
        view.orthographicSize = 0.7f;
    }

    void DrawSpeedChange()
    {
        drawSpeed = drawSpeedSlider.value;
    }
    void SizeChange(InputField input)
    {
        try
        {
            int tmp = int.Parse(input.text);
            if (tmp < 3)
                size = 3;
            else
                size = tmp;
        }
        catch (System.FormatException)
        {
            input.text = size.ToString();
        }
    }

    public void StartModeling()
    {
        isStarted = true;
        start.gameObject.SetActive(false);
    }
    public void ToggleShowSand()
    {
        showSand = toggleSkipSand.isOn;
    }
    public void ToggleShowAvalanche()
    {
        showAvalanche = toggleSkipAvalanche.isOn;
    }
    public void OptionTurnOn()
    {
        pause = true;
        mainViewCanvas.gameObject.SetActive(false);
        optionCanvas.gameObject.SetActive(true);
    }
    public void OptionTurnOff()
    {
        optionCanvas.gameObject.SetActive(false);
        mainViewCanvas.gameObject.SetActive(true);
        pause = false;
    }
    public void GistoTurnOn()
    {
        pause = true;
        mainViewCanvas.gameObject.SetActive(false);
        gistoCanvas.gameObject.SetActive(true);
        DrawGisto();
    }
    public void GistoTurnOff()
    {
        ClearGisto();
        gistoCanvas.gameObject.SetActive(false);
        mainViewCanvas.gameObject.SetActive(true);
        pause = false;
    }
    public void CreateNewTable()
    {
        pause = true;
        ClearScreen();
        Init();
    }
    public void Quit()
    {
        Application.Quit();
    }
}
