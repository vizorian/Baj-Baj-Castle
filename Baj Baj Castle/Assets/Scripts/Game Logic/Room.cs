using System.Collections.Generic;
using System.Linq;
using Actor_Behaviour;
using Enums;
using Game_Logic.Tiles;
using Game_Logic.Triggers;
using Objects;
using Procedural_generation;
using UnityEngine;

namespace Game_Logic;

public class Room
{
    public readonly int Id;
    private readonly TileCreator tileCreator;

    public List<TileData> Tiles;
    public int XMax;
    public int XMin;
    public int YMax;
    public int YMin;
    public List<Room> JointRooms = new List<Room>();
    public List<Room> Neighbours = new List<Room>();
    public TileData Center;

    // Room content
    public List<Actor> Actors = new List<Actor>();
    public List<Interactable> Objects = new List<Interactable>();
    public List<Vector2> DoorPositions = new List<Vector2>();


    // game logic
    public RoomTrigger RoomTrigger;
    public bool IsActive; // activates actors inside the room
    public bool IsCleared = false; // room clear status
    public bool IsTriggered;

    public Room(int id, List<TileData> tiles, TileCreator tileCreator)
    {
        Id = id;
        Tiles = tiles;
        XMax = Tiles.Max(x => x.X);
        YMax = Tiles.Max(x => x.Y);
        XMin = Tiles.Min(x => x.X);
        YMin = Tiles.Min(x => x.Y);
        Center = Tiles.First(x => x.X == (XMax + XMin) / 2 && x.Y == (YMax + YMin) / 2);
        this.tileCreator = tileCreator;
    }

    public List<TileData> DoorTiles
    {
        get { return Tiles.Where(t => t.Type == TileType.Door).ToList(); }
    }

    public int Width => Mathf.Abs(XMax - XMin) - 1;
    public int Height => Mathf.Abs(YMax - YMin) - 1;
    public int Area => Width * Height;
    public float WidthToWorld => (Width + 1) * LevelGenerator.CELL_SIZE;
    public float HeightToWorld => (Height + 1) * LevelGenerator.CELL_SIZE;

    public Vector3 CenterPosition => new Vector3((XMax + XMin) / 2 * LevelGenerator.CELL_SIZE + LevelGenerator.CELL_SIZE / 2,
        (YMax + YMin) / 2 * LevelGenerator.CELL_SIZE + LevelGenerator.CELL_SIZE / 2);

    public Vector3 TopLeftCorner => new Vector3(XMin * LevelGenerator.CELL_SIZE + LevelGenerator.CELL_SIZE,
        YMax * LevelGenerator.CELL_SIZE);

    // Activate room
    public void Trigger()
    {
        IsTriggered = true;

        // Move player from closest door tile towards the center of the room
        MovePlayer();

        // Lock doors
        LockDoors();

        // Activate actors
        IsActive = true;
        foreach (var actor in Actors) actor.IsActive = IsActive;
    }

    // Move player from closest door tile towards the center of the room
    private void MovePlayer()
    {
        var playerObject = GameManager.Instance.Player.gameObject;
        var doorTile = DoorTiles.OrderBy(door =>
            Vector2.Distance(new Vector2(door.X * LevelGenerator.CELL_SIZE, door.Y * LevelGenerator.CELL_SIZE), playerObject.transform.position)).First();
        var direction = (new Vector2(doorTile.X * LevelGenerator.CELL_SIZE, doorTile.Y * LevelGenerator.CELL_SIZE) - (Vector2)CenterPosition).normalized;

        var move = new Vector3(LevelGenerator.CELL_SIZE * 2, 0);
        if (direction.x > 0)
            playerObject.transform.position -= move;
        else if (direction.x < 0)
            playerObject.transform.position += move;

        move = new Vector3(0, LevelGenerator.CELL_SIZE * 2);
        if (direction.y > 0)
            playerObject.transform.position -= move;
        else if (direction.y < 0)
            playerObject.transform.position += move;
    }

    // Lock room doors
    private void LockDoors()
    {
        foreach (var doorTile in DoorTiles) tileCreator.AddToCollisionLayer(doorTile);
    }

    // Unlock room doors
    public void UnlockDoors()
    {
        foreach (var doorTile in DoorTiles) tileCreator.RemoveFromCollisionLayer(doorTile);
    }

