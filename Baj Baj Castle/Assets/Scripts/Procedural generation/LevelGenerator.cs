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

    public int TileSize = 16;
    public int Complexity = 50;
    public float Width = 200;
    public float Height = 10;
    private static int cellSize;

    private bool IsSimulated = false;
    private int simulationLoops = 0;

    public int LoopCount = 10;

    // Start is called before the first frame update
    void Start()
    {
        cellSize = TileSize;
        CreateCells(Complexity);
    }

    // Update is called once per frame
    void Update()
    {
        while (!IsSimulated && simulationLoops < LoopCount)
        {
            SimulateCells();
        }
    }


    private void SimulateCells()
    {
        int simulatedCellsCount = 0;

        for (int i = 0; i < Cells.Count; i++)
        {
            UpdateDisplayCellPosition(i);

            BoxCollider2D collider = Cells[i].DisplayCell.GetComponent<BoxCollider2D>();

            // Separate cells
            List<Collider2D> collisions = FindCollisions(collider);
            if(collisions.Count == 0)
            {
                simulatedCellsCount++;
            }

            foreach (var collision in collisions)
            {
                Vector2 direction = (collider.transform.position - collision.transform.position).normalized * 0.01f;
                Cells[i].SimulationCell.transform.Translate(direction);
                Cells.Find(x => x.DisplayCollider == collision).SimulationCell.transform.Translate(-direction);
            }
        }

        simulationLoops++;

        if (simulatedCellsCount == Cells.Count || simulationLoops >= LoopCount)
        {
            IsSimulated = true;

            foreach (var cell in Cells)
            {
                cell.SimulationCell.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                Destroy(cell.SimulationCell, 3f);
            }
        }
    }

    private void UpdateDisplayCellPosition(int i)
    {
        float x = RoundNumber(100f * Cells[i].SimulationCell.transform.localPosition.x, cellSize);
        float y = RoundNumber(100f * Cells[i].SimulationCell.transform.localPosition.y, cellSize);
        Vector2 position = new Vector2(0.01f * x, 0.01f * y);
        Cells[i].DisplayCell.transform.localPosition = position;
    }

    private List<Collider2D> FindCollisions(BoxCollider2D collider)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.layerMask = LayerMask.NameToLayer("Cell");

        List<Collider2D> collisions = new List<Collider2D>();

        Vector2 originalSize = collider.size;
        collider.size = new Vector2(originalSize.x - 0.08f, originalSize.y - 0.08f);
        collider.OverlapCollider(filter, collisions);
        collider.size = originalSize;

        List<Collider2D> actualCollisions = new List<Collider2D>(collisions);

        foreach (var collision in collisions)
        {
            if (collision.gameObject.layer != LayerMask.NameToLayer("Cell"))
            {
                actualCollisions.Remove(collision);
            }
        }

        return actualCollisions;
    }

    private void CreateCellObject(Cell cell, int i)
    {
        GameObject gameObject = new GameObject($"Cell #{i}");
        gameObject.transform.localScale = new Vector2(0.01f * cell.Width, 0.01f * cell.Height);
        gameObject.layer = LayerMask.NameToLayer("Cell");

        SpriteRenderer sprite = gameObject.AddComponent<SpriteRenderer>();
        sprite.sprite = CellSprite;
        sprite.color = new Color(0, 1, 1, 0.3f);
        sprite.sortingLayerName = "Render";

        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        cell.DisplayCell = gameObject;
        cell.DisplayCollider = collider;
    }

    private void CreateSimulationCellObject(Cell cell, int i)
    {
        GameObject gameObject = new GameObject($"Simulation cell #{i}");
        gameObject.transform.localPosition = new Vector2(0.01f * cell.Position.x, 0.01f * cell.Position.y);
        gameObject.transform.localScale = new Vector2(0.01f * cell.Width, 0.01f * cell.Height);

        SpriteRenderer sprite = gameObject.AddComponent<SpriteRenderer>();
        sprite.sprite = CellSprite;

        gameObject.AddComponent<BoxCollider2D>();

        Rigidbody2D rigidbody = gameObject.AddComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0;
        rigidbody.freezeRotation = true;

        cell.SimulationCell = gameObject;
    }

    public static float RoundNumber(float x, float tileSize)
    {
        return Mathf.Floor((x + tileSize - 1) / tileSize) * tileSize;
    }

    // Creates cells for simulation
    private void CreateCells(int cellCount)
    {
        int genWidth;
        int genHeight;

        for (int i = 0; i < cellCount; i++)
        {
            // Generate random width/height within ratio
            do
            {
                genWidth = Mathf.RoundToInt(RandomGauss(5, 15));
                genHeight = Mathf.RoundToInt(RandomGauss(4, 12));
            }
            while (genWidth / genHeight > 2);

            // Give random position
            Vector2 position = GetRandomPointInElipse(Width, Height);
            Cell cell = new Cell(position, genWidth * cellSize, genHeight * cellSize);

            CreateSimulationCellObject(cell, i);
            CreateCellObject(cell, i);

            Cells.Add(cell);
        }
    }

    // Returns a random number
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

    // Returns a random point in an elipse
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

        return new Vector2(Mathf.Round(width * r * Mathf.Cos(t)), Mathf.Round(height * r * Mathf.Sin(t)));
    }
}
