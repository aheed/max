using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    HANGAR,
    VEHICLE1,
    VEHICLE2,
    ENEMY_HANGAR
}

public enum LevelType
{
    NORMAL,
    ROAD,
    CITY
}

public class RiverSegment
{
    public int height;
    public float slope;
}

public class RoadSegment
{
    public int height;
    public float slope;
}

public class HousePosition
{
    public int x;
    public int y;
}

public class HouseSpec
{
    public HousePosition position;
    public int width;
    public int height;
    public int depth;
}

public class EnemyHQSpec
{
    public int y;
    public bool bombed;
}

public class City 
{
    public int yStart;
    public int yEnd;

    public IEnumerable<EnemyHQSpec> enemyHQs;

    public IEnumerable<HousePosition> bigHouses;
}

public class LevelPrerequisite
{
    public LevelType levelType;
    public bool riverLeftOfAirstrip;

    public IEnumerable<bool> enemyHQsBombed; // Relevant for LevelType.CITY
}

public class LevelContents
{
    public static readonly int gridHeight = 300;
    public static readonly int gridWidth = 100;
    public IEnumerable<HouseSpec> houses = new List<HouseSpec>();
    public IEnumerable<RiverSegment> riverSegments = new List<RiverSegment>();
    public IEnumerable<RoadSegment> roadSegments = new List<RoadSegment>();
    public int riverLowerLeftCornerX;
    public int roadLowerLeftCornerX;
    public bool riverEndsLeftOfAirstrip;
    public IEnumerable<int> roads = new List<int>();
    public IEnumerable<int> enemyAirstrips = new List<int>();
    public CellContent[,] cells = new CellContent[gridWidth, gridHeight];
    public HousePosition hangar;
    public City city;    
}

public class LevelBuilder 
{    
    public static readonly int landingStripHeight = 30;
    public static readonly int landingStripWidth = 6;
    public static readonly int minSpaceBetweenRoads = 10;
    public static readonly float roadProbability = 0.1f;
    public static readonly int roadHeight = 2;
    static readonly float[] riverSlopes = new float[] {-0.5f, -0.5f, 0f, 1f, 1f};
    static readonly float[] roadSlopes = new float[] {-1f, 0f, 1f};
    public static int minDistanceRiverAirstrip = 80;
    public static int maxNormalDistanceRiverMidLevelLeft = 18;
    public static int maxNormalDistanceRiverMidLevelRight = 9;
    public static int riverWidth = 12;
    public static int roadWidth = 2;
    public static int maxRiverSegmentHeight = 7;
    public static int minRiverSegmentHeight = 2;
    public static int roadSegmentHeight = 2;
    public static float approachQuotient = 0.35f;
    public static float outsideCityQuotient = 0.38f;
    public static int bigHousesMargin = 8;
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
    public static float vehicle1Probability = 0.07f;
    public static float vehicle2Probability = 0.07f;
    public static int normalHouseWidth = 5;
    public static int minHouseWidth = 2;
    public static int maxHouseWidth = 6;
    public static int normalHouseHeight = 2;
    public static int minHouseHeight = 1;
    public static int maxHouseHeight = 4;
    public static int normalHouseDepth = 2;
    public static int minHouseDepth = 2;
    public static int maxHouseDepth = 5;

    public static int enemyAirstripHeight = 16;
    public static int minSpaceBetweenEnemyAirstrips = 20;
    public static int enemyAirstripMinDistance = 100;
    public static int enemyAirstripXDistance = 8;
    public static float enemyAirstripProbability = 0.3f;
    public static int bigHouseRoadDistance = 10;
    private static int maxRandom = 65535;
    private System.Random _rnd = new Random();

    public bool TrueByProbability(float probability)
    {
        return _rnd.Next(0, maxRandom) < (int)(probability * maxRandom);
    }

    public async Task<LevelContents> BuildAsync(LevelPrerequisite levelPrerequisite)
    {
        return await Task.Run(() => Build(levelPrerequisite));
    }

