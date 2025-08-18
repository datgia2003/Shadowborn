using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [Tooltip("How quickly shake decays back to calm.")]
    public float traumaDecay = 1.5f; // per second
    [Tooltip("Noise frequency for shake.")]
    public float noiseFrequency = 25f; // Hz
    [Tooltip("Max camera noise amplitude for Cinemachine Perlin.")]
    public float maxAmplitudeGain = 1.2f;
    [Tooltip("Maximum positional offset (world units) when not using Cinemachine.")]
    public float maxOffset = 0.4f;

    [Header("Focus (FX drift)")]
    [Tooltip("Maximum drift offset when focusing on FX (world units).")]
    public float maxFocusOffset = 0.5f;
    [Tooltip("How strongly the camera drifts toward the focus point.")]
    public float focusStrength = 0.1f; // 0..1 multiplier
    [Tooltip("Smoothing time for focus drift.")]
    public float focusSmoothTime = 0.08f;

    [Header("Cinemachine Integration")]
    [Tooltip("Virtual Camera to drive. If null, will auto-find the first active vcam.")]
    public CinemachineVirtualCamera vcam;
    [Tooltip("Attach an ImpulseSource somewhere in the scene (often on the player) and assign here for directional pulses.")]
    public CinemachineImpulseSource impulseSource;
    [Tooltip("If true, will add a Perlin noise component to the vcam if missing.")]
    public bool autoAddPerlin = true;
    [Tooltip("Auto-find and drive the currently live vcam from CinemachineBrain.")]
    public bool autoFindActiveVcam = true;
    [Tooltip("NoiseSettings profile to use for Perlin. Create via Assets > Create > Cinemachine > Noise Settings.")]
    public NoiseSettings defaultNoiseProfile;

    CinemachineBrain brain;
    System.Collections.Generic.List<CinemachineBasicMultiChannelPerlin> perlinTargets = new System.Collections.Generic.List<CinemachineBasicMultiChannelPerlin>();
    System.Collections.IEnumerator zoomCR;
    float savedOrthoSize;
    bool zoomHeld;

    // runtime
    Vector3 baseLocalPos;
    float trauma; // 0..1
    float seedX, seedY;
    float t;

    // temp focus pulse
    Vector3 focusOffset; // local offset applied
    Vector3 focusVel;
    Vector3 pulseFocusWorld;
    float pulseFocusStrength;
    float pulseTimer;

    void Awake()
    {
        baseLocalPos = transform.localPosition;
        seedX = Random.value * 1000f;
        seedY = Random.value * 2000f;
        brain = FindObjectOfType<CinemachineBrain>();
        RefreshPerlinTargets();
    }

    void LateUpdate()
    {
        float dt = Time.unscaledDeltaTime;
        t += dt;
        // decay trauma
        trauma = Mathf.Max(0f, trauma - traumaDecay * dt);

        float shakeAmt = trauma * trauma; // nicer falloff

        // Optionally refresh active vcam during runtime (handles blends/switches)
        if (autoFindActiveVcam)
            RefreshPerlinTargets();

        // If Cinemachine Perlin exists, drive all targets
        if (perlinTargets.Count > 0)
        {
            foreach (var p in perlinTargets)
            {
                if (p == null) continue;
                p.m_AmplitudeGain = maxAmplitudeGain * shakeAmt;
                p.m_FrequencyGain = noiseFrequency;
            }
        }

        // Fallback transform-based shake (when no Cinemachine)
        float nx = (Mathf.PerlinNoise(seedX, t * noiseFrequency) - 0.5f) * 2f;
        float ny = (Mathf.PerlinNoise(seedY, t * noiseFrequency) - 0.5f) * 2f;
        Vector3 shake = new Vector3(nx, ny, 0f) * (maxOffset * shakeAmt);

        // focus pulse drift
        if (pulseTimer > 0f)
        {
            pulseTimer -= dt;
            // world delta to focus
            Vector3 worldDelta = pulseFocusWorld - transform.position;
            Vector2 planar = new Vector2(worldDelta.x, worldDelta.y) * Mathf.Max(0f, pulseFocusStrength);
            if (planar.sqrMagnitude > maxFocusOffset * maxFocusOffset)
            {
                planar = planar.normalized * maxFocusOffset;
            }
            Vector3 desired = new Vector3(planar.x, planar.y, 0f);
            focusOffset = Vector3.SmoothDamp(focusOffset, desired, ref focusVel, Mathf.Max(0.01f, focusSmoothTime));
        }
        else
        {
            // ease focus back to zero when no active pulse
            focusOffset = Vector3.SmoothDamp(focusOffset, Vector3.zero, ref focusVel, Mathf.Max(0.01f, focusSmoothTime));
        }

        // Only apply local offset when not controlled by Cinemachine (or if you mount this on a child rig)
        if (perlinTargets.Count == 0)
        {
            transform.localPosition = baseLocalPos + focusOffset + shake;
        }
    }

    // Back-compat. Adds trauma; duration is treated as a temp focus/persistence window.
    public void ShakeOnce(float duration, float magnitude)
    {
        AddTrauma(magnitude);
        pulseTimer = Mathf.Max(pulseTimer, duration);
        pulseFocusStrength = Mathf.Max(pulseFocusStrength, focusStrength);
        // Optional: emit a small impulse so Cinemachine reacts even without Perlin
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(magnitude);
        }
    }

    // Add shake and bias the camera slightly toward a world position for the given duration.
    public void PulseAt(Vector3 worldPos, float duration, float magnitude, float followStrength = -1f)
    {
        AddTrauma(magnitude);
        pulseFocusWorld = worldPos;
        pulseTimer = Mathf.Max(pulseTimer, duration);
        if (followStrength < 0f) followStrength = focusStrength;
        pulseFocusStrength = followStrength;
        if (impulseSource != null)
        {
            Vector3 dir = (worldPos - transform.position);
            if (dir.sqrMagnitude > 0.0001f) dir = dir.normalized * magnitude;
            impulseSource.GenerateImpulseAt(worldPos, dir);
        }
    }

    void RefreshPerlinTargets()
    {
        perlinTargets.Clear();

        // If explicit vcam assigned, use it
        if (vcam != null)
        {
            AddPerlinTargetsFor(vcam);
            return;
        }

        // Else try brain active vcam
        if (brain == null) brain = FindObjectOfType<CinemachineBrain>();
        if (brain != null && brain.ActiveVirtualCamera != null)
        {
            var icam = brain.ActiveVirtualCamera;
            var go = icam.VirtualCameraGameObject;
            if (go != null)
            {
                var v = go.GetComponent<CinemachineVirtualCamera>();
                if (v != null)
                {
                    AddPerlinTargetsFor(v);
                    return;
                }
                var fl = go.GetComponent<CinemachineFreeLook>();
                if (fl != null)
                {
                    AddPerlinTargetsFor(fl);
                    return;
                }
            }
        }

        // Fallback: first vcam in scene
        var anyVcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (anyVcam != null)
        {
            AddPerlinTargetsFor(anyVcam);
            return;
        }
        var anyFree = FindObjectOfType<CinemachineFreeLook>();
        if (anyFree != null)
        {
            AddPerlinTargetsFor(anyFree);
            return;
        }
    }

    void AddPerlinTargetsFor(CinemachineVirtualCamera v)
    {
        var p = v.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (p == null && autoAddPerlin) p = v.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (p != null)
        {
            if (p.m_NoiseProfile == null && defaultNoiseProfile != null)
                p.m_NoiseProfile = defaultNoiseProfile;
            p.m_AmplitudeGain = 0f;
            p.m_FrequencyGain = noiseFrequency;
            perlinTargets.Add(p);
        }
    }

    void AddPerlinTargetsFor(CinemachineFreeLook fl)
    {
        for (int i = 0; i < 3; i++)
        {
            var rig = fl.GetRig(i);
            if (rig == null) continue;
            var p = rig.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (p == null && autoAddPerlin) p = rig.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (p != null)
            {
                if (p.m_NoiseProfile == null && defaultNoiseProfile != null)
                    p.m_NoiseProfile = defaultNoiseProfile;
                p.m_AmplitudeGain = 0f;
                p.m_FrequencyGain = noiseFrequency;
                perlinTargets.Add(p);
            }
        }
    }

    public void AddTrauma(float amount)
    {
        trauma = Mathf.Clamp01(trauma + Mathf.Abs(amount));
    }

    // Zoom-in rồi trả lại, dành cho Orthographic vcam.
    public void ZoomPulse(float deltaSize, float inTime = 0.12f, float holdTime = 0.25f, float outTime = 0.25f)
    {
        if (vcam == null) { return; }
        if (!vcam.m_Lens.Orthographic) { return; }
        if (zoomCR != null) StopCoroutine(zoomCR);
        zoomCR = ZoomRoutine(deltaSize, inTime, holdTime, outTime);
        StartCoroutine(zoomCR);
    }

    System.Collections.IEnumerator ZoomRoutine(float deltaSize, float inTime, float holdTime, float outTime)
    {
        float start = vcam.m_Lens.OrthographicSize;
        float target = Mathf.Max(0.1f, start - Mathf.Abs(deltaSize));
        float tIn = 0f;
        while (tIn < inTime && inTime > 0f)
        {
            tIn += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(tIn / inTime);
            vcam.m_Lens.OrthographicSize = Mathf.Lerp(start, target, p);
            yield return null;
        }
        vcam.m_Lens.OrthographicSize = target;
        float tHold = 0f;
        while (tHold < holdTime)
        {
            tHold += Time.unscaledDeltaTime;
            yield return null;
        }
        float tOut = 0f;
        while (tOut < outTime && outTime > 0f)
        {
            tOut += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(tOut / outTime);
            vcam.m_Lens.OrthographicSize = Mathf.Lerp(target, start, p);
            yield return null;
        }
        vcam.m_Lens.OrthographicSize = start;
        zoomCR = null;
    }

    // Bắt đầu zoom và giữ cho đến khi gọi ZoomHoldEnd.
    public void ZoomHoldStart(float deltaSize, float inTime = 0.12f)
    {
        if (vcam == null) return;
        if (!vcam.m_Lens.Orthographic) return;
        if (zoomCR != null) StopCoroutine(zoomCR);
        savedOrthoSize = vcam.m_Lens.OrthographicSize;
        zoomCR = ZoomInRoutine(Mathf.Max(0.1f, savedOrthoSize - Mathf.Abs(deltaSize)), inTime);
        StartCoroutine(zoomCR);
    }

    // Kết thúc zoom giữ và trả lens size về ban đầu.
    public void ZoomHoldEnd(float outTime = 0.25f)
    {
        if (vcam == null) return;
        if (!vcam.m_Lens.Orthographic) return;
        if (!zoomHeld)
        {
            if (zoomCR != null) { StopCoroutine(zoomCR); zoomCR = null; }
            return;
        }
        if (zoomCR != null) StopCoroutine(zoomCR);
        zoomCR = ZoomOutRoutine(savedOrthoSize, outTime);
        StartCoroutine(zoomCR);
    }

    System.Collections.IEnumerator ZoomInRoutine(float target, float inTime)
    {
        float start = vcam.m_Lens.OrthographicSize;
        float tIn = 0f;
        while (tIn < inTime && inTime > 0f)
        {
            tIn += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(tIn / inTime);
            vcam.m_Lens.OrthographicSize = Mathf.Lerp(start, target, p);
            yield return null;
        }
        vcam.m_Lens.OrthographicSize = target;
        zoomHeld = true;
        zoomCR = null;
    }

    System.Collections.IEnumerator ZoomOutRoutine(float restoreSize, float outTime)
    {
        float start = vcam.m_Lens.OrthographicSize;
        float tOut = 0f;
        while (tOut < outTime && outTime > 0f)
        {
            tOut += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(tOut / outTime);
            vcam.m_Lens.OrthographicSize = Mathf.Lerp(start, restoreSize, p);
            yield return null;
        }
        vcam.m_Lens.OrthographicSize = restoreSize;
        zoomHeld = false;
        zoomCR = null;
    }
}

