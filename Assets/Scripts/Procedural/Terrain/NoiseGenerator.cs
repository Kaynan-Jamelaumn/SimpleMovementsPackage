﻿using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Threading;
public class VoronoiCache
{

    private static VoronoiCache _instance;
    private ConcurrentDictionary<Vector2Int, List<Vector2>> chunkPoints; // Store points per chunk
    private ConcurrentDictionary<Vector2, Biome> pointBiomeMap;
    private ThreadLocal<System.Random> prng;
    public bool IsInitialized { get; private set; }

    public static VoronoiCache Instance
    {
        get
        {
            if (_instance == null)
                _instance = new VoronoiCache();
            return _instance;
        }
    }

    private VoronoiCache()
    {
        chunkPoints = new ConcurrentDictionary<Vector2Int, List<Vector2>>();
        pointBiomeMap = new ConcurrentDictionary<Vector2, Biome>();
    }

    public void Initialize(int seed = 0)
    {
        if (IsInitialized) return;

        // Thread-safe random number generator
        prng = new ThreadLocal<System.Random>(() => new System.Random(seed));
        IsInitialized = true;
    }

    private void GeneratePointsForChunk(Vector2Int chunkCoord, float scale, int numPoints, List<Biome> availableBiomes)
    {
        if (chunkPoints.ContainsKey(chunkCoord))
            return;

        var points = new List<Vector2>(numPoints);
        int chunkSeed = (chunkCoord.x * 73856093) ^ (chunkCoord.y * 19349663); // Unique seed per chunk
        System.Random localPrng = new System.Random(chunkSeed);

        for (int i = 0; i < numPoints; i++)
        {
            float randX = chunkCoord.x * scale + (float)localPrng.NextDouble() * scale;
            float randY = chunkCoord.y * scale + (float)localPrng.NextDouble() * scale;

            Vector2 newPoint = new Vector2(randX, randY);
            points.Add(newPoint);

            // Assign biome randomly using local PRNG
            Biome biome = availableBiomes[localPrng.Next(availableBiomes.Count)];
            pointBiomeMap[newPoint] = biome;
        }

        chunkPoints[chunkCoord] = points;
    }



    private void GeneratePointsForChunkOld(Vector2Int chunkCoord, float scale, int numPoints, List<Biome> availableBiomes)
    {
        if (chunkPoints.ContainsKey(chunkCoord))
            return;

        var points = new List<Vector2>(numPoints);
        for (int i = 0; i < numPoints; i++)
        {
            float randX = chunkCoord.x * scale + (float)prng.Value.NextDouble() * scale;
            float randY = chunkCoord.y * scale + (float)prng.Value.NextDouble() * scale;
            Vector2 newPoint = new Vector2(randX, randY);
            points.Add(newPoint);

            Biome biome = availableBiomes[prng.Value.Next(availableBiomes.Count)];
            pointBiomeMap[newPoint] = biome;
        }

        chunkPoints[chunkCoord] = points;
    }


    private IEnumerable<Vector2Int> GetAdjacentChunks(Vector2Int chunkCoord)
    {
        yield return chunkCoord;
        yield return chunkCoord + Vector2Int.left;
        yield return chunkCoord + Vector2Int.right;
        yield return chunkCoord + Vector2Int.up;
        yield return chunkCoord + Vector2Int.down;
        yield return chunkCoord + Vector2Int.up + Vector2Int.left;
        yield return chunkCoord + Vector2Int.up + Vector2Int.right;
        yield return chunkCoord + Vector2Int.down + Vector2Int.left;
        yield return chunkCoord + Vector2Int.down + Vector2Int.right;
    }

    public Biome GetClosestBiome(Vector2 worldPosition, Vector2Int chunkCoord, float scale, int numPoints, List<Biome> availableBiomes)
    {
        foreach (var adjChunk in GetAdjacentChunks(chunkCoord))
        {
            GeneratePointsForChunk(adjChunk, scale, numPoints, availableBiomes);
        }

        Biome closestBiome = null;
        float minDist = float.MaxValue;

        foreach (var adjChunk in GetAdjacentChunks(chunkCoord))
        {
            if (!chunkPoints.TryGetValue(adjChunk, out var points)) continue;

            foreach (var point in points)
            {
                float dist = (worldPosition - point).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    closestBiome = pointBiomeMap[point];
                }
            }
        }

        return closestBiome;
    }

}

public static class NoiseGenerator
{
    public static Biome Voronoi(Vector2 worldPosition, float scale, int numPoints, List<Biome> availableBiomes, int seed = 0)
    {
        VoronoiCache.Instance.Initialize(seed);

        Vector2Int chunkCoord = new Vector2Int(
            Mathf.FloorToInt(worldPosition.x / TerrainGenerator.chunkSize),
            Mathf.FloorToInt(worldPosition.y / TerrainGenerator.chunkSize)
        );

        return VoronoiCache.Instance.GetClosestBiome(worldPosition, chunkCoord, scale, numPoints, availableBiomes);
    }
}
