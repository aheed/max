using UnityEngine;

public class Bomb3d : MonoBehaviour
{
    public float verticalSpeed = 1.9f;
    public float maxCollisionAltitude = 0.2f;
    GameState gameState;
    float impactAltitude;
    float forwardSpeed;

    GameState GetGameState() 
    {
        if (gameState == null)
        {
            gameState = GameState.GetInstance();
        }
        return gameState;
    }

    float GetImpactAltitude()
    {
        return transform.position.y > 0 ? 0f : GetGameState().riverAltitude;
    }

    void Start()
    {
        impactAltitude = GetImpactAltitude();
        forwardSpeed = GetGameState().GetStateContents().speed;        
    }

    void Impact()
    {
        // Todo: handle bomb impact
        //Debug.Log($"********* 3D Bomb Impact!!!!!!!!!!!!!!! at {transform.position}");
        GameState.GetInstance().BombLanded(gameObject, null);
        //Destroy(gameObject);
    }


    // Update is called once per frame
    void Update()
    {
        var tmpPos = transform.localPosition;
        var deltaVertical = -verticalSpeed * Time.deltaTime;
        var deltaForward = forwardSpeed * Time.deltaTime;
        tmpPos.y += deltaVertical;
        tmpPos.z += deltaForward;
        transform.localPosition = tmpPos;

        if (tmpPos.y < impactAltitude)
        {
            impactAltitude = GetImpactAltitude();
            Impact();
        }
    }

    void OnTriggerEnter(Collider col)
    {
        //Debug.Log($"********* 3D Bomb Hit!!!!!!!!!!!!!!! with {col.gameObject.name}");

        if(col.gameObject.name.StartsWith("ground"))
        {
            Impact();
        }        
    }
}
