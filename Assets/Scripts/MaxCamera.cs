using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxCamera : MonoBehaviour
{

   public Transform refObject;
   public float yOffset = 1.0f;

   void Start() {}

   void Update()
   {
      transform.position = new Vector3(refObject.position.x, refObject.position.y + yOffset, -10);
   }
}