using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEventHandler : MonoBehaviour, IGameStateObserver
{   
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
    Transform cameraTransform;

    void Start()
    {
        // Assume the camera is a component of the parent game object
        cameraTransform = transform.parent;
        cameraComponent = cameraTransform.gameObject.GetComponent<Camera>();
        OnViewModeChanged();
        GameState.GetInstance().RegisterObserver(this);
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
        var viewMode = GameState.GetInstance().viewMode;
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

    public void OnGameEvent(GameEvent gameEvent)
    {
        if (gameEvent == GameEvent.BIG_DETONATION)
        {
            OnDetonation();
        }
        else if (gameEvent == GameEvent.VIEW_MODE_CHANGED)
        {
            OnViewModeChanged();
        }
    }

    public void OnGameStatusChanged(GameStatus gameStatus) {}

    public void OnBombLanded(GameObject bomb, GameObject hitObject) {}

    public void OnEnemyPlaneStatusChanged(EnemyPlane enemyPlane, bool active) {}
}