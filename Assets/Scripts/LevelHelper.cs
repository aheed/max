
public enum AramamentType
{
    STANDARD,
    BOUNCING_BOMBS,
};

public static class LevelHelper
{
    public static bool PossibleVipTargets(LevelType levelType)
    {
        return levelType != LevelType.CITY &&
            levelType != LevelType.BALLOONS &&
            levelType != LevelType.ROBOT_BOSS &&
            levelType != LevelType.RED_BARON_BOSS &&
            levelType != LevelType.DAM;
    }

    public static int GetLevelHeight(LevelType levelType, bool firstLevel)
    {
        var gridHeight = LevelContents.fullGridHeight;
        if ((levelType == LevelType.RED_BARON_BOSS &&
            !firstLevel) ||
            levelType == LevelType.INTRO)
        {
            gridHeight = LevelContents.shortGridHeight;
        }
        else if (levelType == LevelType.DAM)
        {
            gridHeight = (LevelContents.fullGridHeight * 3) / 2;
        }
        if (gridHeight % 2 != 0)
        {
            gridHeight--;
        }

        return gridHeight;
    }

    public static bool LandingStrip(LevelType levelType, bool firstLevel, bool missionComplete)
    {
        return firstLevel ||
            missionComplete ||
            (levelType != LevelType.RED_BARON_BOSS &&
             levelType != LevelType.INTRO);
    }

    public static bool ClearSpaceAtCentre(LevelType levelType)
    {
        return levelType == LevelType.INTRO;
    }

    public static AirStripInfo GetAirStripInfo(LevelType levelType, bool firstLevel)
    {
        AirStripInfo ret = null;
        if (levelType == LevelType.INTRO)
        {
            ret = firstLevel ?
                AirStripRepository.introLevelStartAirStrip :
                AirStripRepository.introLevelEndAirStrip;
        }

        return ret;
    }

    public static bool River(LevelType levelType)
    {
        return levelType == LevelType.NORMAL ||
            levelType == LevelType.BALLOONS ||
            levelType == LevelType.ROBOT_BOSS ||
            levelType == LevelType.RED_BARON_BOSS ||
            levelType == LevelType.DAM;
    }

    public static bool RoadAlongFlightPath(LevelType levelType)
    {
        return levelType == LevelType.ROAD || levelType == LevelType.CITY;
    }

    public static bool EnemyAirstrips(LevelType levelType)
    {
        return levelType == LevelType.ROAD;
    }

    public static bool Houses(LevelType levelType)
    {
        return levelType == LevelType.ROAD ||
            levelType == LevelType.NORMAL ||
            levelType == LevelType.ROBOT_BOSS ||
            levelType == LevelType.RED_BARON_BOSS ||
            levelType == LevelType.DAM;
    }

    public static bool RiverNearCentre(LevelType levelType)
    {
        return levelType == LevelType.DAM;
    }

    public static bool MidRoadStationaryVehicles(LevelType levelType)
    {
        return levelType == LevelType.ROAD;
    }

    public static bool VariableHouseSizes(LevelType levelType)
    {
        return levelType == LevelType.ROAD;
    }

    public static bool EnemyHQs(LevelType levelType)
    {
        return levelType == LevelType.CITY;
    }

    public static bool Dams(LevelType levelType)
    {
        return levelType == LevelType.DAM;
    }

    public static bool RandomFlakGuns(LevelType levelType)
    {
        return levelType != LevelType.INTRO;
    }

    public static BossType GetBossType(LevelType levelType)
    {
        if (levelType == LevelType.ROBOT_BOSS)
        {
            return BossType.ROBOT;
        }
        else if (levelType == LevelType.RED_BARON_BOSS)
        {
            return BossType.RED_BARON;
        }
        else if (levelType == LevelType.INTRO)
        {
            return BossType.INTRO_CONTROLLER;
        }
        
        return BossType.NONE;
    }
}

