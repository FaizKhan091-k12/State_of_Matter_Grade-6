// ShapeSpawner.cs
using UnityEngine;

public class ShapeSpawner : MonoBehaviour
{
    [Tooltip("Index order should match how you call SpawnShape(index) from UI.")]
    public GameObject[] shapePrefabs;

    [Tooltip("World position where new shapes will appear.")]
    public Transform spawnPoint;

    [Tooltip("PlacementZone in the scene to assign to spawned draggables.")]
    public PlacementZone placementZone; // assign in inspector

    [Tooltip("If true, the spawned object will be parented to spawnPoint. If false, it will be root-level.")]
    public bool parentToSpawnPoint = false;

    [Tooltip("If true, the spawned Draggable.shapeType will be set automatically from the prefab array index.")]
    public bool overrideShapeTypeFromIndex = false;

    private GameObject current;

    /// <summary>
    /// Spawn a shape by index (call from UI). Preserves the prefab's authored rotation.
    /// </summary>
    public void SpawnShape(int index)
    {
        if (index < 0 || index >= shapePrefabs.Length)
        {
            Debug.LogWarning($"SpawnShape: index {index} out of range (0..{shapePrefabs.Length - 1}).");
            return;
        }

        // destroy previous instance if present
        if (current != null)
        {
            Destroy(current);
            current = null;
        }

        GameObject prefab = shapePrefabs[index];
        if (prefab == null)
        {
            Debug.LogWarning($"SpawnShape: prefab at index {index} is null.");
            return;
        }

        // Instantiate using the prefab's authored rotation to preserve orientation
        if (parentToSpawnPoint && spawnPoint != null)
        {
            // Parent to spawnPoint but keep the prefab's local rotation
            current = Instantiate(prefab, spawnPoint.position, prefab.transform.rotation, spawnPoint);
            // Ensure local rotation equals prefab's localRotation (in case spawnPoint scale/rotation affects it)
            current.transform.localRotation = prefab.transform.localRotation;
        }
        else
        {
            // No parent: spawn at world position with prefab rotation
            Quaternion spawnRot = prefab.transform.rotation;
            Vector3 spawnPos = (spawnPoint != null) ? spawnPoint.position : Vector3.zero;
            current = Instantiate(prefab, spawnPos, spawnRot);
        }

        // Assign placement zone and optionally override shape type
        Draggable d = current.GetComponent<Draggable>();
        if (d != null)
        {
            // Use public setter to reliably assign the zone
            d.SetPlacementZone(placementZone);

            if (overrideShapeTypeFromIndex)
            {
                // Guard: ensure enum cast is valid
                var values = System.Enum.GetValues(typeof(ShapeType));
                if (index < values.Length)
                    d.shapeType = (ShapeType)index;
            }
        }
    }

    /// <summary>
    /// Destroys any currently spawned object.
    /// </summary>
    public void ClearCurrent()
    {
        if (current != null)
        {
            Destroy(current);
            current = null;
        }
    }
}
