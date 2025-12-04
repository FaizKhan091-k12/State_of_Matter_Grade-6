// Draggable.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class Draggable : MonoBehaviour
{
    public L1Audiomanager l1Audiomanager;
    public SimulationManager simulationManager;
    public bool isCube;
    public ShapeType shapeType = ShapeType.Cube;

    [Header("Drag / Camera")]
    public Camera cam;
    public float fixedZ = -999f;

    [Header("Collision")]
    public LayerMask obstacleLayers;
    public float collisionPadding = 0.01f;

    [Header("Return / Snap")]
    public bool smoothReturn = true;
    public float returnSpeed = 8f;
    public float snapDuration = 0.25f;
    public float snapBounceScale = 1.08f;

    [Header("Placement")]
    public PlacementZone placementZone;

    [Header("Visuals")]
    public GameObject outline;

    [Header("Parachutes (assign in inspector)")]
    public GameObject smallParachute;
    public GameObject largeParachute;

    [Header("Animation")]
    [Tooltip("Name of the legacy Animation clip on parachute GameObjects.")]
    public string parachuteClipName = "Object_0";

    // runtime state
    public bool InPlacementZone { get; private set; } = false;
    public bool IsPlaced { get; private set; } = false;

    // internals
    private Collider objectCollider;
    private Rigidbody _rb;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float originalY;
    private float originalZ;
    private Vector3 colliderCenterOffset;
    private Vector3 worldExtents;

    // drag state
    private bool dragging = false;
    private int pointerId = -1;

    // physics move queue
    private Vector3 _queuedPosition;
    private bool _hasQueuedPosition = false;

    private Coroutine smoothReturnRoutine = null;
    private Coroutine snapRoutine = null;

    private const float ORIGINAL_POS_EPS = 0.001f;

    // simulation state & timer
    private bool _isSimulating = false;
    public bool IsSimulating => _isSimulating;
    private float _simStartTime = 0f;
    public float SimulationElapsed { get; private set; } = 0f;

    // events (optional)
    public Action OnSimulationStarted;
    public Action<float> OnSimulationEnded; // passes final time

    private void Awake()
    {
        EnsureRigidbody();
        objectCollider = GetComponent<Collider>();
        if (cam == null) cam = Camera.main;

        if (placementZone == null)
            placementZone = FindObjectOfType<PlacementZone>();

        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalY = originalPosition.y;
        originalZ = (fixedZ > -999f) ? fixedZ : originalPosition.z;

        UpdateColliderInfo();
        UpdateOutlineInitial();

        // default parachutes off
        SetParachuteActive(false, false);
        if (placementZone != null) placementZone.ShowPlacement(false);
    }

    private void Start()
    {
        if (placementZone == null) placementZone = FindObjectOfType<PlacementZone>();
    }

    private void EnsureRigidbody()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
            _rb = gameObject.AddComponent<Rigidbody>();

        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.interpolation = RigidbodyInterpolation.None;
        _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
    }

    public void SetPlacementZone(PlacementZone zone)
    {
        placementZone = zone;
        if (placementZone != null) placementZone.ShowPlacement(false);
    }

    private void OnEnable()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalY = originalPosition.y;
        if (fixedZ <= -999f) originalZ = originalPosition.z;
        UpdateColliderInfo();
        UpdateOutlineInitial();
    }

    private void UpdateColliderInfo()
    {
        if (objectCollider == null) objectCollider = GetComponent<Collider>();
        Bounds b = objectCollider.bounds;
        worldExtents = b.extents - Vector3.one * collisionPadding;
        worldExtents.x = Mathf.Max(worldExtents.x, 0.01f);
        worldExtents.y = Mathf.Max(worldExtents.y, 0.01f);
        worldExtents.z = Mathf.Max(worldExtents.z, 0.01f);
        colliderCenterOffset = b.center - transform.position;
    }

    private void UpdateOutlineInitial()
    {
        if (outline == null) return;
        bool atOriginal = Vector3.Distance(transform.position, originalPosition) <= ORIGINAL_POS_EPS;
        outline.SetActive(!IsPlaced && atOriginal && !dragging);
    }

    private void Update()
    {
        if (IsPlaced || _isSimulating) return;

        // Mouse
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            TryStartDrag(Input.mousePosition, 0);

        if (Input.GetMouseButtonUp(0) && dragging && pointerId == 0)
            EndDrag();

        if (dragging && pointerId == 0)
            ContinueDrag(Input.mousePosition);

        // Touch
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began && !IsPointerOverUI())
                TryStartDrag(t.position, 0);

            if (dragging && pointerId == 0)
            {
                if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                    ContinueDrag(t.position);
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    EndDrag();
            }
        }
    }

    private void TryStartDrag(Vector2 screenPos, int id)
    {
        Ray r = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(r, out RaycastHit hit, 100f))
        {
            if (hit.collider == objectCollider)
            {
                dragging = true;
                pointerId = id;
                if (smoothReturnRoutine != null) { StopCoroutine(smoothReturnRoutine); smoothReturnRoutine = null; }
                UpdateColliderInfo();

                if (AdvancedOrbitCamera.instance != null) AdvancedOrbitCamera.instance.canOrbit = false;

                if (outline != null) outline.SetActive(false);
                if (placementZone != null) placementZone.ShowPlacementForDraggable(this, true);
            }
        }
    }

    private void ContinueDrag(Vector2 screenPos)
    {
        if (!dragging) return;

        float depth = cam.WorldToScreenPoint(transform.position).z;
        Vector3 sp = new Vector3(screenPos.x, screenPos.y, depth);
        Vector3 wp = cam.ScreenToWorldPoint(sp);

        float clampedY = Mathf.Max(wp.y, originalY);
        Vector3 desired = new Vector3(wp.x, clampedY, originalZ);

        if (CanMoveTo(desired))
        {
            _queuedPosition = desired;
            _hasQueuedPosition = true;
        }
        else
        {
            _hasQueuedPosition = false;
        }
    }

    private void FixedUpdate()
    {
        if (_hasQueuedPosition && _rb != null && !IsPlaced && !_isSimulating)
        {
            _rb.MovePosition(_queuedPosition);
            _hasQueuedPosition = false;
        }
    }

    private void EndDrag()
    {
        dragging = false;
        pointerId = -1;

        if (placementZone != null) placementZone.ShowPlacementForDraggable(this, false);

        if (placementZone == null) placementZone = FindObjectOfType<PlacementZone>();





        bool trackedInside = placementZone != null && placementZone.IsDraggableInside(this);
        bool geoInside = placementZone != null && placementZone.ContainsPoint(transform.position);
        bool actuallyInside = trackedInside && geoInside;

        if (actuallyInside && placementZone != null)
        {

            if (isCube)
            {
                if (l1Audiomanager == null && simulationManager == null)
                {
                    simulationManager = FindFirstObjectByType<SimulationManager>();
                    l1Audiomanager = FindFirstObjectByType<L1Audiomanager>();
                    if (simulationManager.dropCount == 0)
                    {
                        l1Audiomanager.PlaySpecificAudio(3);

                    }
                    if (simulationManager.dropCount == 1)
                    {
                        l1Audiomanager.PlaySpecificAudio(5);
                    }
                }
            }

            Transform anchor = placementZone.GetSnapAnchorFor(this);
            Vector3 snapTarget = anchor != null ? anchor.position : placementZone.transform.position;
            snapTarget.y = Mathf.Max(snapTarget.y, originalY);
            snapTarget.z = originalZ;
            Quaternion snapRot = anchor != null ? anchor.rotation : placementZone.transform.rotation;

            if (snapRoutine != null) StopCoroutine(snapRoutine);
            snapRoutine = StartCoroutine(SnapToAnchorRoutine(snapTarget, snapRot, snapDuration));

        }
        else
        {
            if (smoothReturn)
            {
                if (smoothReturnRoutine != null) StopCoroutine(smoothReturnRoutine);
                smoothReturnRoutine = StartCoroutine(SmoothReturnCoroutine());
            }
            else
            {
                if (_rb != null) _rb.MovePosition(originalPosition); else transform.position = originalPosition;
                transform.rotation = originalRotation;
                if (outline != null) outline.SetActive(true);
            }
        }

        if (AdvancedOrbitCamera.instance != null) AdvancedOrbitCamera.instance.canOrbit = true;
    }

    private IEnumerator SmoothReturnCoroutine()
    {
        Vector3 start = transform.position;
        Vector3 end = originalPosition;
        start.y = Mathf.Max(start.y, originalY);
        end.y = originalY;
        end.z = originalZ;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = originalRotation;

        float t = 0f;
        float dur = Mathf.Max(0.001f, 1f / returnSpeed);
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            Vector3 pos = Vector3.Lerp(start, end, eased);
            if (_rb != null) _rb.MovePosition(pos); else transform.position = pos;
            transform.rotation = Quaternion.Slerp(startRot, endRot, eased);
            yield return null;
        }

        if (_rb != null) _rb.MovePosition(end); else transform.position = end;
        transform.rotation = endRot;
        smoothReturnRoutine = null;
        if (outline != null) outline.SetActive(true);
    }

    private IEnumerator SnapToAnchorRoutine(Vector3 targetPos, Quaternion targetRot, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        Vector3 startScale = transform.localScale;
        Vector3 bounceScale = startScale * snapBounceScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float e = Mathf.SmoothStep(0f, 1f, t);
            Vector3 pos = Vector3.Lerp(startPos, targetPos, e);
            if (_rb != null) _rb.MovePosition(pos); else transform.position = pos;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, e);

            if (t < 0.5f)
                transform.localScale = Vector3.Lerp(startScale, bounceScale, t * 2f);
            else
                transform.localScale = Vector3.Lerp(bounceScale, startScale, (t - 0.5f) * 2f);

            yield return null;
        }

        if (_rb != null) _rb.MovePosition(targetPos); else transform.position = targetPos;
        transform.rotation = targetRot;
        transform.localScale = startScale;

        IsPlaced = true;
        enabled = false;

        if (outline != null) outline.SetActive(false);
        if (placementZone != null) placementZone.ShowPlacementForDraggable(this, false);
        if (placementZone != null) placementZone.OnDraggablePlaced(this);

        snapRoutine = null;
    }

    // ---------- Simulation API ----------

    public void SetParachuteActive(bool smallOn, bool largeOn)
    {
        if (smallParachute != null) smallParachute.SetActive(smallOn);
        if (largeParachute != null) largeParachute.SetActive(largeOn);
    }

    public void PlayParachuteAnimation()
    {
        if (!string.IsNullOrEmpty(parachuteClipName))
        {
            if (smallParachute != null && smallParachute.activeSelf)
            {
                var anim = smallParachute.GetComponent<Animation>();
                if (anim != null && anim.GetClip(parachuteClipName) != null)
                {
                    anim.Play(parachuteClipName);
                }
            }

            if (largeParachute != null && largeParachute.activeSelf)
            {
                var anim = largeParachute.GetComponent<Animation>();
                if (anim != null && anim.GetClip(parachuteClipName) != null)
                {
                    anim.Play(parachuteClipName);
                }
            }
        }
    }

    private void StopParachuteAnimations()
    {
        if (smallParachute != null)
        {
            var anim = smallParachute.GetComponent<Animation>();
            if (anim != null) anim.Stop();
        }

        if (largeParachute != null)
        {
            var anim = largeParachute.GetComponent<Animation>();
            if (anim != null) anim.Stop();
        }
    }

    /// <summary>
    /// Slide the object to a start position (world) over 'slideDuration' seconds, then start drop.
    /// The final reported time (onComplete) is forced to the authoritative dropDuration.
    /// </summary>
    public IEnumerator SlideToAndDropRoutine(Vector3 slideTarget, float slideDuration, float dropDuration, Action<float> onComplete = null)
    {    // if object got destroyed, don't run coroutine
    if (IsDestroyed()) yield break;

        // begin simulation timing (we will use dropDuration as final authoritative value)
        _isSimulating = true;
        _simStartTime = Time.time;
        SimulationElapsed = 0f;
        OnSimulationStarted?.Invoke();

        dragging = false;
        pointerId = -1;

        if (placementZone != null) placementZone.ShowPlacementForDraggable(this, false);

        // SLIDE phase
        Vector3 start = transform.position;
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {  if (IsDestroyed()) yield break; 
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            Vector3 pos = Vector3.Lerp(start, slideTarget, t);
            if (_rb != null) _rb.MovePosition(pos); else transform.position = pos;

            // update sim timer if you want to display total (slide + drop) comment/uncomment:
            SimulationElapsed = (Time.time - _simStartTime);

            yield return null;
        }

        if (_rb != null) _rb.MovePosition(slideTarget); else transform.position = slideTarget;

        // start drop: play parachute animation (legacy Animation)
        PlayParachuteAnimation();

        // DROP phase - use dropElapsed to drive LERP and timer
        Vector3 dropStart = transform.position;
        float dropElapsed = 0f;

        while (dropElapsed < dropDuration)
        {  if (IsDestroyed()) yield break; 
            dropElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(dropElapsed / dropDuration);

            float y = Mathf.Lerp(dropStart.y, originalY, t);
            Vector3 pos = new Vector3(dropStart.x, y, dropStart.z);
            if (_rb != null) _rb.MovePosition(pos); else transform.position = pos;

            // update SimulationElapsed to reflect fall time (so display shows 0..dropDuration)
            SimulationElapsed = dropElapsed;

            yield return null;
        }

        // finalize at originalY
        Vector3 finalPos = new Vector3(dropStart.x, originalY, dropStart.z);
        if (_rb != null) _rb.MovePosition(finalPos); else transform.position = finalPos;

        // stop parachute animations when landed
        StopParachuteAnimations();

        // ENSURE the final displayed time equals the authoritative dropDuration (table value)
        SimulationElapsed = dropDuration;

        // invoke callback with the authoritative value so data table matches exactly
        onComplete?.Invoke(dropDuration);
        OnSimulationEnded?.Invoke(SimulationElapsed);

        // end simulation
        _isSimulating = false;
        _simStartTime = 0f;

        if (outline != null) outline.SetActive(false);

        yield break;
    }

    /// <summary>
    /// Convenience wrapper used by SimulationManager: starts coroutine on this object.
    /// </summary>
    public void StartSlideAndDrop(MonoBehaviour host, Vector3 slideTarget, float slideDuration, float dropDuration, Action<float> onComplete)
    {
        host.StartCoroutine(SlideToAndDropRoutine(slideTarget, slideDuration, dropDuration, onComplete));
    }

    // ---------- end simulation API ----------

    private bool CanMoveTo(Vector3 desiredPosition)
    {
        Vector3 checkCenter = desiredPosition + colliderCenterOffset;

        bool hadEnabled = objectCollider.enabled;
        objectCollider.enabled = false;

        Collider[] hits = Physics.OverlapBox(checkCenter, worldExtents, transform.rotation, obstacleLayers, QueryTriggerInteraction.Ignore);

        objectCollider.enabled = hadEnabled;

        if (hits == null || hits.Length == 0) return true;
        foreach (var c in hits)
        {
            if (c == null) continue;
            if (c == objectCollider) continue;
            if (c.transform.IsChildOf(transform)) continue;
            return false;
        }
        return true;
    }
    private bool IsDestroyed()
{
    return this == null || gameObject == null;
}
    public void NotifyEnteredPlacementZone()
    {
        InPlacementZone = true;
    }

    public void NotifyExitedPlacementZone()
    {
        InPlacementZone = false;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
#if UNITY_EDITOR
        return EventSystem.current.IsPointerOverGameObject();
#else
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return EventSystem.current.IsPointerOverGameObject();
#endif
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (objectCollider == null) objectCollider = GetComponent<Collider>();
        if (objectCollider == null) return;
        UpdateColliderInfo();
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + colliderCenterOffset;
        Gizmos.DrawWireCube(center, worldExtents * 2f);
#endif
    }
}
