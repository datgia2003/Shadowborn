using UnityEngine;
using System.Collections;

/// <summary>
/// Simple dash trail effect for dodge skill
/// Creates a trail of fading sprites behind the player
/// </summary>
public class DashTrailEffect : MonoBehaviour
{
    [Header("Trail Settings")]
    [SerializeField] private int trailLength = 5;
    [SerializeField] private float trailSpacing = 0.1f;
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private Color trailColor = Color.cyan;
    
    [Header("Visual")]
    [SerializeField] private bool usePlayerSprite = true;
    [SerializeField] private Sprite customTrailSprite;

    private SpriteRenderer playerSpriteRenderer;
    private bool isCreatingTrail = false;

    private void Start()
    {
        // Get player sprite renderer if using player sprite
        if (usePlayerSprite)
        {
            playerSpriteRenderer = GetComponentInParent<SpriteRenderer>();
            if (playerSpriteRenderer == null)
            {
                playerSpriteRenderer = FindObjectOfType<PlayerController>()?.GetComponent<SpriteRenderer>();
            }
        }
        
        // Auto-start trail creation
        StartTrail();
    }

    public void StartTrail()
    {
        if (!isCreatingTrail)
        {
            StartCoroutine(CreateTrailCoroutine());
        }
    }

    public void StopTrail()
    {
        isCreatingTrail = false;
    }

    private IEnumerator CreateTrailCoroutine()
    {
        isCreatingTrail = true;
        
        for (int i = 0; i < trailLength && isCreatingTrail; i++)
        {
            CreateTrailSegment();
            yield return new WaitForSeconds(trailSpacing);
        }
        
        isCreatingTrail = false;
        
        // Destroy this effect object after trail is complete
        Destroy(gameObject, fadeTime + 1f);
    }

    private void CreateTrailSegment()
    {
        GameObject trailSegment = new GameObject("TrailSegment");
        trailSegment.transform.position = transform.position;
        trailSegment.transform.rotation = transform.rotation;
        
        SpriteRenderer sr = trailSegment.AddComponent<SpriteRenderer>();
        
        // Use player sprite or custom sprite
        if (usePlayerSprite && playerSpriteRenderer != null)
        {
            sr.sprite = playerSpriteRenderer.sprite;
            sr.flipX = playerSpriteRenderer.flipX;
            sr.flipY = playerSpriteRenderer.flipY;
        }
        else if (customTrailSprite != null)
        {
            sr.sprite = customTrailSprite;
        }
        else
        {
            // Create a simple square sprite as fallback
            sr.sprite = CreateSimpleSprite();
        }
        
        // Set trail color with transparency
        sr.color = new Color(trailColor.r, trailColor.g, trailColor.b, 0.7f);
        sr.sortingOrder = -1; // Behind player
        
        // Start fade effect
        StartCoroutine(FadeAndDestroy(sr, fadeTime));
    }

    private IEnumerator FadeAndDestroy(SpriteRenderer sr, float duration)
    {
        if (sr == null) yield break;
        
        Color startColor = sr.color;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration && sr != null)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        if (sr != null && sr.gameObject != null)
        {
            Destroy(sr.gameObject);
        }
    }

    private Sprite CreateSimpleSprite()
    {
        // Create a simple 1x1 white texture
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }
}