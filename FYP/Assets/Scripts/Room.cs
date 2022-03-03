using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Coord
{
    public int tileX;
    public int tileY;
    public int tileZ;

    public Coord(int x, int y, int z)
    {
        tileX = x;
        tileY = y;
        tileZ = z;
    }

    public static Coord operator +(Coord a, Coord b)
        => new Coord(a.tileX + b.tileX, a.tileY + b.tileY, a.tileZ + b.tileZ);
    public static Coord operator *(Coord a, int b)
        => new Coord(a.tileX * b, a.tileY * b, a.tileZ * b);
}

class Room : IComparable<Room>
{
    public List<Coord> tiles;
    public List<Coord> edgeTiles;
    public List<Room> connectedRooms;
    public int roomSize;
    public bool isAccessibleFromMainRoom;
    public bool isMainRoom;

    public Room()
    {

    }

    public Room(List<Coord> roomTiles, int[,,] map)
    {
        tiles = roomTiles;
        roomSize = tiles.Count;
        connectedRooms = new List<Room>();

        edgeTiles = new List<Coord>();
        foreach (Coord tile in tiles)
        {
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    for (int z = tile.tileY - 1; z <= tile.tileY + 1; z++)
                    {
                        if (x == tile.tileX || y == tile.tileY || z == tile.tileZ)
                        {
                            if (map[x, y, z] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }
    }

    public void SetAccessibleFromMainRoom()
    {
        if (!isAccessibleFromMainRoom)
        {
            isAccessibleFromMainRoom = true;
            foreach (Room connectedRoom in connectedRooms)
            {
                connectedRoom.SetAccessibleFromMainRoom();
            }
        }
    }

    public static void ConnectRoom(Room roomA, Room roomB)
    {
        if (roomA.isAccessibleFromMainRoom)
        {
            roomB.SetAccessibleFromMainRoom();
        }
        else if (roomB.isAccessibleFromMainRoom)
        {
            roomA.SetAccessibleFromMainRoom();
        }
        roomA.connectedRooms.Add(roomB);
        roomB.connectedRooms.Add(roomA);
    }

    public bool IsConnected(Room otherRoom)
    {
        return connectedRooms.Contains(otherRoom);
    }

    public int CompareTo(Room otherRoom)
    {
        return otherRoom.roomSize.CompareTo(roomSize);
    }
}
