using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class Draggable : MonoBehaviour
{
    [Header("Identity")]
    public ShapeType shapeType = ShapeType.Cube;

    [Header("Camera & Drag")]
    public Camera cam; // optional, default to Camera.main
    [Tooltip("If set (>= -999), this value will be used as the fixed Z coordinate for the object. Otherwise initial Z will be used.")]
    public float fixedZ = -999f;

    [Header("Collision")]
    public LayerMask obstacleLayers;
    [Tooltip("Padding subtracted from collider extents for overlap checks.")]
    public float collisionPadding = 0.01f;

    [Header("Return / Snap")]
    public bool smoothReturn = true;
    public float returnSpeed = 8f;     // larger = faster return
    public float snapDuration = 0.25f;
    [Tooltip("Scale multiplier applied during snap bounce (1 = no bounce).")]
    public float snapBounceScale = 1.08f;

    [Header("Placement")]
    public PlacementZone placementZone; // set by spawner or inspector

    [Header("Visuals")]
    [Tooltip("Outline GameObject (child) to show when idle at original position.")]
    public GameObject outline;

    // runtime state
    public bool InPlacementZone { get; private set; } = false; // visual flag
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

    // dragging state
    private bool dragging = false;
    private int pointerId = -1;

    // move queue for physics-friendly movement
    private Vector3 _queuedPosition;
    private bool _hasQueuedPosition = false;

    private Coroutine smoothReturnRoutine = null;
    private Coroutine snapRoutine = null;

    private const float ORIGINAL_POS_EPS = 0.001f;

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

        if (placementZone != null)
            placementZone.ShowPlacement(false);
    }

    private void Start()
    {
        if (placementZone == null)
        {
            placementZone = FindObjectOfType<PlacementZone>();
            if (placementZone != null)
                placementZone.ShowPlacement(false);
        }
    }

    private void EnsureRigidbody()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }
        else
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }
    }

    public void SetPlacementZone(PlacementZone zone)
    {
        placementZone = zone;
        if (placementZone != null)
            placementZone.ShowPlacement(false);
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
        if (IsPlaced) return;

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            TryStartDrag(Input.mousePosition, 0);

        if (Input.GetMouseButtonUp(0) && dragging && pointerId == 0)
            EndDrag();

        if (dragging && pointerId == 0)
            ContinueDrag(Input.mousePosition);

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

                if (AdvancedOrbitCamera.instance != null)
                    AdvancedOrbitCamera.instance.canOrbit = false;

                // visuals: turn off outline, show only the placement visual for this shape
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
        if (_hasQueuedPosition && _rb != null && !IsPlaced)
        {
            _rb.MovePosition(_queuedPosition);
            _hasQueuedPosition = false;
        }
    }

    private void EndDrag()
    {
        dragging = false;
        pointerId = -1;

        // hide any placement visuals for this shape
        if (placementZone != null)
            placementZone.ShowPlacementForDraggable(this, false);

        if (placementZone == null)
            placementZone = FindObjectOfType<PlacementZone>();

        bool trackedInside = placementZone != null && placementZone.IsDraggableInside(this);
        bool geoInside = placementZone != null && placementZone.ContainsPoint(transform.position);

        bool actuallyInside = trackedInside && geoInside;

        if (actuallyInside && placementZone != null)
        {
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
                if (_rb != null)
                    _rb.MovePosition(originalPosition);
                else
                    transform.position = originalPosition;

                transform.rotation = originalRotation;
                if (outline != null) outline.SetActive(true);
            }
        }

        if (AdvancedOrbitCamera.instance != null)
            AdvancedOrbitCamera.instance.canOrbit = true;
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
            if (_rb != null)
                _rb.MovePosition(pos);
            else
                transform.position = pos;

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
            if (_rb != null)
                _rb.MovePosition(pos);
            else
                transform.position = pos;

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

    // predictive overlap test that ignores own colliders
    private bool CanMoveTo(Vector3 desiredPosition)
    {
        Vector3 checkCenter = desiredPosition + colliderCenterOffset;
        Collider[] hits = Physics.OverlapBox(checkCenter, worldExtents, transform.rotation, obstacleLayers, QueryTriggerInteraction.Ignore);

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
