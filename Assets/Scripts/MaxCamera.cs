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
   float shakeCooldown;
   int shakeMovesLeft = 0;
   Vector3 targetLocalPosition;
   Vector3 shakeVelocity;

   void Start()
   {
      targetLocalPosition = new Vector3(0f, yOffset, -10f);
      transform.localPosition = targetLocalPosition;
   }

   void Update()
   {
      var diff = transform.localPosition - targetLocalPosition;
      transform.localPosition -= diff * correctionRate;

      if (shakeMovesLeft > 0)
      {
         transform.localPosition += shakeVelocity * Time.deltaTime;

         shakeCooldown -= Time.deltaTime;
         if (shakeCooldown <= 0)
         {
            shakeVelocity = new Vector3(
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
      shakeMovesLeft = shakeMoves;
   }
}