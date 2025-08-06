using UnityEngine;

public class Rank
{
    public string title;
    public int maxScore;
}

public static class RankHelper
{
    public static int classes = 4;
    public static int minCompletedRankIndex = 3;
    static Rank[] ranks = new Rank[]
    {
        new Rank { title = "Kamikaze Trainee", maxScore = 1000 },
        new Rank { title = "Runway Sweeper", maxScore = 2000 },
        new Rank { title = "Air Cadet", maxScore = 5000 },
        new Rank { title = "Flying Tiger", maxScore = 10000 },
        new Rank { title = "Blue Max", maxScore = 20000 },
    };

    public static string GetRankDescription(bool completedMission, int score)
    {
        int minIndex = completedMission ? minCompletedRankIndex : 0;
        int maxIndex = completedMission ? ranks.Length - 1 : minCompletedRankIndex - 1;

        int rankIndex = minIndex;
        while (rankIndex < maxIndex && score > ranks[rankIndex].maxScore)
        {
            rankIndex++;
        }

        int low = rankIndex == 0 ? 0 : ranks[rankIndex - 1].maxScore;
        int high = ranks[rankIndex].maxScore;
        int classIndex = (score - low) / ((high - low) / classes);
        classIndex = Mathf.Clamp(classIndex, 0, classes - 1);
        classIndex = classes - classIndex - 1;
        return $"{ranks[rankIndex].title} class {classIndex + 1}";
    }

    /*public static void TestRanks()
    {
        bool ok = true;
        var rank = GetRankDescription(false, 0);
        ok &= rank == "Kamikaze Trainee class 4";
        rank = GetRankDescription(true, 1300);
        ok &= rank == "Flying Tiger class 4";
        rank = GetRankDescription(false, 900);
        ok &= rank == "Kamikaze Trainee class 1";
        rank = GetRankDescription(false, 1010);
        ok &= rank == "Runway Sweeper class 4";
        rank = GetRankDescription(false, 4900);
        ok &= rank == "Air Cadet class 1";

        rank = GetRankDescription(false, 5010);
        ok &= rank == "Air Cadet class 1";
        rank = GetRankDescription(true, 5010);
        ok &= rank == "Flying Tiger class 4";

        rank = GetRankDescription(false, 7510);
        ok &= rank == "Air Cadet class 1";
        rank = GetRankDescription(true, 7510);
        ok &= rank == "Flying Tiger class 2";

        rank = GetRankDescription(true, 6790);
        ok &= rank == "Flying Tiger class 3";

        rank = GetRankDescription(false, 9900);
        ok &= rank == "Air Cadet class 1";
        rank = GetRankDescription(true, 9900);
        ok &= rank == "Flying Tiger class 1";

        rank = GetRankDescription(false, 11000);
        ok &= rank == "Air Cadet class 1";
        rank = GetRankDescription(true, 11000);
        ok &= rank == "Blue Max class 4";

        rank = GetRankDescription(true, 119900);
        ok &= rank == "Blue Max class 1";
        Debug.Log($"Rank Tests {ok}");
    }*/
}