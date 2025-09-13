using UnityEngine;
using System.Collections;

/// <summary>
/// Perfect dodge visual effect with particles and screen effects
/// </summary>
public class PerfectDodgeEffect : MonoBehaviour
{
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem sparkParticles;
    [SerializeField] private ParticleSystem ringEffect;
    [SerializeField] private GameObject glowRing;
    
    [Header("Animation")]
    [SerializeField] private float effectDuration = 1.5f;
    [SerializeField] private float expandSpeed = 2f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Colors")]
    [SerializeField] private Color effectColor = Color.yellow;
    [SerializeField] private Color secondaryColor = Color.white;

    private SpriteRenderer glowRenderer;
    private Vector3 originalScale;

    private void Start()
    {
        // Setup glow ring if available
        if (glowRing != null)
        {
            glowRenderer = glowRing.GetComponent<SpriteRenderer>();
            if (glowRenderer != null)
            {
                glowRenderer.color = new Color(effectColor.r, effectColor.g, effectColor.b, 0f);
                originalScale = glowRing.transform.localScale;
                glowRing.transform.localScale = Vector3.zero;
            }
        }

        // Setup particles
        SetupParticles();
        
        // Start effect
        StartCoroutine(PlayEffect());
    }

    private void SetupParticles()
    {
        // Setup spark particles
        if (sparkParticles != null)
        {
            var main = sparkParticles.main;
            main.startColor = effectColor;
            main.startLifetime = 0.8f;
            main.startSpeed = 5f;
            main.maxParticles = 30;
            
            var emission = sparkParticles.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0.0f, 30)
            });
            
            var shape = sparkParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
            
            sparkParticles.Play();
        }

        // Setup ring effect
        if (ringEffect != null)
        {
            var main = ringEffect.main;
            main.startColor = secondaryColor;
            main.startLifetime = 1.2f;
            main.startSpeed = 0f;
            main.maxParticles = 50;
            
            var emission = ringEffect.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0.1f, 50)
            });
            
            var shape = ringEffect.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 1f;
            
            var velocityOverLifetime = ringEffect.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(3f);
            
            ringEffect.Play();
        }
    }

    private IEnumerator PlayEffect()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < effectDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time for time slow compatibility
            float progress = elapsedTime / effectDuration;
            
            // Animate glow ring
            if (glowRenderer != null && glowRing != null)
            {
                // Scale animation
                float scaleMultiplier = scaleCurve.Evaluate(progress) * expandSpeed;
                glowRing.transform.localScale = originalScale * scaleMultiplier;
                
                // Alpha animation (fade in then out)
                float alpha;
                if (progress < 0.3f)
                {
                    alpha = Mathf.Lerp(0f, 0.8f, progress / 0.3f);
                }
                else
                {
                    alpha = Mathf.Lerp(0.8f, 0f, (progress - 0.3f) / 0.7f);
                }
                
                glowRenderer.color = new Color(effectColor.r, effectColor.g, effectColor.b, alpha);
            }
            
            yield return null;
        }
        
        // Cleanup
        Destroy(gameObject);
    }

    // Static factory method to create perfect dodge effect
    public static GameObject CreatePerfectDodgeEffect(Vector3 position)
    {
        GameObject effectPrefab = Resources.Load<GameObject>("Effects/PerfectDodgeEffect");
        
        if (effectPrefab == null)
        {
            // Create simple version if prefab doesn't exist
            return CreateSimplePerfectDodgeEffect(position);
        }
        
        return Instantiate(effectPrefab, position, Quaternion.identity);
    }

    private static GameObject CreateSimplePerfectDodgeEffect(Vector3 position)
    {
        GameObject effect = new GameObject("PerfectDodgeEffect");
        effect.transform.position = position;
        
        // Add this script
        PerfectDodgeEffect effectScript = effect.AddComponent<PerfectDodgeEffect>();
        
        // Create simple glow ring
        GameObject ring = new GameObject("GlowRing");
        ring.transform.parent = effect.transform;
        ring.transform.localPosition = Vector3.zero;
        
        SpriteRenderer sr = ring.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.sortingOrder = 10;
        
        effectScript.glowRing = ring;
        
        return effect;
    }

    private static Sprite CreateCircleSprite()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius && distance >= radius - 4f)
                {
                    float alpha = 1f - Mathf.Abs(distance - (radius - 2f)) / 2f;
                    colors[y * size + x] = new Color(1f, 1f, 0.5f, alpha);
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }
        
        tex.SetPixels(colors);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}