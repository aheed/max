using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MaxControl : MonoBehaviour
{
    public float playerSpeed = 3.0f;
    public InputAction MoveAction;
    Rigidbody2D rigidbody2d;
    Vector2 move;
    float altitude = 4.0f;

    // Start is called before the first frame update
    void Start()
    {
        MoveAction.Enable();
	    rigidbody2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        move = MoveAction.ReadValue<Vector2>();

    }

    void FixedUpdate()
    {
        Vector2 position = (Vector2)rigidbody2d.position + move * playerSpeed * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }

    public Vector2 GetPosition()
    {
        return rigidbody2d.position;
    }

    public float GetAltitude()
    {
        return altitude;
    }
}
