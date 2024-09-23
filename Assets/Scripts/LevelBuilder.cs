using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum CellContent
{
    GRASS = 0,
    WATER,
    LANDING_STRIP,
    ROAD,
    BRIDGE,
    HOUSE,
    TANK,
    FLACK_GUN,
    TREE1,
    TREE2
}

public class RiverSegment
{
    public int Height;
    public int slopeIndex;
}

public class LevelBuilder 
{
    public readonly int gridHeight = 300;
    public readonly int gridWidth = 30;
    public readonly int landingStripHeight = 30;
    public readonly int landingStripWidth = 30;
    public readonly int minSpaceBetweenRoads = 10;
    public readonly float roadProbability = 0.1f;
    public readonly int roadHeight = 2;
    static readonly int[] riverSlopes = new int[] {-1, -1, 0, 1, 1};
    static readonly int neutralRiverSlopeIndex = 2;
    public int minDistanceRiverAirstrip = 10;
    public int riverWidth = 6;
    public int maxRiverSegmentHeight = 7;
    public int minRiverSegmentHeight = 2;
    public float approachQuotient = 0.2f;
    
    // Builds a 2D level including landing strip at beginning.
    // Never mind viewing perspective or screen position.
    public CellContent[,] Build(bool riverLeftOfAirstrip)
    {
        var ret = new CellContent[gridWidth, gridHeight];
        var midX = gridWidth / 2;

        // Landing Strip
        var lsllcX = midX - (landingStripWidth / 2);
        for (var x = lsllcX; x <= landingStripWidth; x++)
        {
            for (var y = 0; y < landingStripHeight; y++)
            {
                ret[x, y] = CellContent.LANDING_STRIP;
            }
        }

        // Roads        
        var cooldown = 0;
        List<int> roads = new List<int>();
        for (var y = landingStripHeight + cooldown; y < (gridHeight - cooldown); y++)
        {
            if (cooldown <= 0 && UnityEngine.Random.Range(0f, 1.0f) < roadProbability)
            {
                roads.Add(y);
                for (var x = 0; x <= gridWidth; x++)
                {
                    for (var i = 0; y < (y + roadHeight); y++, i++)
                    {
                        ret[x, y] = CellContent.ROAD;
                    }
                }
                cooldown = minSpaceBetweenRoads;
            }
            cooldown--;
        }
        
        // River
        var directionMultiplier = riverLeftOfAirstrip ? -1 : 1;
        int riverLowerLeftCornerXStart = midX + directionMultiplier * minDistanceRiverAirstrip - (riverWidth / 2);
        int riverLowerLeftCornerX = riverLowerLeftCornerXStart;
        List<RiverSegment> riverSegments = new List<RiverSegment>();
        for (var y = 0; y < gridHeight;)
        {
            var segmentHeight = UnityEngine.Random.Range(minRiverSegmentHeight, maxRiverSegmentHeight);
            var maxSegmentY = gridHeight - y;
            if (segmentHeight > maxSegmentY)
            {
                segmentHeight = maxSegmentY;
            }

            var midRiverX = riverLowerLeftCornerX + (riverWidth / 2);
            riverLeftOfAirstrip = midRiverX < midX;
            var minSlopeIndex = 1;
            var maxSlopeIndexExclusive = riverSlopes.Length - 1;
            var approachLength = (int)(gridHeight * approachQuotient);
            bool approaching = gridHeight - y < approachLength;
            bool takingOff = y < approachLength;
            int slopeIndexOffset = 0;
            if (approaching)
            {
                // Airstrip approaching. River must not bend toward next airstrip location.
                slopeIndexOffset =  riverLeftOfAirstrip ? -1 : 1;
            }
            if (takingOff)
            {
                // Leaving Airstrip. River must not bend away from next airstrip location.
                slopeIndexOffset =  riverLeftOfAirstrip ? 1 : -1;
            }
            minSlopeIndex += slopeIndexOffset;
            maxSlopeIndexExclusive += slopeIndexOffset;
            var slopeIndex = UnityEngine.Random.Range(minSlopeIndex, maxSlopeIndexExclusive);
            riverSegments.Add(new RiverSegment {Height = segmentHeight, slopeIndex = slopeIndex});

            var slopeX = riverSlopes[slopeIndex] * segmentHeight;

            Debug.Log($"riverLowerLeftCornerX riverWidth slopeX y segmentHeight: {riverLowerLeftCornerX} {riverWidth} {slopeX} {y} {segmentHeight} {approaching} {takingOff} {minSlopeIndex} {maxSlopeIndexExclusive} {riverLeftOfAirstrip}");
            
            y += segmentHeight;
            riverLowerLeftCornerX += slopeX;
        }

        var ytmp = 0;
        var startX = riverLowerLeftCornerXStart;
        foreach (var segment in riverSegments)
        {
            var newY = ytmp + segment.Height;            
            for (var y = ytmp; y < newY; y++)
            {
                startX += riverSlopes[segment.slopeIndex];
                for (var x = startX; x < riverWidth; x++)
                {
                    if (x >= 0 && x < gridWidth)
                    {
                        ret[x, y] = CellContent.WATER;
                    }
                }
            }

            ytmp = newY;
        }

        // Houses
        // Tanks
        // Flack guns
        // Trees
        return ret;
    }
}