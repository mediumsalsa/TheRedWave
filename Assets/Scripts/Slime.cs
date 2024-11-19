using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : MonoBehaviour
{

    public Transform target;
    private AIPath aiPath;

    void Start()
    {
        aiPath = GetComponent<AIPath>();
    }

    void Update()
    {
        if (target != null)
        {
            aiPath.destination = target.position; 
        }
    }

}
