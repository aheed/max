using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    public MaxControl maxPlanePrefab;
    public EnemyPlane enemyPlanePrefab;
    public ShadowControl shadowControlPrefab;
    public refobj refobject;

    void AddPlaneShadow(IPositionObservable plane)
    {        
        ShadowControl shadow = Instantiate(shadowControlPrefab, transform.position, Quaternion.identity);
        shadow.SetPlane(plane);
    }

    void Start()
    {
        //var refobject = GetComponent<refobj>();
        var startPos = transform.position;
        /*startPos.x += 1.0f;
        startPos.y += 1.0f;
        startPos.z = 0.8f;*/
        MaxControl maxPlane = Instantiate(maxPlanePrefab, startPos, Quaternion.identity);
        maxPlane.refObject = refobject.transform;
        AddPlaneShadow(maxPlane);

        startPos = transform.position;
        startPos.x += 2.0f;
        startPos.y += 2.0f;
        startPos.z = 0.8f;
        EnemyPlane enemyPlane = Instantiate(enemyPlanePrefab, startPos, Quaternion.identity);
        AddPlaneShadow(enemyPlane);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
