using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class SceneOutput
{
    public List<GameObjectCollection> gameObjects;
    public List<EnemyHQ> enemyHQs;
    public float landingStripStartZ;
    public float landingStripEndZ;
    public float landingStripWidth;
    public GameObject riverSectionGameObject;
    public List<Vector3> riverVerts;
    public List<float> roadNearEdgesZ;
    public List<SceneRiverSegment> riverSegments;
}