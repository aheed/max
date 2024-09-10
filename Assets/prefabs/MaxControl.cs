using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MaxControl : MonoBehaviour
{
    public Transform refObject;
    Vector3 offset;
    public float playerSpeed = 3.0f;
    public InputAction MoveAction;
    public InputAction FireAction;
    Rigidbody2D rigidbody2d;
    Vector2 move;
    public GameObject bulletPrefab;

    // Start is called before the first frame update
    void Start()
    {
        MoveAction.Enable();
        FireAction.Enable();
	    rigidbody2d = GetComponent<Rigidbody2D>();
    }

    void FireBullet()
    {
        Debug.Log("pow!");
        GameObject projectileObject = Instantiate(bulletPrefab, rigidbody2d.position, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();
        if (FireAction.IsPressed())
        {
            FireBullet();
        }
    }

    void FixedUpdate()
    {
        //Vector2 position = (Vector2)rigidbody2d.position + move * playerSpeed * Time.deltaTime;
        //rigidbody2d.MovePosition(position);
        //Vector3 position = transform.position;
        //position.x = position.x + move.x * playerSpeed * Time.deltaTime;
        //position.z = position.z + move.y * playerSpeed * Time.deltaTime;
        //transform.position = position;
        offset.x = offset.x + move.x * playerSpeed * Time.deltaTime;
        offset.z = offset.z - move.y * playerSpeed * Time.deltaTime;
        if (offset.z < 0) 
        {
            offset.z = 0;
        }
        offset.y = offset.z;
        transform.position = refObject.transform.position + offset;
        //Debug.Log(transform.position);
    }

    public Vector2 GetPosition()
    {
        return rigidbody2d.position;
    }

    public float GetAltitude()
    {
        return offset.z;
    }    
}
