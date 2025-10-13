using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GasParticleStacker:
/// - Spawns sphere particles in a compact stack
/// - Particles remain kinematic (stacked) until StartGas() is called
/// - When gas is active particles become dynamic and receive Brownian + buoyancy forces to simulate gas motion
/// </summary>
[DisallowMultipleComponent]
public class GasParticleStacker : MonoBehaviour
{
    [Header("Prefab & Physics")]
    [Tooltip("Simple sphere prefab (with SphereCollider). Rigidbody will be added by script.")]
    public GameObject particlePrefab;
    [Tooltip("Layer mask used for particle neighbor queries.")]
    public LayerMask particleLayer = ~0;

    [Header("Layout")]
    public int columns = 8;
    public int rows = 6;
    public int layers = 3;
    [Tooltip("Radius of each particle (Unity units)")]
    public float particleRadius = 0.02f;
    [Tooltip("Spacing multiplier between particles (1 = touching)")]
    public float spacingMultiplier = 1.01f;
    [Tooltip("World position center for spawn")]
    public Vector3 startPosition = Vector3.zero;
    [Tooltip("Use circular spawn area (0 = disabled)")]
    public float spawnCircleRadius = 0f;

    [Header("Initial (stacked) settings")]
    [Tooltip("Should particles be created as kinematic so they stack?")]
    public bool startKinematic = true;
    [Tooltip("Mass while stacked")]
    public float stackedMass = 0.02f;

    [Header("Gas behaviour settings")]
    [Tooltip("When gas starts, mass lowers to this")]
    public float gasMass = 0.0025f;
    [Tooltip("Drag while gas (low for free movement)")]
    public float gasDrag = 0.05f;
    [Tooltip("Random / Brownian force magnitude (like temperature)")]
    public float temperatureForce = 0.8f;
    [Tooltip("Upward acceleration simulating buoyancy / diffusion")]
    public float buoyancy = 0.6f;
    [Tooltip("Neighbor separation - pushes particles apart when too close")]
    public float separationStrength = 1.2f;
    [Tooltip("Distance used for neighbor separation (approx)")]
    public float separationDistance = 0.04f;
    [Tooltip("If true, particles will gently expand to fill available space")]
    public bool enableExpansion = true;

    [Header("Options")]
    public bool spawnOnStart = true;
    public bool autoBecomeGas = false;

    // internals
    private Transform container;
    private List<GameObject> particles = new List<GameObject>();
    private List<Rigidbody> rbs = new List<Rigidbody>();
    private bool isGas = false;

    // void Awake()
    // {
    //     if (spawnOnStart) Spawn();
    //     if (autoBecomeGas) StartGas();
    // }
    void OnEnable()
    {
        
        if (spawnOnStart) Spawn();
        if (autoBecomeGas) StartGas();
    }

    void OnDisable()
    {
        Clear();
    }

