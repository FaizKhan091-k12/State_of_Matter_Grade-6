using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlacementZone : MonoBehaviour
{
    [Serializable]
    public struct AnchorEntry
    {
        public ShapeType shape;
        public Transform anchor;

        [Tooltip("Optional visual for this anchor (e.g. ring/ghost). Will be toggled on while dragging this shape).")]
        public GameObject placementVisual;
    }

    [Header("Anchors")]
    [Tooltip("List of anchors for different shapes. If a shape has no anchor here, the zone transform will be used.")]
    public List<AnchorEntry> anchors = new List<AnchorEntry>();

    [Header("Optional")]
    public UIController uiController;

    // track draggables currently inside
    private HashSet<Draggable> _inside = new HashSet<Draggable>();

    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        if (_collider != null) _collider.isTrigger = true;
        HideAllPlacementVisuals();
    }

    private void OnTriggerEnter(Collider other)
    {
        var d = other.GetComponentInParent<Draggable>();
        if (d != null)
        {
            _inside.Add(d);
            d.NotifyEnteredPlacementZone();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var d = other.GetComponentInParent<Draggable>();
        if (d != null && !_inside.Contains(d))
        {
            _inside.Add(d);
            d.NotifyEnteredPlacementZone();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var d = other.GetComponentInParent<Draggable>();
        if (d != null)
        {
            if (_inside.Contains(d)) _inside.Remove(d);
            d.NotifyExitedPlacementZone();
        }
    }

    public void OnDraggablePlaced(Draggable d)
    {
        if (uiController != null) uiController.UnlockControls();
        if (d != null && _inside.Contains(d)) _inside.Remove(d);

        // ensure visuals off
        HideAllPlacementVisuals();
    }

    public Transform GetSnapAnchorFor(Draggable d)
    {
        if (d == null) return this.transform;
        for (int i = 0; i < anchors.Count; i++)
        {
            if (anchors[i].shape == d.shapeType && anchors[i].anchor != null)
                return anchors[i].anchor;
        }
        return this.transform;
    }

    /// <summary>
    /// Show or hide the generic placement visual(s) (keeps single-anchor visuals untouched if called).
    /// </summary>
    public void ShowPlacement(bool show)
    {
        // if you used the old placementVisual (single), keep backward compatibility:
        // But prefer ShowPlacementForShape for per-anchor visuals.
        // We'll hide all anchor visuals when show==false.
        if (!show)
            HideAllPlacementVisuals();
    }

    /// <summary>
    /// Show only the placement visual for the given shape; hides all others.
    /// Pass null to hide all.
    /// </summary>
    public void ShowPlacementForShape(ShapeType? shape)
    {
        for (int i = 0; i < anchors.Count; i++)
        {
            var v = anchors[i].placementVisual;
            if (v == null) continue;

            if (shape.HasValue && anchors[i].shape == shape.Value)
                v.SetActive(true);
            else
                v.SetActive(false);
        }
    }

    /// <summary>
    /// Convenience: show visuals for a draggable's shape (or hide all if show==false).
    /// </summary>
    public void ShowPlacementForDraggable(Draggable d, bool show)
    {
        if (!show)
        {
            HideAllPlacementVisuals();
            return;
        }

        if (d == null)
        {
            HideAllPlacementVisuals();
            return;
        }

        ShowPlacementForShape(d.shapeType);
    }

    private void HideAllPlacementVisuals()
    {
        for (int i = 0; i < anchors.Count; i++)
        {
            var v = anchors[i].placementVisual;
            if (v != null && v.activeSelf)
                v.SetActive(false);
        }
    }

    /// <summary>
    /// Robust containment test using collider. ClosestPoint==point if inside.
    /// </summary>
    public bool ContainsPoint(Vector3 worldPoint, float tolerance = 0.0005f)
    {
        if (_collider == null) _collider = GetComponent<Collider>();
        if (_collider == null) return false;
        Vector3 closest = _collider.ClosestPoint(worldPoint);
        return Vector3.Distance(closest, worldPoint) <= tolerance;
    }

    /// <summary>
    /// Whether placement zone currently tracks the draggable as inside (visual flag).
    /// </summary>
    public bool IsDraggableInside(Draggable d)
    {
        return d != null && _inside.Contains(d);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (var a in anchors)
        {
            if (a.anchor != null)
            {
                Gizmos.DrawWireSphere(a.anchor.position, 0.05f);
                UnityEditor.Handles.Label(a.anchor.position + Vector3.up * 0.05f, a.shape.ToString());
            }
        }

        if (_collider == null) _collider = GetComponent<Collider>();
        if (_collider != null)
        {
            Gizmos.color = new Color(0f, 0.8f, 0.8f, 0.12f);
            Gizmos.DrawCube(_collider.bounds.center, _collider.bounds.size);
        }
    }
#endif
}
