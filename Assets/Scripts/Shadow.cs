using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Shadow : MonoBehaviour
{
    [Header("Shadow Settings")]
    public Sprite shadowSprite;    
    public Vector2 shadowOffset = new Vector2(0, -0.1f); 
    public Color shadowColor = new Color(0, 0, 0, 0.5f); 
    public float shadowScale = 1f;     

    private GameObject shadowObject;
    private SpriteRenderer shadowRenderer;

    private void Start()
    {
        // Create a shadow GameObject
        shadowObject = new GameObject("Shadow");
        shadowObject.transform.SetParent(transform); // Make shadow a child of this GameObject
        shadowObject.transform.localPosition = shadowOffset; // Apply initial offset

        // Add a SpriteRenderer to render the shadow
        shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = shadowSprite;
        shadowRenderer.color = shadowColor; // Set shadow color
        shadowRenderer.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder - 1; // Render beneath the entity

        // Scale the shadow
        shadowObject.transform.localScale = Vector3.one * shadowScale;
    }

    private void Update()
    {
        // Ensure shadow stays at the offset position
        if (shadowObject != null)
        {
            shadowObject.transform.localPosition = shadowOffset;
        }
    }
}