    #region Spawn / Clear
    [ContextMenu("Spawn")]
    public void Spawn()
    {
        Clear();

        if (particlePrefab == null)
        {
            Debug.LogError("GasParticleStacker: particlePrefab not assigned.");
            return;
        }

        container = new GameObject("GasParticlesContainer").transform;
        container.SetParent(transform, false);

        float spacing = particleRadius * 2f * spacingMultiplier;
        Vector3 baseOrigin = startPosition;

        for (int y = 0; y < layers; y++)
        {
            float yOffset = y * spacing * 0.95f; // slight overlap for compact packing
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

                    // small vertical staggering
                    spawnPos.y += y * (particleRadius * 0.12f);

                    // if using circular spawn, skip outside
                    if (spawnCircleRadius > 0f)
                    {
                        Vector2 localXZ = new Vector2(spawnPos.x - baseOrigin.x, spawnPos.z - baseOrigin.z);
                        if (localXZ.magnitude > spawnCircleRadius) continue;
                    }

                    CreateParticle(spawnPos);
                }
            }
        }

        // initial state: stacked/kinematic if asked
        isGas = false;
        SetKinematicForAll(startKinematic, stackedMass);
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        foreach (var g in particles)
        {
            if (g != null) DestroyImmediate(g);
        }
        particles.Clear();
        rbs.Clear();

        if (container != null) { DestroyImmediate(container.gameObject); container = null; }

        isGas = false;
    }

    void CreateParticle(Vector3 pos)
    {
        GameObject go = Instantiate(particlePrefab, pos, Quaternion.identity, container);
        go.layer = gameObject.layer;

        SphereCollider sc = go.GetComponent<SphereCollider>();
        if (sc == null)
        {
            sc = go.AddComponent<SphereCollider>();
            sc.radius = 0.5f;
        }

        // scale prefab to desired particleRadius
        float prefabScale = Mathf.Max(go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z);
        float currentRadius = prefabScale * sc.radius;
        float desiredScale = (particleRadius) / currentRadius * prefabScale;
        go.transform.localScale = Vector3.one * desiredScale;

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.mass = stackedMass;
        rb.linearDamping = 0.6f;
        rb.angularDamping = 0.9f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // small nudge
        rb.AddForce(Random.onUnitSphere * Mathf.Max(0.002f, particleRadius * 0.2f), ForceMode.Impulse);

        particles.Add(go);
        rbs.Add(rb);
    }
    #endregion

    #region Mode control
    public void StartGas()
    {
        if (isGas) return;
        isGas = true;
        SetGasState(true);
    }

    public void StopGas(bool setKinematic = true)
    {
        isGas = false;
        SetGasState(false);
        if (setKinematic) SetKinematicForAll(true, stackedMass);
    }

    void SetKinematicForAll(bool kinematic, float mass)
    {
        for (int i = 0; i < rbs.Count; i++)
        {
            var rb = rbs[i];
            if (rb == null) continue;
            rb.isKinematic = kinematic;
            rb.mass = mass;
            if (kinematic)
            {
                rb.linearDamping = 0.6f;
                rb.angularDamping = 0.9f;
                rb.sleepThreshold = 0.005f;
            }
            else
            {
                rb.linearDamping = gasDrag;
                rb.angularDamping = 0.05f;
                rb.sleepThreshold = 0f;
            }
        }
    }

    void SetGasState(bool gasOn)
    {
        for (int i = 0; i < rbs.Count; i++)
        {
            var rb = rbs[i];
            if (rb == null) continue;
            rb.isKinematic = !gasOn;
            rb.mass = gasOn ? gasMass : stackedMass;
            rb.linearDamping = gasOn ? gasDrag : 0.6f;
            rb.sleepThreshold = gasOn ? 0f : 0.005f;

            if (gasOn)
            {
                // small wake and random kick so motion starts immediately
                rb.WakeUp();
                rb.AddForce(Random.onUnitSphere * (temperatureForce * 0.1f), ForceMode.VelocityChange);
            }
        }
    }
    #endregion

    #region Gas forces (FixedUpdate)
    void FixedUpdate()
    {
        if (!isGas || rbs.Count == 0) return;

        // Precompute values
        float sepDist = Mathf.Max(0.001f, separationDistance);
        float sepDistSqr = sepDist * sepDist;
        float temp = Mathf.Max(0f, temperatureForce);
        Vector3 buoy = Vector3.up * buoyancy;

        // For each particle apply Brownian + buoyancy, plus basic separation
        for (int i = 0; i < rbs.Count; i++)
        {
            var rb = rbs[i];
            if (rb == null || rb.isKinematic) continue;

            Vector3 pos = rb.position;

            // Brownian / turbulence: random force scaled by temperature and local variance
            Vector3 brownian = Random.insideUnitSphere * temp * 0.5f;

            // gentle upward push (buoyancy / diffusion)
            Vector3 totalForce = brownian + buoy;

            // add a little expansion force (push away from cluster center) if enabled
            if (enableExpansion)
            {
                // approximate cluster center by transform position
                Vector3 fromCenter = (pos - startPosition);
                totalForce += fromCenter.normalized * (temp * 0.05f);
            }

            // neighbour separation: push away from very close colliders
            if (separationStrength > 0f)
            {
                Collider[] hits = Physics.OverlapSphere(pos, sepDist, particleLayer);
                if (hits.Length > 0)
                {
                    Vector3 sep = Vector3.zero;
                    for (int h = 0; h < hits.Length; h++)
                    {
                        Collider c = hits[h];
                        if (c == null || c.attachedRigidbody == null) continue;
                        Rigidbody other = c.attachedRigidbody;
                        if (other == rb) continue;
                        Vector3 d = pos - other.position;
                        float dsqr = d.sqrMagnitude;
                        if (dsqr < 1e-6f) continue;
                        float inv = Mathf.Clamp01(1f - (Mathf.Sqrt(dsqr) / sepDist));
                        sep += d.normalized * (inv * separationStrength * 0.2f);
                    }
                    totalForce += sep;
                }
            }

            // global upward bias stronger near bottom (simulate gas rising from source)
            float heightFactor = Mathf.InverseLerp(startPosition.y, startPosition.y + layers * particleRadius * 2f * 2f, rb.position.y);
            float localBuoyancy = buoyancy * (1f - heightFactor); // stronger near bottom
            totalForce += Vector3.up * localBuoyancy * 0.5f;

            // Apply as acceleration for stable behaviour
            rb.AddForce(totalForce, ForceMode.Acceleration);
        }
    }
    #endregion

    #region Utilities
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(startPosition, 0.02f);
        if (spawnCircleRadius > 0f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startPosition, spawnCircleRadius);
        }
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(startPosition, separationDistance);
    }
    #endregion
}