    // Find shared wall tiles between two rooms and add shared door
    public void SharesWall(Room otherRoom)
    {
        if (Neighbours.Contains(otherRoom) || otherRoom.Neighbours.Contains(this)) return;
        var sharesWall = false;
        var matchingTiles = new List<TileData>();
        foreach (var tile in Tiles)
            if (otherRoom.Tiles.Contains(tile))
            {
                if (!sharesWall)
                {
                    // Define neighbour rooms
                    Neighbours.Add(otherRoom);
                    otherRoom.Neighbours.Add(this);
                    sharesWall = true;
                }

                matchingTiles.Add(tile);
            }

        if (!sharesWall) return;

        TileData first = null;
        TileData second = null;

        // Add door if it fits
        if (matchingTiles.Count > 3)
        {
            var middle = matchingTiles.Count / 2;
            first = matchingTiles[middle];
            second = matchingTiles[middle - 1];
            DoorPositions.Add(new Vector2(first.X, first.Y));
            DoorPositions.Add(new Vector2(second.X, second.Y));
        }

        // Take over shared tiles
        foreach (var tile in matchingTiles)
            if (first != null)
                if (Equals(tile, first) || Equals(tile, second))
                {
                    // TODO make sure removal does not cause problems
                    otherRoom.Tiles.Remove(tile);
                    Tiles.Remove(tile);
                    tile.Type = TileType.Door;
                    otherRoom.Tiles.Add(tile);
                    Tiles.Add(tile);
                }
    }

    // Find nearby wall tiles between two rooms and merge them
    public void WallNearby(Room otherRoom)
    {
        if (JointRooms.Contains(otherRoom) || otherRoom.JointRooms.Contains(this)) return;

        // Find any nearby wall tile
        TileData closeTile = null;
        var otherRoomOuterTiles = otherRoom.Tiles.Where(t =>
            t.X == otherRoom.XMin || t.X == otherRoom.XMax || t.Y == otherRoom.YMax || t.Y == otherRoom.YMin).ToList();
        foreach (var tile in Tiles)
            if (otherRoomOuterTiles.Any(t => t.X == tile.X + 1 && t.Y == tile.Y))
            {
                closeTile = new TileData(tile.X + 1, tile.Y, TileType.None);
                break;
            }
            else if (otherRoomOuterTiles.Any(t => t.X == tile.X - 1 && t.Y == tile.Y))
            {
                closeTile = new TileData(tile.X - 1, tile.Y, TileType.None);
                break;
            }
            else if (otherRoomOuterTiles.Any(t => t.X == tile.X && t.Y == tile.Y + 1))
            {
                closeTile = new TileData(tile.X, tile.Y + 1, TileType.None);
                break;
            }
            else if (otherRoomOuterTiles.Any(t => t.X == tile.X && t.Y == tile.Y - 1))
            {
                closeTile = new TileData(tile.X, tile.Y - 1, TileType.None);
                break;
            }

        // If none found, return
        if (closeTile == null) return;

        // Define joined rooms
        JointRooms.Add(otherRoom);
        otherRoom.JointRooms.Add(this);

        // Update room size
        if (closeTile.X > XMax)
        {
            XMax++;
            for (var i = YMin; i <= YMax; i++) Tiles.Add(new TileData(XMax, i, TileType.None));
        }
        else if (closeTile.X < XMin)
        {
            XMin--;
            for (var i = YMin; i <= YMax; i++) Tiles.Add(new TileData(XMin, i, TileType.None));
        }
        else if (closeTile.Y > YMax)
        {
            YMax++;
            for (var i = XMin; i <= XMax; i++) Tiles.Add(new TileData(i, YMax, TileType.None));
        }
        else if (closeTile.Y < YMin)
        {
            YMin--;
            for (var i = XMin; i <= XMax; i++) Tiles.Add(new TileData(i, YMin, TileType.None));
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;

        if (obj.GetType() != GetType()) return false;

        var other = (Room)obj;
        return Id == other.Id;
    }

    // Get random tile within certain distance from borders
    public Vector3 GetRandomTile(int distanceFromWall)
    {
        Vector3 position;
        var offset = LevelGenerator.CELL_SIZE / 2;
        var tiles = Tiles.Where(t =>
            t.X > XMin + distanceFromWall && t.X < XMax - distanceFromWall && t.Y > YMin + distanceFromWall &&
            t.Y < YMax - distanceFromWall).ToList();
        tiles = tiles.Where(t => t.Type != TileType.Wall && t.Type != TileType.Door).ToList();
        do
        {
            var randomTile = tiles[Random.Range(0, tiles.Count)];
            position = new Vector3(randomTile.X * LevelGenerator.CELL_SIZE + offset,
                randomTile.Y * LevelGenerator.CELL_SIZE + offset);
        } // check if list of Actors or Objects contains anything at this position
        while (Actors.Any(a => a.transform.position == position) || Objects.Any(o => o.transform.position == position));

        return position;
    }

    public override int GetHashCode()
    {
        return Id;
    }
}