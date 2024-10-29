using System;
using System.Collections.Generic;
using UnityEngine;

public enum CellContent
{
    GRASS = 0,
    WATER,
    LANDING_STRIP,
    ROAD,
    BRIDGE,
    HOUSE,
    HOUSE_FRONT,
    TANK,
    FLACK_GUN,
    TREE1,
    TREE2,
    BOAT1,
    BOAT2,
    HANGAR
}

public class RiverSegment
{
    public int height;
    public float slope;
}

public class HousePosition
{
    public int x;
    public int y;
}

public class LevelContents
{
    public static readonly int gridHeight = 300;
    public static readonly int gridWidth = 100;
    public IEnumerable<HousePosition> houses = new List<HousePosition>();
    public IEnumerable<RiverSegment> riverSegments = new List<RiverSegment>();
    public int riverLowerLeftCornerX;
    public bool riverEndsLeftOfAirstrip;
    public IEnumerable<int> roads = new List<int>();
    public CellContent[,] cells = new CellContent[gridWidth, gridHeight];
    public HousePosition hangar;
}

public static class LevelBuilder 
{    
    public static readonly int landingStripHeight = 30;
    public static readonly int landingStripWidth = 6;
    public static readonly int minSpaceBetweenRoads = 10;
    public static readonly float roadProbability = 0.1f;
    public static readonly int roadHeight = 2;
    static readonly float[] riverSlopes = new float[] {-0.5f, -0.5f, 0f, 1f, 1f};
    public static int minDistanceRiverAirstrip = 80;
    public static int maxNormalDistanceRiverMidLevelLeft = 18;
    public static int maxNormalDistanceRiverMidLevelRight = 9;
    public static int riverWidth = 12;
    public static int maxRiverSegmentHeight = 7;
    public static int minRiverSegmentHeight = 2;
    public static float approachQuotient = 0.35f;
    public static float finalApproachQuotient = 0.3f;
    public static int houseHeight = 3;
    public static int houseWidth = 6;
    public static float houseProbability = 0.01f;
    public static float tankProbability = 0.012f;
    public static float tankProbabilityAtHouse = 0.7f;
    public static float flackGunProbability = 0.01f;
    public static float treeProbability = 0.03f;
    public static float boat1Probability = 0.005f;
    public static float boat2Probability = 0.2f;
    
