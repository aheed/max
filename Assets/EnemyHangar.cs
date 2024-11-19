using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHangar : MonoBehaviour
{
    public FlipBook bombed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {        
        if (!col.name.StartsWith("bomb"))
        {
            return;
        }

        bombed.Activate();
        
        var collider = gameObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Todo: report destroyed enemy hangar for scoring
    }
}
