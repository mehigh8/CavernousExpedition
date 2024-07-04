using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapGenerator
{
    public class Tunnel
    {
        public Vector2Int start;
        public Vector2Int end;

        public Tunnel(Vector2Int start, Vector2Int end)
        {
            this.start = start;
            this.end = end;
        }

        public Vector2 GetTunnelCenter()
        {
            return new Vector2(start.x + end.x, start.y + end.y) / 2;
        }
    }

    public class Region : System.IComparable<Region>
    {
        public List<Vector2Int> points;
        public HashSet<Vector2Int> edgePoints;
        public List<Region> connectedRegions;
        public List<Tunnel> tunnels;
        public bool isMainRegion;
        public bool accessibleFromMainRegion;

        public Region()
        {
            points = new List<Vector2Int>();
            edgePoints = new HashSet<Vector2Int>();
            connectedRegions = new List<Region>();
            tunnels = new List<Tunnel>();
        }

        public void AddPoint(Vector2Int point)
        {
            points.Add(point);
        }

        public void AddTunnel(Tunnel tunnel)
        {
            tunnels.Add(tunnel);
        }

        public void ComputeEdgePoints(int[,] map)
        {
            foreach (Vector2Int point in points)
                for (int i = 0; i < Helpers.neswX.Length; i++)
                {
                    Vector2Int neigh = new Vector2Int(point.x + Helpers.neswX[i], point.y + Helpers.neswY[i]);
                    if (map[neigh.x, neigh.y] == -1)
                    {
                        edgePoints.Add(point);
                        break;
                    }
                }
        }

        public void CheckAccessibility()
        {
            if (!accessibleFromMainRegion)
            {
                accessibleFromMainRegion = true;
                foreach (Region region in connectedRegions)
                    region.CheckAccessibility();
            }
        }

        public void ConnectToRegion(Region otherRegion)
        {
            if (accessibleFromMainRegion)
                otherRegion.CheckAccessibility();
            else if (otherRegion.accessibleFromMainRegion)
                CheckAccessibility();


            connectedRegions.Add(otherRegion);
            otherRegion.connectedRegions.Add(this);
        }

        public bool IsConnected(Region otherRegion)
        {
            return connectedRegions.Contains(otherRegion);
        }

        public int CompareTo(Region otherRegion)
        {
            return points.Count.CompareTo(otherRegion.points.Count);
        }
    }

    private int seed;
    private Vector2Int dimensions;
    private int[,] mapTexture;
    private List<Region> caves;
    private List<Region> walls;
    private int caveSizeThreshold;
    private float tunnelSize;

    public MapGenerator(string seed, Vector2Int dimensions, int caveSizeThreshold, float tunnelSize)
    {
        this.seed = seed.GetHashCode();
        Random.InitState(this.seed);

        this.dimensions = dimensions;
        this.caveSizeThreshold = caveSizeThreshold;
        this.tunnelSize = tunnelSize;

        mapTexture = new int[dimensions.x, dimensions.y];
    }

    public int[,] GetMap() { return mapTexture; }

    public float[,] GenerateNoiseMap(int seed, float noisiness)
    {
        Vector2 floatDim = new Vector2(dimensions.x, dimensions.y);
        float[,] noiseMap = new float[dimensions.x, dimensions.y];

        for (float i = 0.0F; i < floatDim.x; i++)
        {
            for (float j = 0.0F; j < floatDim.y; j++)
            {
                float xCoord = seed + i / floatDim.x * noisiness;
                float yCoord = seed + j / floatDim.y * noisiness;
                noiseMap[(int)i, (int)j] = Mathf.PerlinNoise(xCoord, yCoord);
            }
        }

        return noiseMap;
    }

    public Vector2Int GetLeftMostTile()
    {
        Vector2Int result = new Vector2Int(int.MaxValue, 0);
        for (int i = 0; i < dimensions.x; i++)
        {
            for (int j = 0; j < dimensions.y; j++)
            {
                if (mapTexture[i, j] == 1 && i < result.x)
                    result = new Vector2Int(i, j);
            }
        }

        return result;
    }

    public void GenerateMap(int steps, int fillPercentage, int neighborThreshold)
    {
        // Generate random map
        for (int i = 0; i < dimensions.x; i++)
            for (int j = 0; j < dimensions.y; j++)
                if (i == 0 || i == dimensions.x - 1 || j == 0 || j == dimensions.y - 1)
                    mapTexture[i, j] = -1;
                else
                    mapTexture[i, j] = Random.Range(0, 100) < fillPercentage ? 1 : -1;

        // Apply cellular automata rules
        for (int i = 0; i < steps; i++)
            ApplyRules(neighborThreshold);

        // Eliminate small walls
        walls = determineRegions(-1);
        filterRegions(caveSizeThreshold, walls, -1);

        // Eliminate small caves
        caves = determineRegions(1);
        filterRegions(caveSizeThreshold, caves, 1);

        // Calculate edges for each cave
        foreach (Region cave in caves)
            cave.ComputeEdgePoints(mapTexture);

        // Sort caves to determine main region (biggest one)
        caves.Sort();
        caves[0].isMainRegion = true;
        caves[0].accessibleFromMainRegion = true;

        // Connect the caves
        ConnectClosestRegions(caves);
    }

    private void ConnectClosestRegions(List<Region> regions, bool forceAccess = false)
    {
        List<Region> unaccessibleRegions = new List<Region>();
        List<Region> accessibleRegions = new List<Region>();

        if (forceAccess)
        {
            foreach (Region region in regions)
                if (region.accessibleFromMainRegion)
                    accessibleRegions.Add(region);
                else
                    unaccessibleRegions.Add(region);
        }
        else
        {
            accessibleRegions = regions;
            unaccessibleRegions = regions;
        }

        float bestDistance = 0;
        bool foundNewConnection = false;
        Region bestRegA = null;
        Region bestRegB = null;
        Vector2Int bestPointA = new Vector2Int();
        Vector2Int bestPointB = new Vector2Int();

        foreach (Region regionA in unaccessibleRegions)
        {
            if (!forceAccess)
            {
                foundNewConnection = false;

                if (regionA.connectedRegions.Count > 0)
                    continue;
            }

            foreach (Region regionB in accessibleRegions)
            {
                if (regionA == regionB || regionA.IsConnected(regionB))
                    continue;

                foreach (Vector2Int pointA in regionA.edgePoints)
                {
                    foreach (Vector2Int pointB in regionB.edgePoints)
                    {
                        float distance = Vector2Int.Distance(pointA, pointB);

                        if (!foundNewConnection || distance < bestDistance)
                        {
                            bestRegA = regionA;
                            bestRegB = regionB;
                            bestDistance = distance;
                            foundNewConnection = true;
                            bestPointA = pointA;
                            bestPointB = pointB;
                        }
                    }
                }
            }

            if (foundNewConnection && !forceAccess)
                CreateTunnel(bestRegA, bestPointA, bestRegB, bestPointB);
        }

        if (foundNewConnection && forceAccess)
        {
            CreateTunnel(bestRegA, bestPointA, bestRegB, bestPointB);
            ConnectClosestRegions(regions, true);
        }

        if (!forceAccess)
            ConnectClosestRegions(regions, true);

    }

    private void CreateTunnel(Region regionA, Vector2Int pointA, Region regionB, Vector2Int pointB)
    {
        regionA.ConnectToRegion(regionB);

        if (pointA == pointB)
        {
            for (int x = 0; x < dimensions.x; x++)
                for (int y = 0; y < dimensions.y; y++)
                    if (Vector2.Distance(new Vector2(x, y), pointA) < tunnelSize)
                        mapTexture[x, y] = 1;
        }


        int circleCount = Mathf.CeilToInt((pointB - pointA).magnitude);

        Vector2 pointAf = new Vector2(pointA.x, pointA.y);
        Vector2 pointBf = new Vector2(pointB.x, pointB.y);
        Vector2 incrementVector = (pointBf - pointAf) / circleCount;

        for (int i = 0; i < circleCount; i++)
        {
            Vector2 currentCircle = pointAf + incrementVector * i;
            for (int x = 0; x < dimensions.x; x++)
                for (int y = 0; y < dimensions.y; y++)
                    if (Vector2.Distance(new Vector2(x, y), currentCircle) < tunnelSize)
                        mapTexture[x, y] = 1;
        }

        Tunnel tunnel = new Tunnel(pointA, pointB);
        regionA.AddTunnel(tunnel);
        regionB.AddTunnel(tunnel);
    }

    private List<Region> determineRegions(int regionType)
    {
        List<Region> regions = new List<Region>();
        int[,] used = new int[dimensions.x, dimensions.y];

        for (int i = 0; i < dimensions.x; i++)
            for (int j = 0; j < dimensions.y; j++)
            {
                if (used[i, j] == 0 && mapTexture[i, j] == regionType)
                {
                    Region region = new Region();
                    Queue<Vector2Int> queue = new Queue<Vector2Int>();
                    queue.Enqueue(new Vector2Int(i, j));
                    used[i, j] = 1;

                    while (queue.Count > 0)
                    {
                        Vector2Int point = queue.Dequeue();
                        region.AddPoint(point);

                        for (int k = 0; k < Helpers.neswX.Length; k++)
                        {
                            Vector2Int neigh = new Vector2Int(point.x + Helpers.neswX[k], point.y + Helpers.neswY[k]);
                            if (neigh.x >= 0 && neigh.y >= 0 && neigh.x < dimensions.x && neigh.y < dimensions.y)
                            {
                                if (used[neigh.x, neigh.y] == 0 && mapTexture[neigh.x, neigh.y] == regionType)
                                {
                                    used[neigh.x, neigh.y] = 1;
                                    queue.Enqueue(neigh);
                                }
                            }
                        }
                    }

                    regions.Add(region);
                }
            }

        return regions;
    }

    private void filterRegions(int threshold, List<Region> regions, int regionType)
    {
        for (int i = 0; i < regions.Count; i++)
        {
            Region region = regions[i];
            if (region.points.Count < threshold)
            {
                foreach (Vector2Int point in region.points)
                {
                    mapTexture[point.x, point.y] = regionType == 1 ? -1 : 1;
                }

                regions.Remove(region);
                i--;
            }
        }
    }

    private void ApplyRules(int neightborThreshold)
    {
        int[,] newMap = new int[dimensions.x, dimensions.y];
        for (int i = 0; i < dimensions.x; i++)
            for (int j = 0; j < dimensions.y; j++)
            {
                int neighs = CountNeighbors(i, j);
                if (neighs > neightborThreshold)
                    newMap[i, j] = -1;
                else
                    newMap[i, j] = 1;
            }

        mapTexture = newMap;
    }

    private int CountNeighbors(int x, int y)
    {
        if (x == 0 || y == 0 || x == dimensions.x - 1 || y == dimensions.y - 1)
            return 9;

        int count = 0;
        for (int i = 0; i < Helpers.neighX.Length; i++)
            count += mapTexture[x + Helpers.neighX[i], y + Helpers.neighY[i]] == -1 ? 1 : 0;

        return count;
    }

    public List<Region> GetCaves()
    {
        return caves;
    }
}