    // Builds a 2D level including landing strip at beginning.
    // Never mind viewing perspective or screen position.
    public static LevelContents Build(bool riverLeftOfAirstrip)
    {
        var ret = new LevelContents();
        var midX = LevelContents.gridWidth / 2;
        var approachLength = (int)(LevelContents.gridHeight * approachQuotient);
        var finalApproachLength = (int)(LevelContents.gridHeight * finalApproachQuotient);

        // Landing Strip
        var lsllcX = midX - (landingStripWidth / 2);
        for (var x = lsllcX; x <= lsllcX + landingStripWidth; x++)
        {
            for (var y = 0; y < landingStripHeight; y++)
            {
                ret.cells[x, y] = CellContent.LANDING_STRIP;
            }
        }
        
        // River
        var directionMultiplier = riverLeftOfAirstrip ? -1 : 1;
        int riverLowerLeftCornerXStart = midX + directionMultiplier * minDistanceRiverAirstrip - (riverWidth / 2);
        int riverLowerLeftCornerX = riverLowerLeftCornerXStart;
        ret.riverLowerLeftCornerX = riverLowerLeftCornerXStart;
        List<RiverSegment> riverSegments = new List<RiverSegment>();
        for (var y = 0; y < LevelContents.gridHeight;)
        {
            var segmentHeight = UnityEngine.Random.Range(minRiverSegmentHeight, maxRiverSegmentHeight);
            var maxSegmentHeight = LevelContents.gridHeight - y;
            if (segmentHeight > maxSegmentHeight)
            {
                segmentHeight = maxSegmentHeight;
            }
            if (segmentHeight % 2 != 0)
            {
                segmentHeight -= 1;
            }

            var midRiverX = riverLowerLeftCornerX + (riverWidth / 2);
            riverLeftOfAirstrip = midRiverX < midX;
            var minSlopeIndex = 0;
            var maxSlopeIndexExclusive = riverSlopes.Length - 1;
            var yDistanceToEnd = LevelContents.gridHeight - y;
            bool approaching = yDistanceToEnd < approachLength;
            bool finalApproaching = yDistanceToEnd < finalApproachLength;
            bool takingOff = y < approachLength;
            var midRiverOffset = midRiverX - midX;
            if (approaching)
            {
                // Airstrip approaching. River must not bend toward next airstrip location.
                //slopeIndexOffset = riverLeftOfAirstrip ? -1 : 1;
                if (riverLeftOfAirstrip)
                {
                    if (finalApproaching)
                    {
                        minSlopeIndex = 0;
                        maxSlopeIndexExclusive = 1;
                    }
                    else
                    {
                        maxSlopeIndexExclusive -= 1;
                    }
                }
                else
                {
                    if (finalApproaching)
                    {
                        minSlopeIndex = riverSlopes.Length-1;
                        maxSlopeIndexExclusive = riverSlopes.Length;
                    }
                    else
                    {
                        minSlopeIndex += 2;
                        maxSlopeIndexExclusive += 1;
                    }
                }
                
            }
            else if (takingOff)
            {
                // Leaving Airstrip. River must not bend away from next airstrip location.
                //slopeIndexOffset =  riverLeftOfAirstrip ? 1 : -1;
                if (riverLeftOfAirstrip)
                {
                    minSlopeIndex += 2;
                    maxSlopeIndexExclusive += 1;
                }
                else
                {
                    maxSlopeIndexExclusive -= 2;
                }
            }
            else
            {
                if (midRiverOffset > maxNormalDistanceRiverMidLevelRight)
                {
                    // River too far to the right
                    maxSlopeIndexExclusive -= 2;
                }
                else if (midRiverOffset < -maxNormalDistanceRiverMidLevelLeft)
                {
                    // River too far to the left
                    minSlopeIndex += 2;
                }
            }
            //minSlopeIndex += slopeIndexOffset;
            //maxSlopeIndexExclusive += slopeIndexOffset;
            var slopeIndex = UnityEngine.Random.Range(minSlopeIndex, maxSlopeIndexExclusive);
            var slope = riverSlopes[slopeIndex];
            riverSegments.Add(new RiverSegment {height = segmentHeight, slope = slope});

            var slopeX = (int)(slope * segmentHeight);

            //Debug.Log($"riverLowerLeftCornerX riverWidth slopeX y segmentHeight: {riverLowerLeftCornerX} {riverWidth} {slopeX} {y} {segmentHeight} {approaching} {takingOff} {minSlopeIndex} {maxSlopeIndexExclusive} {riverLeftOfAirstrip}");

            var randVal = UnityEngine.Random.Range(0f, 1.0f);
            if (randVal < boat2Probability && midRiverX >= 0 && midRiverX < LevelContents.gridWidth)
            {
                ret.cells[midRiverX, y] = CellContent.BOAT2;
            }
            
            y += segmentHeight;
            riverLowerLeftCornerX += slopeX;
        }
        ret.riverSegments = riverSegments;
        ret.riverEndsLeftOfAirstrip = riverLeftOfAirstrip;

        var ytmp = 0;
        var startX = (float)riverLowerLeftCornerXStart;
        foreach (var segment in riverSegments)
        {
            var newY = ytmp + segment.height;            
            for (var y = ytmp; y < newY; y++)
            {
                startX += segment.slope;
                for (var x = startX; x < (startX + riverWidth); x++)
                {
                    if (x >= 0 && x < LevelContents.gridWidth && ret.cells[(int)x, y] == CellContent.GRASS)
                    {
                        ret.cells[(int)x, y] = CellContent.WATER;
                    }
                }
            }

            ytmp = newY;
        }

        // Roads        
        var cooldown = 0;
        List<int> roads = new List<int>();
        for (var y = landingStripHeight + cooldown; y < (LevelContents.gridHeight - roadHeight - cooldown); y++)
        {
            if (cooldown <= 0 && UnityEngine.Random.Range(0f, 1.0f) < roadProbability)
            {
                roads.Add(y);
                for (var x = 0; x < LevelContents.gridWidth; x++)
                {
                    for (var i = 0; i < roadHeight; i++)
                    {
                        //Debug.Log($"{x} {y} {i} {roadHeight}");
                        ret.cells[x, y+i] = CellContent.ROAD;
                    }
                }
                cooldown = minSpaceBetweenRoads;
            }
            if (cooldown > 0)
            {
                cooldown--;
            }
        }
        ret.roads = roads;

        // Hangar
        ret.hangar = new HousePosition {x = LevelContents.gridWidth / 2 - 6, y = 12};
        var hangarWidth = 4;
        var hangarHeight = 6;
        for (var y = ret.hangar.y - hangarHeight / 2; y < ret.hangar.y + hangarHeight / 2; y++)
        {
            for (var x = ret.hangar.x - hangarWidth / 2; x < ret.hangar.x + hangarWidth / 2; x++)
            {
                var cellContents = x == ret.hangar.x && y == ret.hangar.y ? CellContent.HANGAR : CellContent.HOUSE;
                ret.cells[x, y] = cellContents;
            }
        }

        var houses = new List<HousePosition>();
        for (var y = 0; y < LevelContents.gridHeight; y++)
        {
            for (var x = 0; x < LevelContents.gridWidth; x++)
            {
                var randVal = UnityEngine.Random.Range(0f, 1.0f);

                // Houses
                if (randVal < houseProbability && y > (landingStripHeight * 2))
                {
                    //Debug.Log($"House please!");
                    var spaceEnough =   x < (LevelContents.gridWidth - houseWidth) &&
                                        y < (LevelContents.gridHeight - houseHeight) &&
                                        x > houseWidth &&
                                        y > houseHeight;
                    for (var xtmp = x - houseWidth; (xtmp < (x + houseWidth)) && spaceEnough; xtmp++)
                    {
                        for (ytmp = y - houseHeight; (ytmp < (y + houseHeight)) && spaceEnough; ytmp++)
                        {
                            //Debug.Log($"{x} {y} {xtmp} {ytmp}");
                            spaceEnough = ret.cells[xtmp, ytmp] == CellContent.GRASS;
                        }
                    }

                    if (spaceEnough)
                    {
                        houses.Add(new HousePosition {x = x, y = y});
                        for (var xtmp = x - houseWidth / 2; xtmp < x + houseWidth / 2; xtmp++)
                        {
                            for (ytmp = y-1; ytmp < y + houseHeight; ytmp++)
                            {
                                ret.cells[xtmp, ytmp] = CellContent.HOUSE;
                            }
                        }

                        var houseFrontWidth = 4;
                        var houseFrontHeight = 3;
                        for (var xtmp = x; xtmp < x + houseFrontWidth; xtmp++)
                        {
                            for (ytmp = y - houseFrontHeight; ytmp < y - 1; ytmp++)
                            {
                                ret.cells[xtmp, ytmp] = CellContent.HOUSE_FRONT;
                            }
                        }
                    }
                }
            }
        }

        for (var y = 0; y < LevelContents.gridHeight; y++)
        {
            for (var x = 0; x < LevelContents.gridWidth; x++)
            {   
                // Tanks
                var randVal = UnityEngine.Random.Range(0f, 1.0f);
                var localTankProbability = ret.cells[x, y] == CellContent.HOUSE_FRONT ? tankProbabilityAtHouse : tankProbability;
                if (randVal < localTankProbability && (ret.cells[x, y] == CellContent.GRASS || ret.cells[x, y] == CellContent.HOUSE_FRONT) && y > landingStripHeight)
                {
                    ret.cells[x, y] = CellContent.TANK;
                }

                // Flack guns
                randVal = UnityEngine.Random.Range(0f, 1.0f);
                if (randVal < flackGunProbability && ret.cells[x, y] == CellContent.GRASS && y > landingStripHeight)
                {
                    ret.cells[x, y] = CellContent.FLACK_GUN;
                }

                // Trees
                randVal = UnityEngine.Random.Range(0f, 1.0f);
                if (randVal < treeProbability && ret.cells[x, y] == CellContent.GRASS)
                {
                    ret.cells[x, y] = CellContent.TREE1;
                }
                
                randVal = UnityEngine.Random.Range(0f, 1.0f);
                if (randVal < treeProbability && ret.cells[x, y] == CellContent.GRASS)
                {
                    ret.cells[x, y] = CellContent.TREE2;
                }

                // Boats
                randVal = UnityEngine.Random.Range(0f, 1.0f);
                if (randVal < boat1Probability && ret.cells[x, y] == CellContent.WATER)
                {
                    ret.cells[x, y] = CellContent.BOAT1;
                }                
            }
        }

        ret.houses = houses;       
        return ret;
    }
}