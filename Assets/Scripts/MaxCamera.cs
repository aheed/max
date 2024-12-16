using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxCamera : MonoBehaviour
{   
   public float yOffset = 1.0f;
   public float tvScale = 0.9f;
   public float tvOffsetX = -0.1f;
   public float tvOffsetY = 0.1f;
   public float correctionRate = 0.2f;
   public float shakeAmplitude = 0.3f;
   public int shakeMoves = 5;
   public float shakeMoveIntervalSec = 0.1f;
   float shakeCooldown;
   int shakeMovesLeft = 0;
   Vector3 targetLocalPosition;
   Vector3 shakeVelocity;
   Camera cameraComponent;

   void Start()
   {
      targetLocalPosition = new Vector3(0f, yOffset, -10f);
      transform.localPosition = targetLocalPosition;
      cameraComponent = gameObject.GetComponent<Camera>();
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

   public void OnViewModeChanged()
   {
      var viewMode = FindAnyObjectByType<GameState>().viewMode;
      if (viewMode == ViewMode.NORMAL)
      {
         cameraComponent.rect = new Rect( ) { x = 0, y = 0, width = 1, height = 1};          
      }
      else if (viewMode == ViewMode.TV_SIM)
      {
         cameraComponent.rect = new Rect() {
            x = (1f-tvScale)/2 + tvOffsetX,
            y = (1f-tvScale)/2 + tvOffsetY,
            width = tvScale,
            height = tvScale};
      }
   }
}