
public enum EnemyPlaneNavigatorStage
{
    APPROACHING,
    SITTING_DUCK
}

public class IntroLevelEnemyPlaneNavigator : IEnemyPlaneNavigator
{
    public static readonly float maxOffsetX = 2f;
    public static readonly float startDistanceZ = 10f;
    public static readonly float endDistanceZ = 3f;
    public static readonly float startSpeedFactor = 0.5f;
    EnemyPlane3d enemyPlane;
    EnemyPlaneNavigatorStage stage = EnemyPlaneNavigatorStage.APPROACHING;

    IntroLevelEnemyPlaneNavigator(EnemyPlane3d enemyPlane)
    {
        this.enemyPlane = enemyPlane;        
    }
    
    public void Start()
    {
        stage = EnemyPlaneNavigatorStage.APPROACHING;
        var enemyPlanePosition = enemyPlane.transform.localPosition;
        enemyPlanePosition.z = startDistanceZ;
        enemyPlane.transform.localPosition = enemyPlanePosition;
        enemyPlane.SetSpeed(GameState.GetInstance().maxSpeed * startSpeedFactor);
    }

    public void Update()
    {
        var enemyPlanePosition = enemyPlane.transform.localPosition;
        if (stage == EnemyPlaneNavigatorStage.APPROACHING)
        {
            var distanceZ = enemyPlanePosition.z;
            if (distanceZ < endDistanceZ)
            {
                stage = EnemyPlaneNavigatorStage.SITTING_DUCK;
                enemyPlane.SetSpeed(GameState.GetInstance().maxSpeed);
            }
        }

        if (enemyPlanePosition.x > maxOffsetX)
        {
            //turn left
            enemyPlane.SetMoveX(-1);
        }
        else if (enemyPlanePosition.x < -maxOffsetX)
        { 
            //turn right
            enemyPlane.SetMoveX(1);
        }
    }
}