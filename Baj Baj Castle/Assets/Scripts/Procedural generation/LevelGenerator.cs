using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LevelGenerator : MonoBehaviour
{
    public const float PIXEL_SIZE = 0.01f;
    public float SimulationDelay = 3f;
    public int CycleCount = 2000;

    public int TileSize = 16;
    public int Complexity = 50;
    public float GenerationRegionWidth = 200;
    public float GenerationRegionHeight = 10;

    public int RoomWidthMinimum = 6;
    public int RoomWidthMaximum = 30;

    public int RoomHeightMinimum = 6;
    public int RoomHeightMaximum = 20;

    public float FilteringCriteria = 1.25f;

    public Sprite CellSprite;

    private readonly List<Cell> cells = new List<Cell>();
    private readonly List<Cell> suitableCells = new List<Cell>();
    private readonly List<GameObject> fillerCells = new List<GameObject>();
    private readonly HashSet<Edge> delaunayGraph = new HashSet<Edge>();
    private HashSet<Edge> graph;
    

    public static float cellSize;
    private int simulationLoops = 0;

    private bool startSimulation = false;
    private bool startProcessing = false;
    private bool isSimulated = false;
    private bool isProcessed = false;
    private float startTime;
    private float endTime;
    public float EdgePercentage = 0.1f;

    void Start()
    {
        cellSize = TileSize * PIXEL_SIZE;
        CreateCells(Complexity);
        DrawGrid();
        print("Starting level generation process...");
        StartCoroutine(DelaySimulation(SimulationDelay));
    }

    void Update()
    {
        if (!isSimulated && startSimulation)
        {
            SimulateCells();
        }

        if (isSimulated && !isProcessed && startProcessing)
        {
            ProcessCells();
        }
    }

    private void ProcessCells()
    {
        // filter out cells that qualify to be rooms
        FilterCells();
        endTime = Time.realtimeSinceStartup;
        print("Cell filtering took " + (endTime - startTime) + " seconds.");

        Graph();

        // create filler cells to fill in the gaps between cells
        CreateFillerCells();
        endTime = Time.realtimeSinceStartup;
        print("Filler cell creation took " + (endTime - startTime) + " seconds.");

        // create hallways between cells
        CreateHallways();

        isProcessed = true;
    }

    private void CreateHallways()
    {
        var hallways = new HashSet<Edge>();
        // calculates the midpoint between both nodes' positions and checks to see if that midpoint's x or y attributes are inside the node's boundaries.
        // If they are then I create the line from that midpoint's position.
        // If they aren't then I create two lines, both going from the source's midpoint to the target's midpoint but only in one axis.
        foreach (var edge in graph)
        {
            var midpoint = new Point((edge.P1.X + edge.P2.X) / 2, (edge.P1.Y + edge.P2.Y) / 2);
            if (midpoint.X >= edge.P1.X && midpoint.X <= edge.P2.X)
            {
                hallways.Add(new Edge(edge.P1, midpoint));
                hallways.Add(new Edge(midpoint, edge.P2));
            }
            else if (midpoint.Y >= edge.P1.Y && midpoint.Y <= edge.P2.Y)
            {
                hallways.Add(new Edge(edge.P1, midpoint));
                hallways.Add(new Edge(midpoint, edge.P2));
            }
            else
            {
                hallways.Add(new Edge(edge.P1, edge.P2));
            }
        }

        DrawEdges(hallways, Color.yellow, 100f);
    }

    private void CreateFillerCells()
    {
        // find extremes of suitable cells
        var minX = suitableCells.Min(c => c.SimulationCell.transform.position.x - c.Width * PIXEL_SIZE / 2) + cellSize / 2;
        var minY = suitableCells.Min(c => c.SimulationCell.transform.position.y - c.Height * PIXEL_SIZE / 2) + cellSize / 2;
        var maxX = suitableCells.Max(c => c.SimulationCell.transform.position.x + c.Width * PIXEL_SIZE / 2) - cellSize / 2;
        var maxY = suitableCells.Max(c => c.SimulationCell.transform.position.y + c.Height * PIXEL_SIZE / 2) - cellSize / 2;

        // find all positions between suitable cells every cellSize units
        var positions = new List<Vector2>();
        for (var x = minX; x <= maxX; x += cellSize)
        {
            for (var y = minY; y <= maxY; y += cellSize)
            {
                var position = new Vector2(x, y);
                // check if position is not in any of the suitable cells area
                if (suitableCells.Any(c => !c.IsPointInside(position)))
                {
                    positions.Add(position);
                }
            }
        }

        // create filler cells
        foreach (var position in positions)
        {
            var fillerCell = new GameObject("Filler Cell");
            fillerCell.transform.position = position;
            fillerCell.AddComponent<SpriteRenderer>().sprite = CellSprite;
            fillerCell.GetComponent<SpriteRenderer>().color = new Color(0, 0, 1f, 0.15f);
            fillerCell.transform.localScale = new Vector2(cellSize, cellSize);
            fillerCells.Add(fillerCell);
        }
    }

    // Draw a grid with 1x1 cells
    private void DrawGrid(){
        var minX = -20f;
        var minY = -20f;
        var maxX = 20f;
        var maxY = 20f;

        // round values to grid
        minX = Mathf.Round(minX / cellSize) * cellSize;
        minY = Mathf.Round(minY / cellSize) * cellSize;
        maxX = Mathf.Round(maxX / cellSize) * cellSize;
        maxY = Mathf.Round(maxY / cellSize) * cellSize;

        // find corner positions from extremes
        var topLeft = new Vector2(minX, maxY);
        var topRight = new Vector2(maxX, maxY);
        var bottomLeft = new Vector2(minX, minY);
        var bottomRight = new Vector2(maxX, minY);

        HashSet<Edge> edges = new HashSet<Edge>();

        // create edges every cellSize units
        for (var x = minX; x <= maxX; x += cellSize)
        {
            edges.Add(new Edge(new Point(x, minY), new Point(x, maxY)));
        }
        for (var y = minY; y <= maxY; y += cellSize)
        {
            edges.Add(new Edge(new Point(minX, y), new Point(maxX, y)));
        }

        // Draw all edges
        DrawEdges(edges, new Color(0, 0, 1f, 0.3f), 100f);
    }

    private void Graph()
    {
        // get all unique vertices
        HashSet<Point> points = new HashSet<Point>();
        foreach (var cell in suitableCells)
        {
            points.Add(new Point(cell.SimulationCell.transform.position.x, cell.SimulationCell.transform.position.y));
        }

        // perform triangulation
        var triangulation = Triangulate(points);
        endTime = Time.realtimeSinceStartup;
        print("Cell triangulation took " + (endTime - startTime) + " seconds.");
        DrawTriangles(triangulation);
        startTime = endTime;

        Cleanup(triangulation);

        // get a minimum spanning tree from triangulation results
        graph = MinimumSpanningTree(triangulation, points.ToList());
        endTime = Time.realtimeSinceStartup;
        print("Minimum spanning tree calculation took " + (endTime - startTime) + " seconds.");
        DrawEdges(graph, Color.green, 3f);
        startTime = endTime;

        // add some edges from delaunay graph back to graph
        RefillEdges(EdgePercentage);
        DrawEdges(graph, Color.green, 100f);
    }

    private void RefillEdges(float edgePercentage)
    {
        var remainingEdges = delaunayGraph.Except(graph);
        foreach (var edge in remainingEdges)
        {
            if (Random.value < edgePercentage)
            {
                graph.Add(edge);
            }
        }
    }

    // Perform Delaunay triangulation on a set of points
    private HashSet<Triangle> Triangulate(HashSet<Point> points)
    {
        var triangulator = new DelaunayTriangulator();
        triangulator.CreateSupraTriangle(suitableCells);
        triangulator.AdjustSupraTriangle(points);
        return triangulator.BowyerWatson(points);
    }

    // Destroy cells that are not part of the triangulation
    private void Cleanup(HashSet<Triangle> triangulation)
    {
        var cellsToDestroyCount = 0;
        foreach (var cell in suitableCells)
        {
            if (!cell.IsPartOf(triangulation))
            {
                Destroy(cell.SimulationCell);
                cellsToDestroyCount++;
            }
        }
        if (cellsToDestroyCount > 0)
        {
            print("Destroyed " + cellsToDestroyCount + " cells.");
        }
    }

    private static void DrawTriangles(HashSet<Triangle> triangles)
    {
        foreach (var triangle in triangles)
        {
            var p1 = new Vector2((float)triangle.Vertices[0].X, (float)triangle.Vertices[0].Y);
            var p2 = new Vector2((float)triangle.Vertices[1].X, (float)triangle.Vertices[1].Y);
            var p3 = new Vector2((float)triangle.Vertices[2].X, (float)triangle.Vertices[2].Y);

            Debug.DrawLine(p1, p2, Color.green, 3f, true);
            Debug.DrawLine(p2, p3, Color.green, 3f, true);
            Debug.DrawLine(p3, p1, Color.green, 3f, true);
        }
    }

    private static void DrawEdges(HashSet<Edge> edges, Color color, float duration)
    {
        foreach (var edge in edges)
        {
            var p1 = new Vector2((float)edge.P1.X, (float)edge.P1.Y);
            var p2 = new Vector2((float)edge.P2.X, (float)edge.P2.Y);

            Debug.DrawLine(p1, p2, color, duration, true);
        }
    }

    private HashSet<Edge> MinimumSpanningTree(HashSet<Triangle> triangles, List<Point> points)
    {
        foreach (var triangle in triangles)
        {
            var p1 = new Point(triangle.Vertices[0].X, triangle.Vertices[0].Y);
            var p2 = new Point(triangle.Vertices[1].X, triangle.Vertices[1].Y);
            var p3 = new Point(triangle.Vertices[2].X, triangle.Vertices[2].Y);

            delaunayGraph.Add(new Edge(p1, p2));
            delaunayGraph.Add(new Edge(p2, p3));
            delaunayGraph.Add(new Edge(p3, p1));
        }
        var sortedEdges = delaunayGraph.OrderBy(e => e.Weight).ToList();

        var forest = new DisjointSet(points.Count);
        for(int i = 0; i < points.Count; i++)
        {
            forest.MakeSet(i);
        }

        var minimumSpanningTree = new HashSet<Edge>();

        foreach (var edge in sortedEdges)
        {
            int indexP1 = points.FindIndex(p => p.Equals(edge.P1));
            int indexP2 = points.FindIndex(p => p.Equals(edge.P2));

            if (forest.Find(indexP1) != forest.Find(indexP2))
            {
                minimumSpanningTree.Add(edge);
                forest.Union(indexP1, indexP2);
            }
        }
        return minimumSpanningTree;
    }

    private void FilterCells()
    {
        float widthAverage = 0;
        float heightAverage = 0;

        foreach (var cell in cells)
        {
            widthAverage += cell.Width;
            heightAverage += cell.Height;
        }

        widthAverage /= cells.Count;
        heightAverage /= cells.Count;

        foreach (var cell in cells)
        {
            if(cell.Width >= widthAverage * FilteringCriteria && cell.Height >= heightAverage * FilteringCriteria){
                cell.SimulationCell.GetComponent<SpriteRenderer>().color = Color.red;
                suitableCells.Add(cell);
            }else{
                Destroy(cell.SimulationCell);
            }
        }
    }

    private void SimulateCells()
    {
        int simulatedCellsCount = 0;
        var centralPosition = new Vector3(0, 0, 0);

        while(simulatedCellsCount < cells.Count && simulationLoops < CycleCount)
        {   
            simulatedCellsCount = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                // Run on first loop
                if(simulationLoops == 0){
                    AlignSimulationCell(i);
                    continue;
                }

                Cell cell = cells[i];
                var overlappingCellIdList = FindOverlaps(cell);

                if(overlappingCellIdList.Count > 0)
                {
                    var firstId = overlappingCellIdList[0];
                    var overlappingCell = cells[firstId];

                    Vector2 direction = (cell.PhysicsCell.transform.position - overlappingCell.PhysicsCell.transform.position).normalized * PIXEL_SIZE;

                    overlappingCell.PhysicsCell.transform.Translate(-direction);
                    cell.PhysicsCell.transform.Translate(direction);

                    AlignSimulationCell(firstId);
                }else{
                    simulatedCellsCount++;
                }
            }
            simulationLoops++;
        }

        // Simulation ending
        foreach (var cell in cells)
        {
            cell.PhysicsCell.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            Destroy(cell.PhysicsCell);
        }

        isSimulated = true;

        var endTime = Time.realtimeSinceStartup;
        print($"Simulation took {simulationLoops} cycles");
        print("Simulation took " + (endTime - startTime) + " seconds.");
        
        StartCoroutine(DelayProcessing(SimulationDelay));
    }

    // Update simulation cell positions while snapping to TileSize grid
    private void AlignSimulationCell(int i){
        var cell = cells[i];
        var position = cell.PhysicsCell.transform.localPosition;
        var x = RoundNumber(position.x, cellSize);
        var y = RoundNumber(position.y, cellSize);

        cell.SimulationCell.transform.localPosition = new Vector2(x, y);
    }

    private List<int> FindOverlaps(Cell cell)
    {
        var overlappingCellIdList = new List<int>();

        for (int i = 0; i < cells.Count; i++)
        {
            if(cells[i] != cell)
            {
                if(cell.IsOverlapping(cells[i]))
                {
                    overlappingCellIdList.Add(i);
                }
            }
        }

        return overlappingCellIdList;
    }

    private void CreateSimulationCellObject(Cell cell, int i)
    {
        GameObject gameObject = new GameObject($"Cell #{i}");
        gameObject.SetActive(false);
        gameObject.transform.localScale = new Vector2(PIXEL_SIZE * cell.Width, PIXEL_SIZE * cell.Height);
        gameObject.layer = LayerMask.NameToLayer("Cell");

        SpriteRenderer sprite = gameObject.AddComponent<SpriteRenderer>();
        sprite.sprite = CellSprite;
        sprite.color = new Color(0, 1, 1, 0.3f);
        sprite.sortingLayerName = "Render";

        cell.SimulationCell = gameObject;
    }

    private void CreatePhysicsCellObject(Cell cell, int i)
    {
        GameObject gameObject = new GameObject($"Simulation cell #{i}");
        gameObject.transform.localPosition = new Vector2(PIXEL_SIZE * cell.Position.x, PIXEL_SIZE * cell.Position.y);
        gameObject.transform.localScale = new Vector2(PIXEL_SIZE * cell.Width, PIXEL_SIZE * cell.Height);

        SpriteRenderer sprite = gameObject.AddComponent<SpriteRenderer>();
        sprite.sprite = CellSprite;

        gameObject.AddComponent<BoxCollider2D>();

        Rigidbody2D rigidbody = gameObject.AddComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0;
        rigidbody.freezeRotation = true;

        cell.PhysicsCell = gameObject;
    }

    public static float RoundNumber(float x, float y)
    {
        return Mathf.Floor((x + y - 1) / y) * y;
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
                genWidth = Mathf.RoundToInt(RandomGauss(RoomWidthMinimum, RoomWidthMaximum));
                genHeight = Mathf.RoundToInt(RandomGauss(RoomHeightMinimum, RoomHeightMaximum));
            }
            while (genWidth / genHeight > 2
                   || genWidth % 2 != 0
                   || genHeight % 2 != 0);

            // Give random position
            Vector2 position = GetRandomPointInElipse(GenerationRegionWidth, GenerationRegionHeight);
            Cell cell = new Cell(position, genWidth * TileSize, genHeight * TileSize);

            CreatePhysicsCellObject(cell, i);
            CreateSimulationCellObject(cell, i);

            cells.Add(cell);
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

    // Functions below are for organising and delaying various actions
    IEnumerator DelaySimulation(float time)
    {
        yield return new WaitForSeconds(time);
        print("Starting simulation...");
        foreach (var cell in cells)
        {
            cell.SimulationCell.SetActive(true);
        }
        startTime = Time.realtimeSinceStartup;
        startSimulation = true;
    }

    IEnumerator DelayProcessing(float time)
    {
        yield return new WaitForSeconds(time);
        print("Starting processing...");
        startTime = Time.realtimeSinceStartup;
        startProcessing = true;
    }
}

