using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxCamera : MonoBehaviour
{   
   public float yOffset = 1.0f;
   public float correctionRate = 0.2f;
   public float shakeAmplitude = 0.3f;
   public int shakeMoves = 5;
   public float shakeMoveIntervalSec = 0.1f;
   Vector3 targetLocalPosition;
   public float shakeCooldown;
   public int shakeMovesLeft = 0;

   void Start()
   {
      //var parentPos = gameObject.transform.parent.position;
      //transform.position = new Vector3(parentPos.x, parentPos.y + yOffset, -10f);
      targetLocalPosition = new Vector3(0f, yOffset, -10f);
      transform.localPosition = targetLocalPosition;
   }

   void Update()
   {
      var diff = transform.localPosition - targetLocalPosition;
      transform.localPosition -= diff * correctionRate;

      if (shakeMovesLeft > 0)
      {
         shakeCooldown -= Time.deltaTime;
         if (shakeCooldown <= 0)
         {
            transform.localPosition += new Vector3(
               UnityEngine.Random.Range(-shakeAmplitude, shakeAmplitude),
               UnityEngine.Random.Range(-shakeAmplitude, shakeAmplitude),
               0f);
            shakeCooldown = shakeMoveIntervalSec;
            --shakeMovesLeft;
         }
      }
   }

   public void OnDetonation()
   {
      Debug.Log("Camera detects detonation..........................");
      shakeMovesLeft = shakeMoves;
   }
}