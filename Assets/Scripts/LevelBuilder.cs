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

public class LevelBuilder 
{
    public readonly int gridHeight = 300;
    public readonly int gridWidth = 30;
    public readonly int landingStripHeight = 30;
    public readonly int landingStripWidth = 30;
    public readonly int minSpaceBetweenRoads = 10;
    public readonly float roadProbability = 0.1f;
    public readonly int roadHeight = 2;
    
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
        for (var y = landingStripHeight + cooldown; y < (gridHeight - cooldown); y++)
        {
            if (cooldown <= 0 && Random.Range(0f, 1.0f) < roadProbability)
            {
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
        // Houses
        // Tanks
        // Flack guns
        // Trees
        return ret;
    }
}