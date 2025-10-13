using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns spherical particles in a compact grid and attaches each to a spring-like anchor
/// so they stack and can vibrate when StartVibration() is called.
/// </summary>
[DisallowMultipleComponent]
public class SolidParticleStacker : MonoBehaviour
{
    [Header("Prefab & Physics")]
    [Tooltip("Prefab must have a SphereCollider and optionally a MeshRenderer.")]
    public GameObject particlePrefab;
    [Tooltip("Layer used by particles.")]
    public LayerMask particleLayer = ~0;

    [Header("Layout")]
    public int columns = 8;
    public int rows = 6;
    public int layers = 3;
    [Tooltip("Radius of individual particle (Unity units)")]
    public float particleRadius = 0.05f;
    [Tooltip("Extra spacing between particles (1 = touching)")]
    public float spacingMultiplier = 1.02f;
    [Tooltip("Where to start spawning (world position)")]
    public Vector3 startPosition = Vector3.zero;

    [Header("Joint / Vibration Physics")]
    public float spring = 100f;
    public float damper = 10f;
    public float particleMass = 0.02f;
    public float initialRandomImpulse = 0.02f;

    [Header("Continuous Vibration")]
    [Tooltip("Amplitude of anchor oscillation (meters)")]
    public float vibrationAmplitude = 0.005f;
    [Tooltip("Oscillation frequency (Hz)")]
    public float vibrationFrequency = 3f;
    [Tooltip("Use Perlin noise instead of pure sine for motion")]
    public bool usePerlin = true;

    [Header("Options")]
    [Tooltip("Clear and respawn automatically on Start")]
    public bool spawnOnStart = true;

    // Internal data structures
    private List<GameObject> particles = new List<GameObject>();
    private List<ParticleInfo> particleInfos = new List<ParticleInfo>();
    private Transform container;

    private bool isVibrating = false;
    public bool IsVibrating => isVibrating;

    private class ParticleInfo
    {
        public GameObject go;
        public Rigidbody rb;
        public SpringJoint sj;
        public Vector3 originalAnchor;
        public float seed;
    }

    // void Awake()
    // {
    //     if (spawnOnStart)
    //         Spawn();
    // }

    void OnEnable()
    {
        if (spawnOnStart)
            Spawn();
    }
    void OnDisable()
    {
        Clear();
    }

    [ContextMenu("Spawn")]
    public void Spawn()
    {
        Clear();

        if (particlePrefab == null)
        {
            Debug.LogError("SolidParticleStacker: particlePrefab not assigned.");
            return;
        }

        container = new GameObject("ParticlesContainer").transform;
        container.SetParent(transform, false);

        float spacing = particleRadius * 2f * spacingMultiplier;
        Vector3 baseOrigin = startPosition;

        for (int y = 0; y < layers; y++)
        {
            float yOffset = y * spacing * 0.95f; // vertical spacing
            for (int z = 0; z < rows; z++)
            {
                bool zOdd = (z % 2 == 1);
                for (int x = 0; x < columns; x++)
                {
                    float xOffset = x * spacing + (zOdd ? spacing * 0.5f : 0f);
                    Vector3 spawnPos = baseOrigin + new Vector3(
                        xOffset - (columns - 1) * spacing * 0.5f,
                        yOffset,
                        z * spacing - (rows - 1) * spacing * 0.5f
                    );

                    // small offset per layer for interlocking effect
                    spawnPos.y += y * (particleRadius * 0.2f);
                    CreateParticle(spawnPos);
                }
            }
        }

        // Ensure vibration is off after spawning (so they settle)
        isVibrating = false;
    }

