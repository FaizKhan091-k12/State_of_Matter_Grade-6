using System;
using UnityEngine;

public class SimulationResetter : MonoBehaviour
{
    [Header("Level Templates")]
    [Tooltip("Assign one prefab per level. Index 0 = Level 1, Index 1 = Level 2, ...")]
    [SerializeField] private GameObject[] levelTemplates;

    [Header("Runtime")]
    [Tooltip("All level instances will be parented here")]
    [SerializeField] private Transform parentContainer;

    // Index of the currently active level prefab in levelTemplates (-1 = none)
    private int currentLevelIndex = -1;

    // Store the active instance for each level index (null when not instantiated)
    private GameObject[] currentInstances;

    private void Awake()
    {
        if (levelTemplates == null)
            levelTemplates = new GameObject[0];

        // create storage for instances equal to number of templates
        currentInstances = new GameObject[levelTemplates.Length];
    }

    private void Start()
    {
        // Optionally auto-load the first level if available
        if (levelTemplates.Length > 0)
            LoadLevel(0);
    }

    /// <summary>
    /// Loads (or reloads) the specified level index.
    /// This will destroy the previously active level instance (if any)
    /// and instantiate the requested level prefab as a child of parentContainer.
    /// </summary>
    /// <param name="index">Index into levelTemplates array</param>
    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levelTemplates.Length)
        {
            Debug.LogWarning($"LoadLevel: index {index} is out of range (0..{levelTemplates.Length - 1})");
            return;
        }

        // If the requested level is already active, just reset it
        if (currentLevelIndex == index)
        {
            ResetCurrentLevel();
            return;
        }

        // Destroy previously active level instance (if any)
        if (currentLevelIndex >= 0 && currentLevelIndex < currentInstances.Length && currentInstances[currentLevelIndex] != null)
        {
            Destroy(currentInstances[currentLevelIndex]);
            currentInstances[currentLevelIndex] = null;
        }

        // Instantiate new level if not already cached
        if (currentInstances[index] == null)
        {
            var prefab = levelTemplates[index];
            if (prefab == null)
            {
                Debug.LogError($"LoadLevel: levelTemplates[{index}] is null.");
                return;
            }

            var instance = Instantiate(prefab, parentContainer);
            instance.SetActive(true);

            // Reset local transform so it appears correctly under parent
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            currentInstances[index] = instance;
        }
        else
        {
            // If instance already exists (it was created earlier), just re-enable it
            currentInstances[index].SetActive(true);
        }

        currentLevelIndex = index;
    }

    /// <summary>
    /// Destroys and re-instantiates the currently active level (if any).
    /// Useful when you want to fully reset the level state.
    /// </summary>
    public void ResetCurrentLevel()
    {
        if (currentLevelIndex < 0 || currentLevelIndex >= levelTemplates.Length)
        {
            Debug.Log("ResetCurrentLevel: no level loaded.");
            return;
        }

        // destroy existing instance
        if (currentInstances[currentLevelIndex] != null)
        {
            Destroy(currentInstances[currentLevelIndex]);
            currentInstances[currentLevelIndex] = null;
        }

        // instantiate fresh instance
        var prefab = levelTemplates[currentLevelIndex];
        if (prefab == null)
        {
            Debug.LogError($"ResetCurrentLevel: prefab at index {currentLevelIndex} is null.");
            return;
        }

        var instance = Instantiate(prefab, parentContainer);
        instance.SetActive(true);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        currentInstances[currentLevelIndex] = instance;
    }

    /// <summary>
    /// Destroys *all* instantiated level instances.
    /// Does not change currentLevelIndex (but the active instance will be removed).
    /// </summary>
    public void ResetAllLevels()
    {
        for (int i = 0; i < currentInstances.Length; i++)
        {
            if (currentInstances[i] != null)
            {
                Destroy(currentInstances[i]);
                currentInstances[i] = null;
            }
        }

        // If you want to reset the active index as well, uncomment:
        // currentLevelIndex = -1;
    }

    /// <summary>
    /// Convenience method: load the next level in the array (wrap-around).
    /// </summary>
    public void NextLevel()
    {
        if (levelTemplates.Length == 0) return;
        int next = (currentLevelIndex + 1) % levelTemplates.Length;
        LoadLevel(next);
    }

    /// <summary>
    /// Convenience method: load the previous level in the array (wrap-around).
    /// </summary>
    public void PrevLevel()
    {
        if (levelTemplates.Length == 0) return;
        int prev = (currentLevelIndex - 1 + levelTemplates.Length) % levelTemplates.Length;
        LoadLevel(prev);
    }

    /// <summary>
    /// Return currently active level index (-1 if none)
    /// </summary>
    public int GetCurrentLevelIndex() => currentLevelIndex;

    /// <summary>
    /// Return the active GameObject instance for a level (may be null)
    /// </summary>
    public GameObject GetInstanceForLevel(int index)
    {
        if (index < 0 || index >= currentInstances.Length) return null;
        return currentInstances[index];
    }
}
