using UnityEngine;

public class ShapeSpawner : MonoBehaviour
{
    [Tooltip("Index order should match how you call SpawnShape(index) from UI.")]
    public GameObject[] shapePrefabs;
    public Transform spawnPoint;
    public PlacementZone placementZone; // assign in inspector

    private GameObject current;

    public void SpawnShape(int index)
    {
        if (index < 0 || index >= shapePrefabs.Length)
        {
            Debug.LogWarning("SpawnShape index out of range");
            return;
        }

        if (current != null) Destroy(current);

        // Instantiate using prefab's local rotation (preserve prefab rotation)
        GameObject prefab = shapePrefabs[index];
        current = Instantiate(prefab, spawnPoint.position, prefab.transform.rotation);

        Draggable d = current.GetComponent<Draggable>();
        if (d != null)
        {
            d.SetPlacementZone(placementZone);
            // Optionally set shapeType here if needed:
            // d.shapeType = (ShapeType)index;

            // Tell SimulationManager a new draggable was spawned so it can reset parachute selection
            var sim = FindObjectOfType<SimulationManager>();
            if (sim != null)
            {
                sim.ResetParachuteSelectionForNewShape(d);
            }
        }
    }

    public void ClearCurrent()
    {
        if (current != null)
        {
            Destroy(current);
            current = null;
        }
    }
}