    void CreateParticle(Vector3 worldPos)
    {
        GameObject go = Instantiate(particlePrefab, worldPos, Quaternion.identity, container);
        go.layer = gameObject.layer;

        // Collider
        SphereCollider sc = go.GetComponent<SphereCollider>();
        if (sc == null)
        {
            sc = go.AddComponent<SphereCollider>();
            sc.radius = 0.5f;
        }

        // Scale sphere to match desired radius
        float prefabScale = Mathf.Max(go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z);
        float currentRadius = prefabScale * sc.radius;
        float desiredScale = (particleRadius) / currentRadius * prefabScale;
        go.transform.localScale = Vector3.one * desiredScale;

        // Rigidbody
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.mass = particleMass;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.9f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        // NOTE: we do NOT force sleepThreshold = 0 here so particles can settle.

        // Spring joint to hold it near its anchor
        SpringJoint sj = go.GetComponent<SpringJoint>();
        if (sj == null) sj = go.AddComponent<SpringJoint>();
        sj.autoConfigureConnectedAnchor = false;
        sj.connectedBody = null;
        sj.connectedAnchor = worldPos;
        sj.anchor = Vector3.zero;
        sj.spring = spring;
        sj.damper = damper;
        sj.enableCollision = false;

        // Add small random force so initial settling isn't perfectly symmetric
        rb.AddForce(Random.onUnitSphere * initialRandomImpulse, ForceMode.Impulse);

        // Track info for vibration
        ParticleInfo info = new ParticleInfo
        {
            go = go,
            rb = rb,
            sj = sj,
            originalAnchor = worldPos,
            seed = Random.value * 100f
        };
        particleInfos.Add(info);
        particles.Add(go);
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        if (particles != null)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                if (particles[i] != null)
                    DestroyImmediate(particles[i]);
            }
            particles.Clear();
        }

        if (particleInfos != null)
            particleInfos.Clear();

        if (container != null)
        {
            DestroyImmediate(container.gameObject);
            container = null;
        }

        isVibrating = false;
    }

    /// <summary>
    /// Call this to start continuous vibration. Wakes rigidbodies so motion is visible.
    /// </summary>
    public void StartVibration()
    {
        if (isVibrating) return;
        isVibrating = true;

        // Wake all rigidbodies so vibration is visible immediately
        foreach (var p in particleInfos)
        {
            if (p?.rb != null) p.rb.WakeUp();

            // (re)store original anchor as baseline
            if (p?.sj != null) p.sj.connectedAnchor = p.originalAnchor;
        }
    }

    /// <summary>
    /// Call this to stop vibration. Anchors reset to original positions.
    /// </summary>
    public void StopVibration()
    {
        if (!isVibrating) return;
        isVibrating = false;

        // Reset anchors to original positions and optionally wake/sleep bodies
        foreach (var p in particleInfos)
        {
            if (p?.sj != null) p.sj.connectedAnchor = p.originalAnchor;
            // You can let bodies sleep naturally now; no forced WakeUp
        }
    }

    void FixedUpdate()
    {
        if (!isVibrating) return;
        if (particleInfos == null || particleInfos.Count == 0) return;

        float t = Time.time;
        float twoPiF = Mathf.PI * 2f * vibrationFrequency;

        foreach (var p in particleInfos)
        {
            if (p == null || p.sj == null) continue;

            Vector3 offset;

            if (usePerlin)
            {
                float s = p.seed;
                float nx = Mathf.PerlinNoise(t * vibrationFrequency + s, s) - 0.5f;
                float ny = Mathf.PerlinNoise(t * vibrationFrequency + s + 13.37f, s + 7.1f) - 0.5f;
                float nz = Mathf.PerlinNoise(t * vibrationFrequency + s + 42.0f, s + 21.0f) - 0.5f;
                offset = new Vector3(nx, ny, nz) * vibrationAmplitude * 2f;
            }
            else
            {
                float phase = p.seed;
                offset = new Vector3(
                    Mathf.Sin(t * twoPiF + phase),
                    Mathf.Sin(t * twoPiF * 1.1f + phase * 1.3f),
                    Mathf.Sin(t * twoPiF * 0.9f + phase * 0.7f)
                ) * vibrationAmplitude;
            }

            p.sj.connectedAnchor = p.originalAnchor + offset;

            // ensure rigidbody stays awake while vibrating
            if (p.rb != null) p.rb.WakeUp();
        }
    }
}
