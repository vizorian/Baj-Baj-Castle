using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public Sprite CellSprite;

    public bool allowChange = false;

    public static Texture2D texture_cell;
    public static Texture2D texture_cellBorder;
    public static GUIStyle style_cell;
    public static GUIStyle style_cellBorder;

    private List<Cell> Cells = new List<Cell>();
    private List<GameObject> CellObjects = new List<GameObject>();
    private List<GameObject> CellDisplayObjects = new List<GameObject>();

    public int TileSize = 16;
    public int Complexity = 50;
    public float Width = 200;
    public float Height = 10;
    private static int cellSize;

    public bool DisplayCells = false;

    private bool displaysCreated = false;

    // Start is called before the first frame update
    void Start()
    {
        cellSize = TileSize;
        CreateTextures();
        CreateCells(Complexity);
        CreateCellObjects();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            foreach (var cell in CellObjects)
            {
                cell.GetComponent<Rigidbody2D>().WakeUp();
            }
        }

        if (Input.GetKeyUp(KeyCode.D))
        {
            if (DisplayCells)
            {
                DisplayCells = false;
                foreach (var cell in CellDisplayObjects)
                {
                    cell.SetActive(false);
                }
            }
            else
            {
                DisplayCells = true;
            }
        }

        if (DisplayCells)
        {
            // Draw aligned cells
            if (!displaysCreated)
            {
                CreateCellDisplayObjects();
                displaysCreated = true;
            }

            for (int i = 0; i < CellDisplayObjects.Count; i++)
            {
                float x = RoundNumber(100f * CellObjects[i].transform.localPosition.x, cellSize);
                float y = RoundNumber(100f * CellObjects[i].transform.localPosition.y, cellSize);
                Vector2 position = new Vector2(0.01f * x, 0.01f * y);
                CellDisplayObjects[i].transform.localPosition = position;
                CellDisplayObjects[i].SetActive(true);
            }
        }

        if (allowChange)
        {
            foreach (var cell in CellObjects)
            {
            }

            //foreach (var cell in Cells)
            //{
            //    print(cell.ToString());
            //}
            //print("-----------------");

            //SeparateCells();
        }
    }

    private void CreateCellDisplayObjects()
    {
        for (int i = 0; i < Cells.Count; i++)
        {
            GameObject gameObject = new GameObject($"cell_{i}_display");
            gameObject.transform.localScale = new Vector2(0.01f * Cells[i].Width, 0.01f * Cells[i].Height);

            SpriteRenderer sprite = gameObject.AddComponent<SpriteRenderer>();
            sprite.sprite = CellSprite;
            sprite.color = new Color(0, 1, 1, 0.3f);

            gameObject.SetActive(false);

            CellDisplayObjects.Add(gameObject);
        }
    }



    //private bool CheckForOverlaps()
    //{
    //    for (int currentCell = 0; currentCell < Cells.Count / 2; currentCell++)
    //    {
    //        for (int nextCell = Cells.Count / 2; nextCell < Cells.Count; nextCell++)
    //        {
    //            if (Cells[currentCell].IsOverlapping(Cells[nextCell]))
    //            {
    //                return true;
    //            }
    //        }
    //    }

    //    return false;
    //}

    //void OnGUI()
    //{
    //    foreach (var cell in Cells)
    //    {
    //        GUI.Box(new Rect(Screen.width / 2 + cell.Position.x, Screen.height / 2 + cell.Position.y, cell.Width, cell.Height), GUIContent.none, style_cell);
    //    }
    //}

    //private void OnDrawGizmos()
    //{
    //    if(Cells.Count > 0)
    //    {
    //        foreach (var cell in Cells)
    //        {
    //            Vector2 bottomLeft = new Vector2(cell.Position.x - cell.Width / 2, cell.Position.y - cell.Height / 2);
    //            Vector2 topLeft = new Vector2(cell.Position.x - cell.Width / 2, cell.Position.y + cell.Height / 2);
    //            Vector2 topRight = new Vector2(cell.Position.x + cell.Width / 2, cell.Position.y + cell.Height / 2);
    //            Vector2 bottomRight = new Vector2(cell.Position.x + cell.Width / 2, cell.Position.y - cell.Height / 2);
    //            Gizmos.color = Color.red;
    //            Gizmos.DrawLine(bottomLeft, topLeft);
    //            Gizmos.DrawLine(topLeft, topRight);
    //            Gizmos.DrawLine(topRight, bottomRight);
    //            Gizmos.DrawLine(bottomRight, bottomLeft);
    //        }
    //    }
    //}

    private void CreateCellObjects()
    {
        for (int i = 0; i < Cells.Count; i++)
        {
            GameObject gameObject = new GameObject($"cell_{i}");
            gameObject.transform.localPosition = new Vector2(0.01f * Cells[i].Position.x, 0.01f * Cells[i].Position.y);
            gameObject.transform.localScale = new Vector2(0.01f * Cells[i].Width, 0.01f * Cells[i].Height);

            SpriteRenderer sprite = gameObject.AddComponent<SpriteRenderer>();
            sprite.sprite = CellSprite;

            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();

            Rigidbody2D rigidbody = gameObject.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0;
            rigidbody.freezeRotation = true;

            CellObjects.Add(gameObject);
        }
    }

    public static float RoundNumber(float x, float tileSize)
    {
        return Mathf.Floor((x + tileSize - 1) / tileSize) * tileSize;
    }

    //private void SeparateCells()
    //{
    //    for (int currentCell = 0; currentCell < Cells.Count; currentCell++)
    //    {
    //        for (int nextCell = 0; nextCell < Cells.Count; nextCell++)
    //        {
    //            if (currentCell == nextCell || !Cells[currentCell].IsOverlapping(Cells[nextCell])) continue;

    //            Vector2 direction = (Cells[nextCell].Position - Cells[currentCell].Position).normalized;

    //            Cells[currentCell].Move(-direction, 1);
    //            Cells[nextCell].Move(direction, 1);
    //        }
    //    }
    //}

    private void CreateTextures()
    {
        texture_cell = new Texture2D(1, 1);
        texture_cell.SetPixel(1, 1, Color.red);
        texture_cell.wrapMode = TextureWrapMode.Repeat;
        texture_cell.Apply();

        style_cell = new GUIStyle();
        style_cell.normal.background = texture_cell;


        texture_cellBorder = new Texture2D(1, 1);
        texture_cellBorder.SetPixel(1, 1, Color.black);
        texture_cellBorder.wrapMode = TextureWrapMode.Repeat;
        texture_cellBorder.Apply();

        style_cellBorder = new GUIStyle();
        style_cellBorder.normal.background = texture_cellBorder;
    }

    private void CreateCells(int cellCount)
    {
        int genWidth;
        int genHeight;
        Vector2 position;

        for (int i = 0; i < cellCount; i++)
        {
            // Generate random width/height
            genWidth = (int)Mathf.Round(RandomGauss(5, 15));
            genHeight = (int)Mathf.Round(RandomGauss(4, 12));

            // Ratio
            //while (genWidth/genHeight > 1)
            //{
            //    genWidth = (int)Mathf.Round(RandomGauss(5, 15));
            //    genHeight = (int)Mathf.Round(RandomGauss(4, 12));
            //}

            // Give random position
            //position = GetRandomPointInCircle(radius);
            position = GetRandomPointInElipse(Width, Height);

            Cells.Add(new Cell(position, genWidth * cellSize, genHeight * cellSize));
        }
    }

    public static float RandomGauss(float minValue, float maxValue)
    {
        float x, y, S;
        do
        {
            x = 2 * Random.value - 1;
            y = 2 * Random.value - 1;
            S = x * x + y * y;
        }
        while (S >= 1);

        // Standard distribution
        float stdDist = x * Mathf.Sqrt(-2 * Mathf.Log(S) / S);

        // Three sigma rule
        float mean = (minValue + maxValue) / 2;
        float sigma = (maxValue - mean) / 3;
        return Mathf.Clamp(stdDist * sigma + mean, minValue, maxValue);
    }

    public static Vector2 GetRandomPointInElipse(float width, float height)
    {
        float t = 2 * Mathf.PI * Random.value;
        float u = Random.value + Random.value;
        float r;

        if (u > 1)
        {
            r = 2 - u;
        }
        else
        {
            r = u;
        }

        //return new Vector2(RoundNumber(width * r * Mathf.Cos(t), cellSize), RoundNumber(height * r * Mathf.Sin(t), cellSize));
        return new Vector2(Mathf.Round(width * r * Mathf.Cos(t)), Mathf.Round(height * r * Mathf.Sin(t)));
    }
}
