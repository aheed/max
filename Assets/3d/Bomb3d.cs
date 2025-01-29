using UnityEngine;

public class Bomb3d : MonoBehaviour
{
    public float verticalSpeed = 1.9f;
    public float maxCollisionAltitude = 0.2f;
    GameState gameState;
    float impactAltitude;

    GameState GetGameState() 
    {
        if (gameState == null)
        {
            gameState = FindAnyObjectByType<GameState>();
        }
        return gameState;
    }

    float GetImpactAltitude()
    {
        return GetGameState().riverAltitude;
    }

    void Start()
    {
        impactAltitude = GetImpactAltitude();
    }

    void Impact()
    {
        // Todo: handle bomb impact
        Debug.Log($"********* 3D Bomb Impact!!!!!!!!!!!!!!! at {transform.position}");
        GameState.GetInstance().BombLanded(gameObject, null);
        //Destroy(gameObject);
    }


    // Update is called once per frame
    void Update()
    {
        var tmpPos = transform.localPosition;
        var deltaVertical = -verticalSpeed * Time.deltaTime;
        tmpPos.y += deltaVertical;
        transform.localPosition = tmpPos;

        if (tmpPos.y <= impactAltitude)
        {
            Impact();
        }
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log($"********* 3D Bomb Hit!!!!!!!!!!!!!!! with {col.gameObject.name}");

        if(col.gameObject.name.StartsWith("ground"))
        {
            Impact();
        }        
    }
}
