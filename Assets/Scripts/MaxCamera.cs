using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxCamera : MonoBehaviour
{

   public Transform refObject;

   void Start() {}

   void Update()
   {
      transform.position = new Vector3(refObject.position.x, refObject.position.y, -10);
   }
}