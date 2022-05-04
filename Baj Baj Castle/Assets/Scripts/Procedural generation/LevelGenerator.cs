using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    // Constants
    public const float PIXEL_SIZE = 0.01f;
    public const float CELL_SIZE = 0.16f;
    private readonly List<Cell> cells = new List<Cell>();

    private Sprite cellSprite;

    // Inputs
    public int Complexity = 50;
    public int CycleCount = 2000;
    private HashSet<Edge> delaunayGraph;
    public float EdgePercentage = 0.1f;
    private float endTime;
    public float FilteringCriteria = 1.0f;
    public float GenerationRegionHeight = 50;
    public float GenerationRegionWidth = 50;
    public List<Cell> Hallways = new List<Cell>();
    public int HallwayWidth = 4;

    private bool isDebug;
    private bool isMapped;
    private bool isProcessed;
    private bool isSimulated;
    private HashSet<Edge> levelGraph;
    private int levelSize;
    public int RoomHeightMaximum = 20;
    public int RoomHeightMinimum = 6;

    // Outputs
    public List<Cell> Rooms = new List<Cell>();
    public int RoomWidthMaximum = 30;
    public int RoomWidthMinimum = 6;
    public float SimulationDelay = 3f;
    private int simulationLoops;
    private bool startMapping;
    private bool startProcessing;
    private bool startSimulation;
    private float startTime;
    public bool IsCompleted => isSimulated && isProcessed && isMapped;

    public void Cleanup()
    {
        foreach (var cell in cells)
        {
            if (cell.DisplayCell != null) Destroy(cell.DisplayCell);

            if (cell.SimulationCell != null) Destroy(cell.SimulationCell);

            if (cell.PhysicsCell != null) Destroy(cell.PhysicsCell);
        }

        cells.Clear();

        foreach (var cell in Rooms)
        {
            if (cell.DisplayCell != null) Destroy(cell.DisplayCell);

            if (cell.SimulationCell != null) Destroy(cell.SimulationCell);

            if (cell.PhysicsCell != null) Destroy(cell.PhysicsCell);
        }

        Rooms.Clear();

        foreach (var cell in Hallways)
        {
            if (cell.DisplayCell != null) Destroy(cell.DisplayCell);

            if (cell.SimulationCell != null) Destroy(cell.SimulationCell);

            if (cell.PhysicsCell != null) Destroy(cell.PhysicsCell);
        }

        Hallways.Clear();

        delaunayGraph = null;
        levelGraph = null;
        simulationLoops = 0;
        startSimulation = false;
        startProcessing = false;
        startMapping = false;
        isSimulated = false;
        isProcessed = false;
    }

    public void GenerateLevel(int level, bool newIsDebug, Sprite newCellSprite)
    {
        // Set input variables
        isDebug = newIsDebug;
        cellSprite = newCellSprite;
        levelSize = (int)(4 + level * 1.4f);
        if (isDebug) Debug.Log("Generating level #" + level);
        PrepareForGeneration();
    }

    private void PrepareForGeneration()
    {
        startTime = Time.realtimeSinceStartup;
        isSimulated = false;
        isProcessed = false;
        isMapped = false;
        simulationLoops = 0;

        CreateCells(Complexity);
        if (isDebug)
        {
            startSimulation = false;
            startProcessing = false;
            startMapping = false;
        }
        else
        {
            startProcessing = true;
            startMapping = true;
        }

        StartCoroutine(DelaySimulation(SimulationDelay));
    }

    [UsedImplicitly]
    private void FixedUpdate()
    {
        if (!isSimulated && startSimulation) SimulateCells();

        if (isSimulated && !isProcessed && startProcessing) ProcessCells();

        if (isProcessed && !isMapped && startMapping) MapCells();
    }

    private void SimulateCells()
    {
        var simulatedCellsCount = 0;

        while (simulatedCellsCount < cells.Count && simulationLoops < CycleCount)
        {
            simulatedCellsCount = 0;
            for (var i = 0; i < cells.Count; i++)
            {
                // Run on first loop
                if (simulationLoops == 0)
                {
                    AlignSimulationCell(i);
                    continue;
                }

                var cell = cells[i];
                var overlappingCellIdList = FindOverlaps(cell);

                if (overlappingCellIdList.Count > 0)
                {
                    var firstId = overlappingCellIdList[0];
                    var overlappingCell = cells[firstId];

                    Vector2 direction =
                        (cell.PhysicsCell.transform.position - overlappingCell.PhysicsCell.transform.position)
                        .normalized * PIXEL_SIZE;

                    overlappingCell.PhysicsCell.transform.Translate(-direction);
                    cell.PhysicsCell.transform.Translate(direction);

                    AlignSimulationCell(firstId);
                }
                else
                {
                    simulatedCellsCount++;
                }
            }

            simulationLoops++;
        }

        // Simulation ending
        foreach (var cell in cells) Destroy(cell.PhysicsCell);

        isSimulated = true;

        if (isDebug)
        {
            endTime = Time.realtimeSinceStartup;
            print($"Simulation took {simulationLoops} cycles");
            print("Simulation took " + (endTime - startTime) + " seconds.");
            StartCoroutine(DelayProcessing(SimulationDelay));
        }
    }

    private void ProcessCells()
    {
        // set cell positions
        foreach (var cell in cells)
            cell.Position = new Vector2((int)(cell.SimulationCell.transform.position.x / CELL_SIZE),
                (int)(cell.SimulationCell.transform.position.y / CELL_SIZE));

        if (isDebug) startTime = Time.realtimeSinceStartup;
        // filter out cells that qualify to be rooms
        FilterCells();

        if (isDebug)
        {
            endTime = Time.realtimeSinceStartup;
            print("Cell filtering took " + (endTime - startTime) + " seconds.");
        }

        Graph();

        isProcessed = true;
        if (isDebug) StartCoroutine(DelayMapping(SimulationDelay * 2));
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
            if (cell.Width >= widthAverage * FilteringCriteria
                && cell.Height >= heightAverage * FilteringCriteria)
            {
                if (Rooms.Count < levelSize)
                {
                    cell.SimulationCell.GetComponent<SpriteRenderer>().color = Color.red;
                    Rooms.Add(cell);
                }
                else
                {
                    break;
                }
            }

        if (Rooms.Count < levelSize)
        {
            var sortedCells = cells.OrderByDescending(cell => cell.Width / cell.Height);
            foreach (var cell in sortedCells)
                if (Rooms.Count < levelSize)
                {
                    cell.SimulationCell.GetComponent<SpriteRenderer>().color = Color.red;
                    Rooms.Add(cell);
                }
                else
                {
                    break;
                }
        }

        // destroy all simulation cells that are not part of suitableCells
        foreach (var cell in cells)
            if (!Rooms.Contains(cell))
                Destroy(cell.SimulationCell);
        cells.Clear();
    }

    private void Graph()
    {
        // get all unique vertices
        var points = new HashSet<Point>();
        foreach (var cell in Rooms)
            points.Add(new Point(cell.SimulationCell.transform.position.x, cell.SimulationCell.transform.position.y));

        // perform triangulation
        if (isDebug) startTime = Time.realtimeSinceStartup;
        var triangulation = Triangulate(points);
        if (isDebug)
        {
            endTime = Time.realtimeSinceStartup;
            print("Cell triangulation took " + (endTime - startTime) + " seconds.");
            DrawTriangles(triangulation);
            print("Triangulation: " + triangulation.Count);
        }

        Cleanup(triangulation);

        // get a minimum spanning tree from triangulation results
        if (isDebug) startTime = Time.realtimeSinceStartup;
        levelGraph = MinimumSpanningTree(triangulation, points.ToList());
        if (isDebug)
        {
            endTime = Time.realtimeSinceStartup;
            print("Minimum spanning tree calculation took " + (endTime - startTime) + " seconds.");
            DrawEdges(levelGraph, Color.green, 3f);
        }

        // add some edges from delaunay graph back to graph
        RefillEdges(EdgePercentage);

        if (isDebug) DrawEdges(levelGraph, Color.green, 6f);
    }

    private void MapCells()
    {
        // calculate hallways between cells
        if (isDebug)
        {
            startTime = Time.realtimeSinceStartup;
        }

        var hallways = CalculateHallways();
        if (isDebug)
        {
            endTime = Time.realtimeSinceStartup;
            print("Hallway calculation took " + (endTime - startTime) + " seconds.");
            print("Found " + hallways.Count + " hallways.");
            DrawEdges(hallways, Color.cyan, 3f);
            startTime = Time.realtimeSinceStartup;
        }

        // carve out hallways
        var hallwayPoints = GetHallwayPoints(hallways);
        CreateHallways(hallwayPoints, Color.cyan);

        if (isDebug)
        {
            endTime = Time.realtimeSinceStartup;
            print("Hallway creation took " + (endTime - startTime) + " seconds.");
        }

        isMapped = true;
    }

    private void CreateHallways(HashSet<Vector2> positions, Color color)
    {
        foreach (var position in positions)
        // if(!roomCells.Any(c => c.IsPointInside(new Point(position.x * cellSize, position.y * cellSize))))
        {
            // check if cell is inside a room
            // if (!roomCells.Any(c => c.IsPointInside(new Point(position.x, position.y))))
            // {
            var cell = new Cell(position, 1, 1);
            cell.CreateDisplayCellObject(cellSprite, color);
            Hallways.Add(cell);
            // }
        }
    }

    // Creates hallways between rooms based on hallway edges, removing excess ones
    private HashSet<Vector2> GetHallwayPoints(HashSet<Edge> hallways)
    {
        var hallwayPoints = new HashSet<Vector2>();

        for (var i = 0; i < hallways.Count; i++)
        {
            var hallway = hallways.ElementAt(i);
            int from;
            int to;
            var trueWidth = HallwayWidth - 2;
            var offsetFrom = 1 - trueWidth;
            var offsetTo = 1 + trueWidth;

            var TOLERANCE = 0.001f;
            if (Math.Abs(hallway.P1.X - hallway.P2.X) < TOLERANCE) // hallway is vertical
            {
                if (hallway.P1.Y < hallway.P2.Y) // goes up
                {
                    from = Mathf.RoundToInt((float)hallway.P1.Y / CELL_SIZE);
                    to = Mathf.RoundToInt((float)hallway.P2.Y / CELL_SIZE);
                }
                else // goes down
                {
                    from = Mathf.RoundToInt((float)hallway.P2.Y / CELL_SIZE);
                    to = Mathf.RoundToInt((float)hallway.P1.Y / CELL_SIZE);
                }

                for (var y = from + offsetFrom; y < to + offsetTo; y++)
                {
                    var isOdd = false;
                    var odds = 0;
                    var evens = 0;
                    for (var j = 0; j < HallwayWidth; j++)
                    {
                        int positionX;
                        if (isOdd)
                        {
                            positionX = Mathf.RoundToInt((float)(hallway.P1.X - (j - evens) * CELL_SIZE) / CELL_SIZE);
                            isOdd = false;
                            odds++;
                        }
                        else
                        {
                            positionX = Mathf.RoundToInt(
                                (float)(hallway.P1.X + (j - odds + 1) * CELL_SIZE) / CELL_SIZE);
                            isOdd = true;
                            evens++;
                        }

                        // check if point is inside any room
                        hallwayPoints.Add(new Vector2(positionX, y));
                    }
                }
            }
            else // hallway is horizontal
            {
                if (hallway.P1.X < hallway.P2.X) // goes right
                {
                    from = Mathf.RoundToInt((float)hallway.P1.X / CELL_SIZE);
                    to = Mathf.RoundToInt((float)hallway.P2.X / CELL_SIZE);
                }
                else // goes left
                {
                    from = Mathf.RoundToInt((float)hallway.P2.X / CELL_SIZE);
                    to = Mathf.RoundToInt((float)hallway.P1.X / CELL_SIZE);
                }

                for (var x = from + offsetFrom; x < to + offsetTo; x++)
                {
                    var isOdd = false;
                    var odds = 0;
                    var evens = 0;
                    for (var j = 0; j < HallwayWidth; j++)
                    {
                        int positionY;
                        if (isOdd)
                        {
                            positionY = Mathf.RoundToInt((float)(hallway.P1.Y - (j - evens) * CELL_SIZE) / CELL_SIZE);
                            isOdd = false;
                            odds++;
                        }
                        else
                        {
                            positionY = Mathf.RoundToInt(
                                (float)(hallway.P1.Y + (j - odds + 1) * CELL_SIZE) / CELL_SIZE);
                            isOdd = true;
                            evens++;
                        }

                        hallwayPoints.Add(new Vector2(x, positionY));
                    }
                }
            }
        }

        return hallwayPoints;
    }

    // Converts all graph edges to hallways between rooms
    private HashSet<Edge> CalculateHallways()
    {
        // offset is to ensure all hallway walls are aligned with the room walls
        var offset = CELL_SIZE * (HallwayWidth / 4 + 1);

        var hallways = new HashSet<Edge>();
        foreach (var edge in levelGraph)
        {
            // find cells that are connected by an edge
            var TOLERANCE = 0.001;
            var c1 = Rooms.First(c => Math.Abs(c.SimulationCell.transform.position.x - edge.P1.X) < TOLERANCE
                                      && Math.Abs(c.SimulationCell.transform.position.y - edge.P1.Y) < TOLERANCE);
            var c2 = Rooms.First(c => Math.Abs(c.SimulationCell.transform.position.x - edge.P2.X) < TOLERANCE
                                      && Math.Abs(c.SimulationCell.transform.position.y - edge.P2.Y) < TOLERANCE);

            // calculate midpoint between the two cells
            var midpoint = new Point((edge.P1.X + edge.P2.X) / 2, (edge.P1.Y + edge.P2.Y) / 2);

            // calculate various offsets and extremes
            var c1OffsetX = c1.SimulationCell.transform.localScale.x / 2;
            var c1OffsetY = c1.SimulationCell.transform.localScale.y / 2;

            var c2OffsetX = c2.SimulationCell.transform.localScale.x / 2;
            var c2OffsetY = c2.SimulationCell.transform.localScale.y / 2;

            var c1XMax = c1.SimulationCell.transform.position.x + c1OffsetX;
            var c1XMin = c1.SimulationCell.transform.position.x - c1OffsetX;
            var c1YMax = c1.SimulationCell.transform.position.y + c1OffsetY;
            var c1YMin = c1.SimulationCell.transform.position.y - c1OffsetY;

            var c2XMax = c2.SimulationCell.transform.position.x + c2OffsetX;
            var c2XMin = c2.SimulationCell.transform.position.x - c2OffsetX;
            var c2YMax = c2.SimulationCell.transform.position.y + c2OffsetY;
            var c2YMin = c2.SimulationCell.transform.position.y - c2OffsetY;

            var isFound = false;

            var isLeft = c1.SimulationCell.transform.position.x > c2.SimulationCell.transform.position.x;
            var isUp = c1.SimulationCell.transform.position.y > c2.SimulationCell.transform.position.y;

            var cellSizeHalf = CELL_SIZE / 2;

            // Alignment checks
            if (Mathf.RoundToInt((float)midpoint.X / CELL_SIZE) % 2 == 0) midpoint.X += cellSizeHalf;

            if (Mathf.RoundToInt((float)midpoint.Y / CELL_SIZE) % 2 == 0) midpoint.Y += cellSizeHalf;

            midpoint.X = Mathf.RoundToInt((float)midpoint.X / CELL_SIZE) * CELL_SIZE;
            midpoint.Y = Mathf.RoundToInt((float)midpoint.Y / CELL_SIZE) * CELL_SIZE;

            if (isLeft)
            {
                if (midpoint.X > c1XMin + offset
                    && midpoint.X < c1XMax - offset
                    && midpoint.X > c2XMin + offset
                    && midpoint.X < c2XMax - offset)
                {
                    hallways.Add(isUp
                        ? new Edge(new Point(midpoint.X, c1YMin), new Point(midpoint.X, c2YMax))
                        : new Edge(new Point(midpoint.X, c1YMax), new Point(midpoint.X, c2YMin)));
                    isFound = true;
                }
            }
            else
            {
                if (midpoint.X > c2XMin + offset
                    && midpoint.X < c2XMax - offset
                    && midpoint.X > c1XMin + offset
                    && midpoint.X < c1XMax - offset)
                {
                    hallways.Add(isUp
                        ? new Edge(new Point(midpoint.X, c1YMin), new Point(midpoint.X, c2YMax))
                        : new Edge(new Point(midpoint.X, c1YMax), new Point(midpoint.X, c2YMin)));
                    isFound = true;
                }
            }

            if (!isFound)
            {
                if (isUp)
                {
                    if (midpoint.Y > c1YMin + offset
                        && midpoint.Y < c1YMax - offset
                        && midpoint.Y > c2YMin + offset
                        && midpoint.Y < c2YMax - offset)
                    {
                        hallways.Add(isLeft
                            ? new Edge(new Point(c1XMin, midpoint.Y), new Point(c2XMax, midpoint.Y))
                            : new Edge(new Point(c1XMax, midpoint.Y), new Point(c2XMin, midpoint.Y)));
                        isFound = true;
                    }
                }
                else
                {
                    if (midpoint.Y > c2YMin + offset
                        && midpoint.Y < c2YMax - offset
                        && midpoint.Y > c1YMin + offset
                        && midpoint.Y < c1YMax - offset)
                    {
                        hallways.Add(isLeft
                            ? new Edge(new Point(c1XMin, midpoint.Y), new Point(c2XMax, midpoint.Y))
                            : new Edge(new Point(c1XMax, midpoint.Y), new Point(c2XMin, midpoint.Y)));
                        isFound = true;
                    }
                }
            }

            // if a straight hallway isn't possible, create a L shaped hallway
            if (!isFound)
            {
                Point c1Point;
                Point c2Point;

                var leftPoint = new Point(c1.SimulationCell.transform.position.x,
                    c2.SimulationCell.transform.position.y);

                // check if point is within any of the cells
                if (!Rooms.Any(c => c.IsPointInside(leftPoint)))
                {
                    if (isLeft && isUp)
                    {
                        c1Point = new Point(c1.SimulationCell.transform.position.x, c1YMin);
                        c2Point = new Point(c2XMax, c2.SimulationCell.transform.position.y);
                    }
                    else if (!isLeft && isUp)
                    {
                        c1Point = new Point(c1.SimulationCell.transform.position.x, c1YMin);
                        c2Point = new Point(c2XMin, c2.SimulationCell.transform.position.y);
                    }
                    else if (isLeft && !isUp)
                    {
                        c1Point = new Point(c1.SimulationCell.transform.position.x, c1YMax);
                        c2Point = new Point(c2XMax, c2.SimulationCell.transform.position.y);
                    }
                    else
                    {
                        c1Point = new Point(c1.SimulationCell.transform.position.x, c1YMax);
                        c2Point = new Point(c2XMin, c2.SimulationCell.transform.position.y);
                    }

                    hallways.Add(new Edge(c1Point, leftPoint));
                    hallways.Add(new Edge(leftPoint, c2Point));
                }
                else
                {
                    if (isLeft && isUp)
                    {
                        c1Point = new Point(c1XMin, c1.SimulationCell.transform.position.y);
                        c2Point = new Point(c2.SimulationCell.transform.position.x, c2YMax);
                    }
                    else if (!isLeft && isUp)
                    {
                        c1Point = new Point(c1XMax, c1.SimulationCell.transform.position.y);
                        c2Point = new Point(c2.SimulationCell.transform.position.x, c2YMax);
                    }
                    else if (isLeft && !isUp)
                    {
                        c1Point = new Point(c1XMin, c1.SimulationCell.transform.position.y);
                        c2Point = new Point(c2.SimulationCell.transform.position.x, c2YMin);
                    }
                    else
                    {
                        c1Point = new Point(c1XMax, c1.SimulationCell.transform.position.y);
                        c2Point = new Point(c2.SimulationCell.transform.position.x, c2YMin);
                    }

                    var rightPoint = new Point(c2.SimulationCell.transform.position.x,
                        c1.SimulationCell.transform.position.y);
                    hallways.Add(new Edge(c1Point, rightPoint));
                    hallways.Add(new Edge(rightPoint, c2Point));
                }
            }
        }

        return hallways;
    }

    private void RefillEdges(float edgePercentage)
    {
        var remainingEdges = delaunayGraph.Except(levelGraph);
        foreach (var edge in remainingEdges)
            if (Random.value < edgePercentage)
                levelGraph.Add(edge);
    }

    // Perform Delaunay triangulation on a set of points
    private HashSet<Triangle> Triangulate(HashSet<Point> points)
    {
        var triangulator = new DelaunayTriangulator();
        return triangulator.BowyerWatson(points);
    }

    // Destroy cells that are not part of the triangulation
    private void Cleanup(HashSet<Triangle> triangulation)
    {
        for (var i = Rooms.Count - 1; i >= 0; i--)
            if (!Rooms[i].IsPartOf(triangulation))
            {
                Destroy(Rooms[i].SimulationCell);
                Rooms.RemoveAt(i);
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
        delaunayGraph = new HashSet<Edge>();
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
        for (var i = 0; i < points.Count; i++) forest.MakeSet(i);

        var minimumSpanningTree = new HashSet<Edge>();

        foreach (var edge in sortedEdges)
        {
            var indexP1 = points.FindIndex(p => p.Equals(edge.P1));
            var indexP2 = points.FindIndex(p => p.Equals(edge.P2));

            if (forest.Find(indexP1) != forest.Find(indexP2))
            {
                minimumSpanningTree.Add(edge);
                forest.Union(indexP1, indexP2);
            }
        }

        return minimumSpanningTree;
    }


    // Update simulation cell positions while snapping to TileSize grid
    private void AlignSimulationCell(int i)
    {
        var cell = cells[i];
        var position = cell.PhysicsCell.transform.localPosition;
        var x = RoundNumber(position.x, CELL_SIZE);
        var y = RoundNumber(position.y, CELL_SIZE);

        cell.SimulationCell.transform.localPosition = new Vector2(x, y);
    }

    private List<int> FindOverlaps(Cell cell)
    {
        var overlappingCellIdList = new List<int>();

        for (var i = 0; i < cells.Count; i++)
            if (cells[i] != cell)
                if (cell.Overlaps(cells[i].SimulationCell, 0.001f))
                    overlappingCellIdList.Add(i);

        return overlappingCellIdList;
    }

    public static float RoundNumber(float x, float y)
    {
        return Mathf.Floor((x + y - 1) / y) * y;
    }

    // Creates cells for simulation
    private void CreateCells(int cellCount)
    {
        for (var i = 0; i < cellCount; i++)
        {
            // Generate random width/height within ratio
            int genWidth;
            int genHeight;
            do
            {
                genWidth = Mathf.RoundToInt(RandomGauss(RoomWidthMinimum, RoomWidthMaximum));
                genHeight = Mathf.RoundToInt(RandomGauss(RoomHeightMinimum, RoomHeightMaximum));
            } while (genWidth % 2 != 0
                     || genHeight % 2 != 0);

            // Give random position
            var position = GetRandomPointInElipse(GenerationRegionWidth, GenerationRegionHeight);
            var cell = new Cell(position, genWidth, genHeight);

            cell.CreatePhysicsCellObject(i, cellSprite);
            cell.CreateSimulationCellObject(i, cellSprite);

            cells.Add(cell);
        }
    }

    // Returns a random number
    public static float RandomGauss(float minValue, float maxValue)
    {
        float x;
        float s;
        do
        {
            x = 2 * Random.value - 1;
            var y = 2 * Random.value - 1;
            s = x * x + y * y;
        } while (s >= 1);

        // Standard distribution
        var stdDist = x * Mathf.Sqrt(-2 * Mathf.Log(s) / s);

        // Three sigma rule
        var mean = (minValue + maxValue) / 2;
        var sigma = (maxValue - mean) / 3;
        return Mathf.Clamp(stdDist * sigma + mean, minValue, maxValue);
    }

    // Returns a random point in an elipse
    public static Vector2 GetRandomPointInElipse(float width, float height)
    {
        var t = 2 * Mathf.PI * Random.value;
        var u = Random.value + Random.value;
        float r;

        if (u > 1)
            r = 2 - u;
        else
            r = u;

        return new Vector2(Mathf.Round(width * r * Mathf.Cos(t)), Mathf.Round(height * r * Mathf.Sin(t)));
    }

    // Functions below are for organising and delaying various actions
    private IEnumerator DelaySimulation(float time)
    {
        yield return new WaitForSeconds(time);
        if (isDebug)
        {
            print("Starting simulation...");
            foreach (var cell in cells) cell.SimulationCell.SetActive(true);
            startTime = Time.realtimeSinceStartup;
        }

        startSimulation = true;
    }

    private IEnumerator DelayProcessing(float time)
    {
        yield return new WaitForSeconds(time);
        print("Starting processing...");
        startTime = Time.realtimeSinceStartup;
        startProcessing = true;
    }

    private IEnumerator DelayMapping(float time)
    {
        yield return new WaitForSeconds(time);
        print("Starting mapping...");
        startTime = Time.realtimeSinceStartup;
        startMapping = true;
    }
}