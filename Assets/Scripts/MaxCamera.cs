using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxCamera : MonoBehaviour
{
   public float yOffset = 1.0f;

   void Start()
   {
      var parentPos = gameObject.transform.parent.position;
      transform.position = new Vector3(parentPos.x, parentPos.y + yOffset, -10f);
   }
}