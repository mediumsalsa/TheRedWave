using UnityEngine;

public class Entity : MonoBehaviour
{
    [HideInInspector] public bool isKnockedBack = false;
    [HideInInspector] public float knockbackDuration = 0.1f;
    [HideInInspector] public float knockbackForce = 2f;

    [HideInInspector] public float iFrameDuration = 2f;
    [HideInInspector] public bool areIFrames = false;
}
