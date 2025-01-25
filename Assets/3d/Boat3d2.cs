using UnityEngine;

public class Boat3d2 : MonoBehaviour, IVip
{
    public float speed = 0.8f;
    //GameState gameState;
    Vector3 velocity;
    bool alive = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        velocity = new Vector3(0, 0, -speed);
    }

    public void SetVip()
    {
        //todo: implement VIP
    }

    public bool IsVip()
    {
        //todo: implement VIP
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!alive)
        {
            return;
        }

        Vector3 position = transform.position;
        position += velocity * Time.deltaTime;
        transform.position = position;        
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log($"3D Boat 2 Hit!!!!!! Collided with {col.gameObject.name}");
        if (col.gameObject.name.StartsWith("riversection"))
        {
            return;
        }
        
        alive = false;
    }
}
