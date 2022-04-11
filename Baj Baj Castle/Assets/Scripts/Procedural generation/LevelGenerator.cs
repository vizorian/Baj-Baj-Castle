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
    private readonly List<Cell> roomCells = new List<Cell>();
    private List<Cell> hallwayCells = new List<Cell>();
    private readonly HashSet<Edge> delaunayGraph = new HashSet<Edge>();
    private HashSet<Edge> levelGraph;
    

    public static float cellSize;
    private int simulationLoops = 0;

    private bool startSimulation = false;
    private bool startProcessing = false;
    private bool startMapping = false;
    private bool isSimulated = false;
    private bool isProcessed = false;
    private bool isMapped = false;
    private float startTime;
    private float endTime;
    public float EdgePercentage = 0.1f;

    public int HallwayWidth = 2;

    void Start()
    {
        cellSize = TileSize * PIXEL_SIZE;
        CreateCells(Complexity);
        DrawGrid();
        print("Starting level generation process...");
        StartCoroutine(DelaySimulation(SimulationDelay));
    }

    void FixedUpdate()
    {
        if (!isSimulated && startSimulation)
        {
            SimulateCells();
        }

        if (isSimulated && !isProcessed && startProcessing)
        {
            ProcessCells();
        }

        if(isProcessed && !isMapped && startMapping)
        {
            MapCells();
        }
    }

    private void ProcessCells()
    {
        // filter out cells that qualify to be rooms
        startTime = Time.realtimeSinceStartup;
        FilterCells();
        endTime = Time.realtimeSinceStartup;
        print("Cell filtering took " + (endTime - startTime) + " seconds.");

        Graph();

        isProcessed = true;
        StartCoroutine(DelayMapping(SimulationDelay * 2));
    }
    
    private void MapCells()
    {
        // calculate hallways between cells
        startTime = Time.realtimeSinceStartup;;
        var hallways = CalculateHallways();
        endTime = Time.realtimeSinceStartup;
        print("Hallway calculation took " + (endTime - startTime) + " seconds.");
        print("Found " + hallways.Count + " hallways.");
        DrawEdges(hallways, Color.cyan, 100f);

        // carve out hallways
        startTime = Time.realtimeSinceStartup;
        var hallwayPoints = GetHallwayPoints(hallways);

        CreateHallways(hallwayPoints);
        endTime = Time.realtimeSinceStartup;
        print("Hallway creation took " + (endTime - startTime) + " seconds.");

        isMapped = true;
    }

    private void CreateHallways(HashSet<Vector2> positions)
    {
        foreach(var position in positions)
        {
            var cell = new Cell(position, 1, 1);
            cell.CreateDisplayCellObject(CellSprite);
            hallwayCells.Add(cell);
        }
    }

    // Creates hallways between rooms based on hallway edges, removing excess ones
    private HashSet<Vector2> GetHallwayPoints(HashSet<Edge> hallways)
    {
        HashSet<Vector2> hallwayPoints = new HashSet<Vector2>();

        foreach (var hallway in hallways)
        {
            double from;
            double to;
            if(hallway.P1.X == hallway.P2.X) // hallway is vertical
            {
                if(hallway.P1.Y < hallway.P2.Y)
                {
                    from = hallway.P1.Y;
                    to = hallway.P2.Y;
                }else
                {
                    from = hallway.P2.Y;
                    to = hallway.P1.Y;
                }

                for (var y = from; y < to; y += cellSize)
                {
                    bool isOdd = false;
                    int odds = 0;
                    int evens = 0;
                    for(var i = 0; i < HallwayWidth; i++)
                    {
                        int positionX;
                        var positionY = Mathf.RoundToInt((float)(y / cellSize / PIXEL_SIZE));
                        if(isOdd)
                        {
                            positionX = Mathf.RoundToInt((float)(hallway.P1.X + ((i - evens + 1) * cellSize)) / cellSize / PIXEL_SIZE);
                            isOdd = false;
                            odds++;
                        }else
                        {
                            positionX = Mathf.RoundToInt((float)(hallway.P1.X + ((i - odds) * cellSize)) / cellSize / PIXEL_SIZE);
                            isOdd = true;
                            evens++;
                        }
                        hallwayPoints.Add(new Vector2(positionX, positionY));
                    }
                }
            }
            else // hallway is horizontal
            {
                if (hallway.P1.X < hallway.P2.X)
                {
                    from = hallway.P1.X;
                    to = hallway.P2.X;
                }
                else
                {
                    from = hallway.P2.X;
                    to = hallway.P1.X;
                }
                
                for (var x = from; x < to; x += cellSize)
                {
                    bool isOdd = false;
                    int odds = 0;
                    int evens = 0;
                    for (var i = 0; i < HallwayWidth; i++)
                    {
                        var positionX = Mathf.RoundToInt((float)(x / cellSize / PIXEL_SIZE));
                        int positionY;
                        if (isOdd)
                        {
                            positionY = Mathf.RoundToInt((float)(hallway.P1.Y + ((i - evens + 1) * cellSize)) / cellSize / PIXEL_SIZE);
                            isOdd = false;
                            odds++;
                        }
                        else
                        {
                            positionY = Mathf.RoundToInt((float)(hallway.P1.Y + ((i - odds) * cellSize)) / cellSize / PIXEL_SIZE);
                            isOdd = true;
                            evens++;
                        }
                        hallwayPoints.Add(new Vector2(positionX, positionY));
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
        var offset = cellSize * (HallwayWidth / 4 + 1);
        
        var hallways = new HashSet<Edge>();
        foreach (var edge in levelGraph){

            // find cells that are connected by an edge
            var c1 = roomCells.First(c => c.SimulationCell.transform.position.x == edge.P1.X
                                              && c.SimulationCell.transform.position.y == edge.P1.Y);
            var c2 = roomCells.First(c => c.SimulationCell.transform.position.x == edge.P2.X
                                              && c.SimulationCell.transform.position.y == edge.P2.Y);

            // calculate midpoint between the two cells
            var midpoint = new Point((edge.P1.X + edge.P2.X) / 2, (edge.P1.Y + edge.P2.Y) / 2);

            // calculate various offsets and extremes
            var c1offsetX = c1.SimulationCell.transform.localScale.x / 2;
            var c1offsetY = c1.SimulationCell.transform.localScale.y / 2;

            var c2offsetX = c2.SimulationCell.transform.localScale.x / 2;
            var c2offsetY = c2.SimulationCell.transform.localScale.y / 2; 

            var c1XMax = c1.SimulationCell.transform.position.x + c1offsetX;
            var c1XMin = c1.SimulationCell.transform.position.x - c1offsetX;
            var c1YMax = c1.SimulationCell.transform.position.y + c1offsetY;
            var c1YMin = c1.SimulationCell.transform.position.y - c1offsetY;

            var c2XMax = c2.SimulationCell.transform.position.x + c2offsetX;
            var c2XMin = c2.SimulationCell.transform.position.x - c2offsetX;
            var c2YMax = c2.SimulationCell.transform.position.y + c2offsetY;
            var c2YMin = c2.SimulationCell.transform.position.y - c2offsetY;

            var isFound = false;

            bool isLeft = c1.SimulationCell.transform.position.x > c2.SimulationCell.transform.position.x;
            bool isUp = c1.SimulationCell.transform.position.y > c2.SimulationCell.transform.position.y;

            if(isLeft)
            {
                if(midpoint.X > c1XMin + offset
                   && midpoint.X < c1XMax - offset
                   && midpoint.X > c2XMin + offset
                   && midpoint.X < c2XMax - offset){
                    if(isUp){
                        hallways.Add(new Edge(new Point(midpoint.X, c1YMin), new Point(midpoint.X, c2YMax)));
                    }else{
                        hallways.Add(new Edge(new Point(midpoint.X, c1YMax), new Point(midpoint.X, c2YMin)));
                    }
                    isFound = true;
                }
            }
            else{
                if(midpoint.X > c2XMin + offset
                   && midpoint.X < c2XMax - offset
                   && midpoint.X > c1XMin + offset
                   && midpoint.X < c1XMax - offset){
                    if(isUp){
                        hallways.Add(new Edge(new Point(midpoint.X, c1YMin), new Point(midpoint.X, c2YMax)));
                    }else{
                        hallways.Add(new Edge(new Point(midpoint.X, c1YMax), new Point(midpoint.X, c2YMin)));
                    }
                    isFound = true;
                }
            }

            if(!isFound){
                if(isUp)
                {
                    if(midpoint.Y > c1YMin + offset
                       && midpoint.Y < c1YMax - offset
                       && midpoint.Y > c2YMin + offset
                       && midpoint.Y < c2YMax - offset){
                        if(isLeft){
                            hallways.Add(new Edge(new Point(c1XMin, midpoint.Y), new Point(c2XMax, midpoint.Y)));
                        }else{
                            hallways.Add(new Edge(new Point(c1XMax, midpoint.Y), new Point(c2XMin, midpoint.Y)));
                        }
                        isFound = true;
                    }
                }
                else{
                    if(midpoint.Y > c2YMin + offset
                       && midpoint.Y < c2YMax - offset
                       && midpoint.Y > c1YMin + offset
                       && midpoint.Y < c1YMax - offset){
                        if(isLeft){
                            hallways.Add(new Edge(new Point(c1XMin, midpoint.Y), new Point(c2XMax, midpoint.Y)));
                        }else{
                            hallways.Add(new Edge(new Point(c1XMax, midpoint.Y), new Point(c2XMin, midpoint.Y)));
                        }
                        isFound = true;
                    }
                }
            }

            // if a straight hallway isn't possible, create a L shaped hallway
            if(!isFound){
                Point c1Point;
                Point c2Point;

                var leftPoint = new Point(c1.SimulationCell.transform.position.x, c2.SimulationCell.transform.position.y);

                // TODO: improve this
                // check if point is within any of the cells
                if(!roomCells.Any(c => c.IsPointInside(leftPoint))){
                    if(isLeft && isUp)
                    {
                        c1Point = new Point(c1.SimulationCell.transform.position.x, c1YMin);
                        c2Point = new Point(c2XMax, c2.SimulationCell.transform.position.y);
                    }else if(!isLeft && isUp)
                    {
                        c1Point = new Point(c1.SimulationCell.transform.position.x, c1YMin);
                        c2Point = new Point(c2XMin, c2.SimulationCell.transform.position.y);
                    }else if(isLeft && !isUp)
                    {
                        c1Point = new Point(c1.SimulationCell.transform.position.x, c1YMax);
                        c2Point = new Point(c2XMax, c2.SimulationCell.transform.position.y);
                    }else{
                        c1Point = new Point(c1.SimulationCell.transform.position.x, c1YMax);
                        c2Point = new Point(c2XMin, c2.SimulationCell.transform.position.y);
                    }
                    hallways.Add(new Edge(c1Point, leftPoint));
                    hallways.Add(new Edge(leftPoint, c2Point));
                }
                else{
                    if(isLeft && isUp)
                    {
                        c1Point = new Point(c1XMin, c1.SimulationCell.transform.position.y);
                        c2Point = new Point(c2.SimulationCell.transform.position.x, c2YMax);
                    }else if(!isLeft && isUp)
                    {
                        c1Point = new Point(c1XMax, c1.SimulationCell.transform.position.y);
                        c2Point = new Point(c2.SimulationCell.transform.position.x, c2YMax);
                    }else if(isLeft && !isUp)
                    {
                        c1Point = new Point(c1XMin, c1.SimulationCell.transform.position.y);
                        c2Point = new Point(c2.SimulationCell.transform.position.x, c2YMin);
                    }else
                    {
                        c1Point = new Point(c1XMax, c1.SimulationCell.transform.position.y);
                        c2Point = new Point(c2.SimulationCell.transform.position.x, c2YMin);
                    }

                    var rightPoint = new Point(c2.SimulationCell.transform.position.x, c1.SimulationCell.transform.position.y);
                    hallways.Add(new Edge(c1Point, rightPoint));
                    hallways.Add(new Edge(rightPoint, c2Point));
                }
            }
        }
        return hallways;
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
        foreach (var cell in roomCells)
        {
            points.Add(new Point(cell.SimulationCell.transform.position.x, cell.SimulationCell.transform.position.y));
        }

        // perform triangulation
        startTime = Time.realtimeSinceStartup;
        var triangulation = Triangulate(points);
        endTime = Time.realtimeSinceStartup;
        print("Cell triangulation took " + (endTime - startTime) + " seconds.");
        DrawTriangles(triangulation);
        
        Cleanup(triangulation);

        // get a minimum spanning tree from triangulation results
        startTime = Time.realtimeSinceStartup;
        levelGraph = MinimumSpanningTree(triangulation, points.ToList());
        endTime = Time.realtimeSinceStartup;
        print("Minimum spanning tree calculation took " + (endTime - startTime) + " seconds.");
        DrawEdges(levelGraph, Color.green, 3f);

        // add some edges from delaunay graph back to graph
        RefillEdges(EdgePercentage);
        DrawEdges(levelGraph, Color.green, 10f);
    }

    private void RefillEdges(float edgePercentage)
    {
        var remainingEdges = delaunayGraph.Except(levelGraph);
        foreach (var edge in remainingEdges)
        {
            if (Random.value < edgePercentage)
            {
                levelGraph.Add(edge);
            }
        }
    }

    // Perform Delaunay triangulation on a set of points
    private HashSet<Triangle> Triangulate(HashSet<Point> points)
    {
        var triangulator = new DelaunayTriangulator();
        triangulator.CreateSupraTriangle(roomCells);
        triangulator.AdjustSupraTriangle(points);
        return triangulator.BowyerWatson(points);
    }

    // Destroy cells that are not part of the triangulation
    private void Cleanup(HashSet<Triangle> triangulation)
    {
        for (int i = roomCells.Count - 1; i >= 0; i--)
        {
            if (!roomCells[i].IsPartOf(triangulation))
            {
                Destroy(roomCells[i].SimulationCell);
                roomCells.RemoveAt(i);
            }
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
                roomCells.Add(cell);
            }
        }

        // destroy all simulation cells that are not part of suitableCells
        foreach (var cell in cells)
        {
            if (!roomCells.Contains(cell))
            {
                Destroy(cell.SimulationCell);
            }
        }

        cells.Clear();
    }

    private void SimulateCells()
    {
        int simulatedCellsCount = 0;

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
                if(cell.IsOverlapping(cells[i].SimulationCell, 0.001f))
                {
                    overlappingCellIdList.Add(i);
                }
            }
        }

        return overlappingCellIdList;
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
            while (genWidth % 2 != 0
                   || genHeight % 2 != 0);

            // Give random position
            Vector2 position = GetRandomPointInElipse(GenerationRegionWidth, GenerationRegionHeight);
            Cell cell = new Cell(position, genWidth * TileSize, genHeight * TileSize);

            cell.CreatePhysicsCellObject(i, CellSprite);
            cell.CreateSimulationCellObject(i, CellSprite);

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
        var t = 2 * Mathf.PI * Random.value;
        var u = Random.value + Random.value;
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

    IEnumerator DelayMapping(float time)
    {
        yield return new WaitForSeconds(time);
        print("Starting mapping...");
        startTime = Time.realtimeSinceStartup;
        startMapping = true;
    }
}

