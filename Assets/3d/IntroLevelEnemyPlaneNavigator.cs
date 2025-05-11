
public enum EnemyPlaneNavigatorStage
{
    APPROACHING,
    SITTING_DUCK
}

public class IntroLevelEnemyPlaneNavigator : IEnemyPlaneNavigator
{
    public static readonly float maxOffsetX = 2f;
    public static readonly float endDistanceZ = 3f;
    public static readonly float startSpeedFactor = 0.5f;
    public EnemyPlaneNavigatorStage stage = EnemyPlaneNavigatorStage.APPROACHING;
    EnemyPlane3d enemyPlane;


    public IntroLevelEnemyPlaneNavigator(EnemyPlane3d enemyPlane)
    {
        this.enemyPlane = enemyPlane;        
    }
    
    public void Start()
    {
        stage = EnemyPlaneNavigatorStage.APPROACHING;
        var enemyPlanePosition = enemyPlane.transform.position;
        enemyPlanePosition.z = enemyPlane.refObject.position.z + enemyPlane.maxDistance;
        enemyPlane.transform.position = enemyPlanePosition;
        enemyPlane.SetSpeed(GameState.GetInstance().maxSpeed * startSpeedFactor);
    }

    public void Update()
    {
        var distance = enemyPlane.transform.position - enemyPlane.refObject.position;
        if (stage == EnemyPlaneNavigatorStage.APPROACHING)
        {
            if (distance.z < endDistanceZ)
            {
                stage = EnemyPlaneNavigatorStage.SITTING_DUCK;
                enemyPlane.SetSpeed(GameState.GetInstance().maxSpeed);
            }
        }

        if (distance.x > maxOffsetX)
        {
            //turn left
            enemyPlane.SetMoveX(-1);
        }
        else if (distance.x < -maxOffsetX)
        { 
            //turn right
            enemyPlane.SetMoveX(1);
        }
    }
}