    // Builds a 2D level including landing strip at beginning.
    // Never mind viewing perspective or screen position.
    public LevelContents Build(LevelPrerequisite levelPrerequisite)
    {
        var ret = new LevelContents();
        var midX = LevelContents.gridWidth / 2;
        var approachLength = (int)(LevelContents.gridHeight * approachQuotient);
        var cityApproachLength = (int)(LevelContents.gridHeight * outsideCityQuotient);
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

        LevelType levelType = levelPrerequisite.levelType;
        var riverLeftOfAirstrip = levelPrerequisite.riverLeftOfAirstrip;
        ret.riverEndsLeftOfAirstrip = riverLeftOfAirstrip; // May be overridden below
        
        if (levelType == LevelType.NORMAL)
        {
            // River
            var directionMultiplier = riverLeftOfAirstrip ? -1 : 1;
            int riverLowerLeftCornerXStart = midX + directionMultiplier * minDistanceRiverAirstrip - (riverWidth / 2);
            int riverLowerLeftCornerX = riverLowerLeftCornerXStart;
            ret.riverLowerLeftCornerX = riverLowerLeftCornerXStart;
            List<RiverSegment> riverSegments = new List<RiverSegment>();
            for (var y = 0; y < LevelContents.gridHeight;)
            {
                var segmentHeight = _rnd.Next(minRiverSegmentHeight, maxRiverSegmentHeight);
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
                var slopeIndex = _rnd.Next(minSlopeIndex, maxSlopeIndexExclusive);
                var slope = riverSlopes[slopeIndex];
                riverSegments.Add(new RiverSegment {height = segmentHeight, slope = slope});

                var slopeX = (int)(slope * segmentHeight);

                //Debug.Log($"riverLowerLeftCornerX riverWidth slopeX y segmentHeight: {riverLowerLeftCornerX} {riverWidth} {slopeX} {y} {segmentHeight} {approaching} {takingOff} {minSlopeIndex} {maxSlopeIndexExclusive} {riverLeftOfAirstrip}");

                if (TrueByProbability(boat2Probability) && midRiverX >= 0 && midRiverX < LevelContents.gridWidth)
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
                    for (var x = startX; x <= (startX + riverWidth); x++)
                    {
                        if (x >= 0 && x < LevelContents.gridWidth && ret.cells[(int)x, y] == CellContent.GRASS)
                        {
                            ret.cells[(int)x, y] = CellContent.WATER;
                        }
                    }
                    startX += segment.slope;
                }

                ytmp = newY;
            }
        


            // Roads across
            var cooldown = 0;
            List<int> roads = new List<int>();
            for (var y = landingStripHeight + cooldown; y < (LevelContents.gridHeight - roadHeight - cooldown); y++)
            {
                if (cooldown <= 0 && TrueByProbability(roadProbability))
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
        }

        if (levelType == LevelType.ROAD || levelType == LevelType.CITY)
        {
            ret.riverEndsLeftOfAirstrip = riverLeftOfAirstrip;

            // Road along flight path
            var directionMultiplier = riverLeftOfAirstrip ? 1 : -1;
            int roadLowerLeftCornerXStart = midX + directionMultiplier * minDistanceRiverAirstrip - (roadWidth / 2);
            int roadLowerLeftCornerX = roadLowerLeftCornerXStart;
            ret.roadLowerLeftCornerX = roadLowerLeftCornerXStart;
            List<RoadSegment> roadSegments = new List<RoadSegment>();
            for (var y = 0; y < LevelContents.gridHeight;)
            {
                var segmentHeight = roadSegmentHeight;
                var maxSegmentHeight = LevelContents.gridHeight - y;
                if (segmentHeight > maxSegmentHeight)
                {
                    segmentHeight = maxSegmentHeight;
                }
                if (segmentHeight % 2 != 0)
                {
                    segmentHeight -= 1;
                }

                var midRoadX = roadLowerLeftCornerX + (roadWidth / 2);
                //riverLeftOfAirstrip = midRiverX < midX;
                //var minSlopeIndex = 0;
                //var maxSlopeIndexExclusive = riverSlopes.Length - 1;
                var yDistanceToEnd = LevelContents.gridHeight - y;
                bool approaching = yDistanceToEnd < approachLength;
                //bool finalApproaching = yDistanceToEnd < finalApproachLength;
                //bool takingOff = y < approachLength;
                var midRoadOffset = Math.Abs(midRoadX - midX);
                bool takingOff = midRoadOffset > 0.3f;
                var slopeIndex = 1;
                if (approaching)
                {
                    // Airstrip approaching. Road must go away from river direction.
                    slopeIndex = riverLeftOfAirstrip ? 2 : 0;
                }
                else if (takingOff)
                {
                    // Leaving Airstrip. Road must go toward river direction.
                    slopeIndex = riverLeftOfAirstrip ? 0 : 2;
                }
                
                var slope = roadSlopes[slopeIndex];
                roadSegments.Add(new RoadSegment {height = segmentHeight, slope = slope});

                var slopeX = (int)(slope * segmentHeight);

                if (levelType == LevelType.ROAD  && !approaching && !takingOff)
                {
                    // Mid-road stationary vehicles
                    if (TrueByProbability(vehicle1Probability))
                    {
                        ret.cells[midRoadX, y] = CellContent.VEHICLE1;
                    }
                    else if (TrueByProbability(vehicle2Probability))
                    {
                        ret.cells[midRoadX, y] = CellContent.VEHICLE2;
                    }
                }
                
                y += segmentHeight;
                roadLowerLeftCornerX += slopeX;
            }
            ret.roadSegments = roadSegments;

            var ytmp = 0;
            var startX = (float)roadLowerLeftCornerXStart;
            foreach (var segment in roadSegments)
            {
                var newY = ytmp + segment.height;            
                for (var y = ytmp; y < newY; y++)
                {
                    for (var x = startX; x <= (startX + roadWidth); x++)
                    {
                        if (x >= 0 && x < LevelContents.gridWidth && ret.cells[(int)x, y] == CellContent.GRASS)
                        {
                            ret.cells[(int)x, y] = CellContent.ROAD;
                        }
                    }
                    startX += segment.slope;
                }

                ytmp = newY;
            }

            if (levelType == LevelType.ROAD )
            {
                // Enemy airstrips
                var cooldown = 0;
                List<int> strips = new List<int>();
                for (var y = enemyAirstripMinDistance; y < (LevelContents.gridHeight - enemyAirstripHeight - enemyAirstripMinDistance); y++)
                {
                    if (cooldown <= 0 && TrueByProbability(enemyAirstripProbability))
                    {
                        strips.Add(y);
                        var stripStartX = midX - enemyAirstripXDistance;
                        for (var x = stripStartX; x < stripStartX + landingStripWidth; x++)
                        {
                            for (var i = 0; i < enemyAirstripHeight; i++)
                            {
                                ret.cells[x, y+i] = CellContent.LANDING_STRIP;
                            }
                        }

                        // space for hangar
                        var hangarX = stripStartX - hangarWidth / 2 - 1;
                        var hangarY = y + enemyAirstripHeight / 2;
                        var hangarStartX = hangarX - hangarWidth / 2;
                        var hangarStartY = hangarY - hangarHeight / 2;
                        for (var yy = hangarStartY; yy < hangarStartY + hangarHeight; yy++)
                        {
                            for (var xx = hangarStartX; xx < hangarStartX + hangarWidth; xx++)
                            {
                                var cellContents = xx == hangarX && yy == hangarY ? CellContent.ENEMY_HANGAR : CellContent.HOUSE;
                                ret.cells[xx, yy] = cellContents;
                            }
                        }
                        
                        cooldown = minSpaceBetweenEnemyAirstrips;
                    }
                    if (cooldown > 0)
                    {
                        cooldown--;
                    }
                }
                ret.enemyAirstrips = strips;
            }
        }    

        if (levelType == LevelType.ROAD || levelType == LevelType.NORMAL)
        {
            var houses = new List<HouseSpec>();
            for (var y = 0; y < LevelContents.gridHeight; y++)
            {
                for (var x = 0; x < LevelContents.gridWidth; x++)
                {
                    // Houses
                    if (TrueByProbability(houseProbability) && y > (landingStripHeight * 2))
                    {
                        //Debug.Log($"House please!");
                        var spaceEnough =   x < (LevelContents.gridWidth - houseWidth) &&
                                            y < (LevelContents.gridHeight - houseHeight) &&
                                            x > houseWidth &&
                                            y > houseHeight;
                        for (var xtmp = x - houseWidth; (xtmp < (x + houseWidth)) && spaceEnough; xtmp++)
                        {
                            for (var ytmp = y - houseHeight; (ytmp < (y + houseHeight)) && spaceEnough; ytmp++)
                            {
                                //Debug.Log($"{x} {y} {xtmp} {ytmp}");
                                spaceEnough = ret.cells[xtmp, ytmp] == CellContent.GRASS;
                            }
                        }

                        if (spaceEnough)
                        {
                            var width = normalHouseWidth;
                            var height = normalHouseHeight;
                            var depth = normalHouseDepth;

                            if (levelType == LevelType.ROAD && x < midX)
                            {
                                width = _rnd.Next(minHouseWidth, maxHouseWidth+1);
                                height = _rnd.Next(minHouseHeight, maxHouseHeight+1);
                                depth = _rnd.Next(minHouseDepth, maxHouseDepth+1);
                            }

                            houses.Add(new HouseSpec { 
                                position = new HousePosition {x = x, y = y},
                                width = width,
                                height = height,
                                depth = depth
                                });

                            for (var xtmp = x - houseWidth / 2; xtmp < x + houseWidth / 2; xtmp++)
                            {
                                for (var ytmp = y-1; ytmp < y + houseHeight; ytmp++)
                                {
                                    ret.cells[xtmp, ytmp] = CellContent.HOUSE;
                                }
                            }

                            var houseFrontWidth = 4;
                            var houseFrontHeight = 3;
                            for (var xtmp = x; xtmp < x + houseFrontWidth; xtmp++)
                            {
                                for (var ytmp = y - houseFrontHeight; ytmp < y - 1; ytmp++)
                                {
                                    ret.cells[xtmp, ytmp] = CellContent.HOUSE_FRONT;
                                }
                            }
                        }
                    }
                }
            }
            ret.houses = houses;
        }


        if (levelType == LevelType.CITY)
        {
            var yStart = cityApproachLength;
            var yEnd = LevelContents.gridHeight - cityApproachLength;

            var enemyHqDistance = (yEnd - yStart) / (levelPrerequisite.enemyHQsBombed.Count() + 1);
            var yOffset = yStart + enemyHqDistance;
            var enemyHQs = levelPrerequisite.enemyHQsBombed.Select(bombed => 
                {
                    var ret = new EnemyHQSpec {y = yOffset, bombed = bombed};
                    yOffset += enemyHqDistance;
                    return ret;
                }).ToList();

            ret.city = new City {
                yStart = yStart,
                yEnd = yEnd,
                enemyHQs = enemyHQs
            };

            foreach (var hq in ret.city.enemyHQs)
            {
                for (var xtmp = midX - houseWidth / 2; xtmp < midX + houseWidth / 2; xtmp++)
                {
                    for (var ytmp = hq.y-1; ytmp < hq.y + houseHeight; ytmp++)
                    {
                        ret.cells[xtmp, ytmp] = CellContent.HOUSE;
                    }
                }
            }
        }

        var bigHousesList = new List<HousePosition>();        
        for (var y = 0; y < LevelContents.gridHeight; y++)
        {
            var yDistanceToEnd = LevelContents.gridHeight - y;
            var inCity = levelType == LevelType.CITY && y > cityApproachLength && yDistanceToEnd > cityApproachLength;            
            if (inCity)
            {
                var flakX = 0;
                switch (y % 6)
                {
                    case 0:
                        flakX = midX - 2;
                        break;
                    case 3:
                        flakX = midX - 4;
                        break;
                }
                if (flakX != 0 && ret.cells[flakX, y] != CellContent.HOUSE)
                {
                    ret.cells[flakX, y] = CellContent.FLACK_GUN;
                }

                var housesApproachLength = cityApproachLength + bigHousesMargin;
                if (y > housesApproachLength && yDistanceToEnd > housesApproachLength)
                {
                    if (y % 4 == 3)
                    {
                        bigHousesList.Add(new HousePosition {x = midX - bigHouseRoadDistance, y = y});
                        bigHousesList.Add(new HousePosition {x = midX + bigHouseRoadDistance, y = y});
                    }
                }
                continue;
            }

            for (var x = 0; x < LevelContents.gridWidth; x++)
            {   
                // Tanks
                var localTankProbability = ret.cells[x, y] == CellContent.HOUSE_FRONT ? tankProbabilityAtHouse : tankProbability;
                if (TrueByProbability(localTankProbability) && (ret.cells[x, y] == CellContent.GRASS || ret.cells[x, y] == CellContent.HOUSE_FRONT) && y > landingStripHeight)
                {
                    ret.cells[x, y] = CellContent.TANK;
                }

                // Flack guns
                if (TrueByProbability(flackGunProbability) && ret.cells[x, y] == CellContent.GRASS && y > landingStripHeight)
                {
                    ret.cells[x, y] = CellContent.FLACK_GUN;
                }

                // Trees
                if (TrueByProbability(treeProbability) && ret.cells[x, y] == CellContent.GRASS)
                {
                    ret.cells[x, y] = CellContent.TREE1;
                }
                
                if (TrueByProbability(treeProbability) && ret.cells[x, y] == CellContent.GRASS)
                {
                    ret.cells[x, y] = CellContent.TREE2;
                }

                // Boats
                if (TrueByProbability(boat1Probability) && ret.cells[x, y] == CellContent.WATER)
                {
                    ret.cells[x, y] = CellContent.BOAT1;
                }

                /*if (ret.cells[x, y] == CellContent.ROAD || ret.cells[x, y] == CellContent.WATER || ret.cells[x, y] == CellContent.LANDING_STRIP) //TEMP !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                {
                    ret.cells[x, y] = CellContent.TANK;
                }*/               
            }
        }

        if (ret.city != null)
        {
            ret.city.bigHouses = bigHousesList;
        }
        
        return ret;
    }
}