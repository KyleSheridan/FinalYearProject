using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class MapGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct ParticleParams
    {
        public int length;
        [Range(0.001f, 1)]
        public float H;
        public int stepLength;
        public int radius;
        public int edgeSize;
        [Range(0, 100)]
        public int fillPercent;
    }
    
    public int width;
    public int height;
    public int depth;

    public float edgeSize = 1;

    public int iterations = 5;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    [Range(1, 10)]
    public int passageRadius = 1;

    public bool onlyOneRoom = true;
    public bool connectRooms = false;

    public int[,,] map { get; private set; }

    public ParticleParams[] patricles;

    MarchingCubes meshGen;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Profiler.BeginSample("MapGeneration");
            GenerateMap();
            Profiler.EndSample();
        }
    }

    public void ClearMap()
    {
        if(map != null)
        {
            Array.Clear(map, 0, map.Length);
            map = null;
            meshGen = GetComponent<MarchingCubes>();
            meshGen.ClearMesh();

            Debug.Log("Map cleared.");
        }
    }

    public void GenerateMap()
    {
        map = new int[width, height, depth];
        RandomFillMap();

        GenerateParticles();

        for(int i = 0; i < iterations; i++)
        {
            SmoothMap();
        }

        ProcessMap();

        meshGen = GetComponent<MarchingCubes>();
        meshGen.GenerateMesh(map, 1);

        Debug.Log("Map generated.");
    }

    void GenerateParticles()
    {
        BrownianMotion bm = GetComponent<BrownianMotion>();

        foreach(ParticleParams p in patricles)
        {
            Particle particle = bm.GenerateParticle(p.length, p.H);

            List<Coord> path = particle.GeneratePath(width, height, depth, p.stepLength, p.edgeSize);

            FBMAffect(path, p.radius, p.fillPercent);

            DrawDebugParticleLine(path);
        }
    }

    void DrawDebugParticleLine(List<Coord> path)
    {
        for (int i = 1; i < path.Count; i++)
        {
            Debug.DrawLine(CoordToWorldPoint(path[i-1]),
                           CoordToWorldPoint(path[i]),
                           Color.magenta,
                           10
                           );
        }
    }

    void FBMAffect(List<Coord> path, int r, int drawPercent)
    {
        foreach (Coord c in path)
        {
            if (!IsInMapRange(c.tileX, c.tileY, c.tileZ)) { return; }

            DrawSphere(c, r, drawPercent);
        }
    }

    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(1);
        int wallThresholdSize = 150;

        foreach(List<Coord> wallRegion in wallRegions)
        {
            if(wallRegion.Count < wallThresholdSize)
            {
                foreach(Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY, tile.tileZ] = 0;
                }
            }
        }
        List<List<Coord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 50;
        List<Room> remainingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY, tile.tileZ] = 1;
                }
            }
            else
            {
                remainingRooms.Add(new Room(roomRegion, map));
            }
        }

        if(remainingRooms.Count <= 0) { return; }

        remainingRooms.Sort();
        remainingRooms[0].isMainRoom = true;
        remainingRooms[0].isAccessibleFromMainRoom = true;

        if(onlyOneRoom)
        {
            RemoveNotConnectedRooms(remainingRooms);
        }
        else if(connectRooms)
        {
            ConnectClosestRooms(remainingRooms);
        }
    }

    void RemoveNotConnectedRooms(List<Room> allRooms)
    {
        foreach(Room room in allRooms)
        {
            if(!room.isMainRoom)
            {
                foreach(Coord tile in room.tiles)
                {
                    map[tile.tileX, tile.tileY, tile.tileZ] = 1;
                }
            }
        }
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach(Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDist = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();

        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach(Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if(roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }
            foreach (Room roomB in roomListB)
            {
                if(roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                Coord tileA = roomA.edgeNodes.FindClosest(roomB.edgeNodes.pos) as Coord;
                Coord tileB = roomB.edgeNodes.FindClosest(roomA.edgeNodes.pos) as Coord;

                int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2)
                                               + Mathf.Pow(tileA.tileY - tileB.tileY, 2)
                                               + Mathf.Pow(tileA.tileZ - tileB.tileZ, 2)
                                               );

                if (distanceBetweenRooms < bestDist || !possibleConnectionFound)
                {
                    bestDist = distanceBetweenRooms;
                    possibleConnectionFound = true;
                    bestTileA = tileA;
                    bestTileB = tileB;
                    bestRoomA = roomA;
                    bestRoomB = roomB;
                }
            }

            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }
        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRoom(roomA, roomB);
        Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 100);

        List<Coord> line = GetLine(tileA, tileB);

        foreach(Coord c in line)
        {
            DrawSphere(c, passageRadius, 100);
        }
    }

    void DrawSphere(Coord c, int r, int drawPercent)
    {
        for(int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                for (int z = -r; z <= r; z++)
                {
                    if ((x * x) + (y * y) + (z * z) <= (r * r))
                    {
                        int drawX = c.tileX + x;
                        int drawY = c.tileY + y;
                        int drawZ = c.tileZ + z;
                        if (IsInMapRange(drawX, drawY, drawZ) && !IsEdge(drawX, drawY, drawZ))
                        {
                            map[drawX, drawY, drawZ] = (UnityEngine.Random.Range(0, 100) < drawPercent) ? 0 : 1;
                        }
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;
        int z = from.tileZ;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;
        int dz = to.tileZ - from.tileZ;
        
        //dy / dx
        bool invertedY = false;
        int stepY = Math.Sign(dx);
        int gradientStepY = Math.Sign(dy);

        int longestY = Mathf.Abs(dx);
        int shortestY = Mathf.Abs(dy);


        if (longestY < shortestY)
        {
            invertedY = true;
            longestY = Mathf.Abs(dy);
            shortestY = Mathf.Abs(dx);

            stepY = Math.Sign(dy);
            gradientStepY = Math.Sign(dx);
        }

        //dz / dx
        bool invertedZ = false;
        int stepZ = Math.Sign(dx);
        int gradientStepZ = Math.Sign(dz);

        int longestZ = Mathf.Abs(dx);
        int shortestZ = Mathf.Abs(dz);

        if (longestZ < shortestZ)
        {
            invertedZ = true;
            longestZ = Mathf.Abs(dz);
            shortestZ = Mathf.Abs(dx);

            longestY = Mathf.Abs(dz);

            stepZ = Math.Sign(dz);
            gradientStepZ = Math.Sign(dx);
        }

        if (invertedY)
        {
            invertedZ = false;
            stepZ = Math.Sign(dy);
            gradientStepZ = Math.Sign(dz);

            longestZ = Mathf.Abs(dy);
            shortestZ = Mathf.Abs(dz);

            longestY = Mathf.Abs(dy);

            if (longestZ < shortestZ)
            {
                invertedZ = true;
                longestZ = Mathf.Abs(dz);
                shortestZ = Mathf.Abs(dy);

                stepZ = Math.Sign(dz);
                gradientStepZ = Math.Sign(dy);
            }
        }

        int gradientAccumulationY = longestY / 2;
        int gradientAccumulationZ = longestZ / 2;

        int longest = longestY;

        if (invertedZ)
        {
            longest = longestZ;
        }

        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y, z));

            if (invertedZ)
            {
                z += stepZ;
            }
            else if (invertedY)
            {
                y += stepY;
            }
            else
            {
                x += stepY;
            }

            gradientAccumulationY += shortestY;
            if(gradientAccumulationY >= longestY)
            {
                if (invertedY)
                {
                    x += gradientStepY;
                }
                else
                {
                    y += gradientStepY;
                }
                gradientAccumulationY -= longestY;
            }
            
            gradientAccumulationZ += shortestZ;
            if(gradientAccumulationZ >= longestZ)
            {
                if (invertedZ)
                {
                    if (invertedY)
                    {
                        y += gradientStepZ;
                    }
                    else
                    {
                        x += gradientStepZ;
                    }
                }
                else
                {
                    z += gradientStepZ;
                }
                gradientAccumulationZ -= longestZ;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, -height / 2 + .5f + tile.tileY, -depth / 2 + .5f + tile.tileZ);
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,,] mapFlags = new int[width, height, depth];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (mapFlags[x, y, z] == 0 && map[x, y, z] == tileType)
                    {
                        List<Coord> newRegion = GetRegionTiles(x, y, z);
                        regions.Add(newRegion);

                        foreach (Coord tile in newRegion)
                        {
                            mapFlags[tile.tileX, tile.tileY, tile.tileZ] = 1;
                        }
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY, int startZ)
    {
        List<Coord> tiles = new List<Coord>();
        int[,,] mapFlags = new int[width, height, depth];
        int tileType = map[startX, startY, startZ];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY, startZ));
        mapFlags[startX, startY, startZ] = 1;

        while(queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    for (int z = tile.tileZ - 1; z <= tile.tileZ + 1; z++)
                    {
                        if (IsInMapRange(x, y, z) && (z == tile.tileZ || y == tile.tileY || x == tile.tileX))
                        {
                            if (mapFlags[x, y, z] == 0 && map[x, y, z] == tileType)
                            {
                                mapFlags[x, y, z] = 1;
                                queue.Enqueue(new Coord(x, y, z));
                            }
                        }
                    }
                }
            }
        }

        return tiles;
    }

    bool IsInMapRange(int x, int y, int z)
    {
        bool isInMap = (x >= 0
                        && x < width
                        && y >= 0
                        && y < height
                        && z >= 0
                        && z < depth
                        );

        return isInMap;
    }

    bool IsEdge(int x, int y, int z)
    {
        bool isEdge = (x < edgeSize
                        || x >= width - edgeSize
                        || y < edgeSize
                        || y >= height - edgeSize
                        || z < edgeSize
                        || z >= depth - edgeSize
                        );

        return isEdge;
    }

    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        UnityEngine.Random.InitState(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (IsEdge(x, y, z))
                    {
                        map[x, y, z] = 1;
                    }
                    else
                    {
                        map[x, y, z] = (UnityEngine.Random.Range(0, 100) < randomFillPercent) ? 1 : 0;
                    }
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    int neighbourWallTiles = GetSurroundingWallCount(x, y, z);

                    if (neighbourWallTiles > 14)
                        map[x, y, z] = 1;
                    if (neighbourWallTiles < 14)
                        map[x, y, z] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY, int gridZ)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                for (int neighbourZ = gridZ - 1; neighbourZ <= gridZ + 1; neighbourZ++)
                {
                    if (IsInMapRange(neighbourX, neighbourY, neighbourZ))
                    {
                        if (neighbourX != gridX || neighbourY != gridY || neighbourZ != gridZ)
                        {
                            wallCount += map[neighbourX, neighbourY, neighbourZ];
                        }
                    }
                    else
                    {
                        wallCount++;
                    }
                }
            }
        }

        return wallCount;
    }
}
