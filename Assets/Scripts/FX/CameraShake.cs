using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    public float shakeDuration = 0.15f;
    public float shakeMagnitude = 0.2f;
    Vector3 originalPos;
    float shakeTime = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeTime > 0)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeMagnitude;
            shakeTime -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }

    public void Shake(float? duration = null, float? magnitude = null)
    {
        shakeTime = duration ?? shakeDuration;
        shakeMagnitude = magnitude ?? shakeMagnitude;
    }
}
