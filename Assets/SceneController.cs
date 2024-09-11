using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public EnemyPlane enemyPlanePrefab;
    public ShadowControl shadowControlPrefab;

    void Start()
    {
        var startPos = transform.position;
        startPos.z = 0.2f;
        EnemyPlane enemyPlane = Instantiate(enemyPlanePrefab, startPos, Quaternion.identity);
        ShadowControl enemyShadow = Instantiate(shadowControlPrefab, transform.position, Quaternion.identity);
        enemyShadow.SetPlane(enemyPlane);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
