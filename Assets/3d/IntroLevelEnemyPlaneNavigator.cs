
public enum EnemyPlaneNavigatorStage
{
    APPROACHING,
    SITTING_DUCK
}

public class IntroLevelEnemyPlaneNavigator : IEnemyPlaneNavigator
{
    public static readonly float maxOffsetX = 2f;
    public static readonly float startDistanceZ = 7f;
    public static readonly float endDistanceZ = 3f;
    public static readonly float startSpeedFactor = 0.5f;
    EnemyPlane3d enemyPlane;
    EnemyPlaneNavigatorStage stage = EnemyPlaneNavigatorStage.APPROACHING;

    public IntroLevelEnemyPlaneNavigator(EnemyPlane3d enemyPlane)
    {
        this.enemyPlane = enemyPlane;        
    }
    
    public void Start()
    {
        stage = EnemyPlaneNavigatorStage.APPROACHING;
        var enemyPlanePosition = enemyPlane.transform.position;
        enemyPlanePosition.z = enemyPlane.refObject.position.z + startDistanceZ;
        //enemyPlanePosition.y = GameState.GetInstance().maxAltitude / 2;
        enemyPlane.transform.position = enemyPlanePosition;
        enemyPlane.SetSpeed(GameState.GetInstance().maxSpeed * startSpeedFactor);
    }

    public void Update()
    {
        //var enemyPlanePosition = enemyPlane.transform.localPosition;
        var distance = enemyPlane.transform.position - enemyPlane.refObject.position;
        //var distanceX = enemyPlane.transform.position.x - enemyPlane.refObject.position.x;
        //var distanceZ = enemyPlane.transform.position.z - enemyPlane.refObject.position.z;
        if (stage == EnemyPlaneNavigatorStage.APPROACHING)
        {
            //var distanceZ = enemyPlanePosition.z;
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