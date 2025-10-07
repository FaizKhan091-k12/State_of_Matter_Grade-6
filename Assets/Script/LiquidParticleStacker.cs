using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawn particles that initially stack. When StartLiquid() is called they switch to a
/// neighbour-driven "liquid" behaviour: cohesion + separation + viscosity + turbulence.
/// Not a full SPH solver, but good for classroom/visual demos.
/// </summary>
[DisallowMultipleComponent]
public class LiquidParticleStacker : MonoBehaviour
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
    public float particleRadius = 0.02f;
    [Tooltip("Extra spacing between particles (1 = touching)")]
    public float spacingMultiplier = 1.01f;
    [Tooltip("Start spawn center (world position)")]
    public Vector3 startPosition = Vector3.zero;
    [Tooltip("Spawn only inside a circle of this radius (0 = no circle)")]
    public float spawnCircleRadius = 0f;

    [Header("Initial / Stacked Physics")]
    [Tooltip("When spawned, the Rigidbodies start as kinematic (they stack).")]
    public bool startKinematic = true;
    [Tooltip("Mass of each particle while stacked")]
    public float stackedMass = 0.02f;

    [Header("Liquid Behaviour Settings")]
    [Tooltip("Radius to consider neighbors for forces (usually ~ 1.5 * particleDiameter)")]
    public float neighborRadius = 0.045f;
    [Tooltip("Strength pulling particle toward neighbour center (cohesion)")]
    public float cohesionStrength = 20f;
    [Tooltip("Strength pushing particles apart when too close (separation)")]
    public float separationStrength = 60f;
    [Tooltip("Preferred minimum distance between particles (approx)")]
    public float separationDistanceMultiplier = 0.9f;
    [Tooltip("Viscosity: damps relative velocities with neighbours")]
    public float viscosity = 0.5f;
    [Tooltip("Small global turbulence force (random)")]
    public float turbulenceStrength = 0.02f;
    [Tooltip("Global flow force applied to every particle (simulate current)")]
    public Vector3 globalFlow = Vector3.zero;
    [Tooltip("Mass when in liquid state (affects responsiveness)")]
    public float liquidMass = 0.015f;

    [Header("Options")]
    [Tooltip("Spawn particles on Awake")]
    public bool spawnOnStart = true;
    [Tooltip("If true, switch to liquid automatically once spawned")]
    public bool autoBecomeLiquid = false;

    // internals
    private Transform container;
    private List<GameObject> particles = new List<GameObject>();
    private List<Rigidbody> rbs = new List<Rigidbody>();
    private bool isLiquid = false;

    void Awake()
    {
        if (spawnOnStart) Spawn();
        if (autoBecomeLiquid) StartLiquid();
    }

    #region Spawn / Clear
    [ContextMenu("Spawn")]
    public void Spawn()
    {
        Clear();

        if (particlePrefab == null)
        {
            Debug.LogError("LiquidParticleStacker: particlePrefab not assigned.");
            return;
        }

        container = new GameObject("LiquidParticlesContainer").transform;
        container.SetParent(transform, false);

        float spacing = particleRadius * 2f * spacingMultiplier;
        Vector3 baseOrigin = startPosition;

        for (int y = 0; y < layers; y++)
        {
            float yOffset = y * spacing * 0.95f; // slight overlap for tighter packing
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

                    // if spawnCircleRadius > 0, skip those outside the circle (XZ-plane)
                    if (spawnCircleRadius > 0f)
                    {
                        Vector2 localXZ = new Vector2(spawnPos.x - baseOrigin.x, spawnPos.z - baseOrigin.z);
                        if (localXZ.magnitude > spawnCircleRadius) continue;
                    }

                    CreateParticle(spawnPos);
                }
            }
        }

        // set stacked/kinematic state
        isLiquid = false;
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

        if (container != null)
        {
            DestroyImmediate(container.gameObject);
            container = null;
        }

        isLiquid = false;
    }

    void CreateParticle(Vector3 pos)
    {
        GameObject go = Instantiate(particlePrefab, pos, Quaternion.identity, container);
        go.layer = gameObject.layer;

        SphereCollider sc = go.GetComponent<SphereCollider>();
        if (sc == null)
        {
            sc = go.AddComponent<SphereCollider>();
            sc.radius = 0.5f; // typical default
        }

        // scale so collider radius matches particleRadius
        float prefabScale = Mathf.Max(go.transform.localScale.x, go.transform.localScale.y, go.transform.localScale.z);
        float currentRadius = prefabScale * sc.radius;
        float desiredScale = (particleRadius) / currentRadius * prefabScale;
        go.transform.localScale = Vector3.one * desiredScale;

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.mass = stackedMass;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.9f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // small nudge so they don't perfectly interlock
        rb.AddForce(Random.onUnitSphere * Mathf.Max(0.002f, particleRadius * 0.2f), ForceMode.Impulse);

        particles.Add(go);
        rbs.Add(rb);
    }
    #endregion

    #region Mode Control
    /// <summary>
    /// Switches particles to liquid behaviour (dynamic forces applied).
    /// </summary>
    public void StartLiquid()
    {
        if (isLiquid) return;
        isLiquid = true;
        SetKinematicForAll(false, liquidMass);
        // optionally wake all rigidbodies
        foreach (var rb in rbs) if (rb != null) rb.WakeUp();
    }

    /// <summary>
    /// Stops liquid behaviour, sets particles kinematic (they will freeze in place).
    /// </summary>
    public void StopLiquid(bool setKinematic = true)
    {
        isLiquid = false;
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
            if (!kinematic)
            {
                // tune drag for liquid
                rb.linearDamping = Mathf.Clamp(0.2f, 0f, 5f);
                rb.angularDamping = 0.5f;
                rb.sleepThreshold = 0f; // keep awake while liquid
            }
            else
            {
                // when stacked we allow them to sleep naturally
                rb.linearDamping = 0.6f;
                rb.angularDamping = 0.9f;
                rb.sleepThreshold = 0.005f;
            }
        }
    }
    #endregion

    #region Simple Liquid Solver (neighbour forces)
    void FixedUpdate()
    {
        if (!isLiquid || rbs.Count == 0) return;

        // Pre-alloc arrays for speed (if necessary) - here keep simple loops
        float neighborR = Mathf.Max(0.0001f, neighborRadius);
        float neighborRSqr = neighborR * neighborR;
        float preferredSeparation = particleRadius * 2f * separationDistanceMultiplier;

        // For each particle, gather neighbours and compute forces
        for (int i = 0; i < rbs.Count; i++)
        {
            var rb = rbs[i];
            if (rb == null) continue;

            Vector3 pos = rb.position;

            // Use Physics.OverlapSphere to find neighbours (colliders on particle layer)
            Collider[] hits = Physics.OverlapSphere(pos, neighborR, particleLayer);

            // accumulate neighbour info
            Vector3 center = Vector3.zero;
            Vector3 separationForce = Vector3.zero;
            Vector3 relativeVelSum = Vector3.zero;
            int neighbourCount = 0;

            for (int h = 0; h < hits.Length; h++)
            {
                Collider c = hits[h];
                if (c == null || c.attachedRigidbody == null) continue;
                Rigidbody otherRb = c.attachedRigidbody;
                if (otherRb == rb) continue;

                Vector3 otherPos = otherRb.position;
                Vector3 diff = otherPos - pos;
                float dsqr = diff.sqrMagnitude;
                if (dsqr < 1e-6f) continue;

                neighbourCount++;
                center += otherPos;
                relativeVelSum += otherRb.linearVelocity;

                float dist = Mathf.Sqrt(dsqr);
                // separation: if too close, push away
                if (dist < preferredSeparation && dist > 0f)
                {
                    // stronger when closer
                    float push = (preferredSeparation - dist) / Mathf.Max(0.0001f, preferredSeparation);
                    separationForce -= diff.normalized * (push * separationStrength);
                }
            }

            // cohesion: pull towards average neighbor center
            Vector3 cohesionForce = Vector3.zero;
            if (neighbourCount > 0)
            {
                center /= neighbourCount;
                Vector3 toCenter = (center - pos);
                cohesionForce = toCenter * (cohesionStrength * 0.5f);
            }

            // viscosity: damp relative velocity with neighbours
            Vector3 viscosityForce = Vector3.zero;
            if (neighbourCount > 0)
            {
                Vector3 avgVel = relativeVelSum / neighbourCount;
                Vector3 relVel = (avgVel - rb.linearVelocity);
                viscosityForce = relVel * viscosity;
            }

            // turbulence + global flow
            Vector3 turbulence = (Random.insideUnitSphere * turbulenceStrength) + globalFlow;

            // total
            Vector3 total = cohesionForce + separationForce + viscosityForce + turbulence;

            // apply scaled by mass for stability (use ForceMode.Acceleration or Force)
            rb.AddForce(total, ForceMode.Acceleration);
        }
    }
    #endregion

    #region Debug & Utility
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
        Gizmos.DrawWireSphere(startPosition, neighborRadius);
    }
    #endregion
}
