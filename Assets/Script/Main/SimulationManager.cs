using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ParachuteSize { None, Small, Large }

public class SimulationManager : MonoBehaviour
{
    [Header("References")]
    public ShapeSpawner spawner;
    public Draggable currentDraggable;
    public UIController uiController;
    public Transform simulateSlideTarget;
    public float slideDuration = 0.6f;
public GameObject smallparachute;
    [Header("Controls")]
    public Toggle airToggle;
    public Button simulateButton;
    public Button smallParaButton;
    public Button largeParaButton;

    private ParachuteSize selectedParachute = ParachuteSize.None;

    private Dictionary<ShapeType, float[,]> durationTable;

    private void Awake()
    {
        BuildDurationTable();
        if (simulateButton != null)
            simulateButton.onClick.AddListener(OnSimulateClicked);
        if (smallParaButton != null) smallParaButton.onClick.AddListener(() => SelectParachute(ParachuteSize.Small));
        if (largeParaButton != null) largeParaButton.onClick.AddListener(() => SelectParachute(ParachuteSize.Large));
        if (airToggle != null) airToggle.onValueChanged.AddListener((v) => { /* no-op here */ });
    }

    private void BuildDurationTable()
    {
        durationTable = new Dictionary<ShapeType, float[,]>();

        durationTable[ShapeType.Cube] = new float[2, 2] {
            { 0.64f, 0.64f }, // air off
            { 2.03f, 6.15f }  // air on
        };

        durationTable[ShapeType.Sphere] = new float[2, 2] {
            { 0.64f, 0.64f },
            { 2.02f, 6.12f }
        };

        durationTable[ShapeType.Streamlined] = new float[2, 2] {
            { 0.64f, 0.64f },
            { 2.00f, 6.08f }
        };
    }

    /// <summary>
    /// Called by spawner when a new Draggable is created.
    /// Resets parachute selection state and turns off both parachute visuals on the new draggable.
    /// </summary>
    public void ResetParachuteSelectionForNewShape(Draggable d)
    {
        selectedParachute = ParachuteSize.None;
        currentDraggable = d;

        if (currentDraggable != null)
        {
            currentDraggable.SetParachuteActive(false, false);
        }

        // (Optional) update UI to reflect no selection - you can add highlight toggles here.
        // e.g., clear button highlights or set colors back to default.
    }

    public void SetCurrentDraggable(Draggable d)
    {
        currentDraggable = d;
        // keep parachute selection in sync visually
        if (currentDraggable != null)
        {
            if (selectedParachute == ParachuteSize.Small) currentDraggable.SetParachuteActive(true, false);
            else if (selectedParachute == ParachuteSize.Large) currentDraggable.SetParachuteActive(false, true);
            else currentDraggable.SetParachuteActive(false, false);
        }
    }

    private void SelectParachute(ParachuteSize size)
    {
        selectedParachute = size;

        if (currentDraggable != null)
        {
            if (size == ParachuteSize.Small) currentDraggable.SetParachuteActive(true, false);
            else if (size == ParachuteSize.Large) currentDraggable.SetParachuteActive(false, true);
            else currentDraggable.SetParachuteActive(false, false);
        }

        // (Optional) update button visuals to show selection
    }

    private float LookupDurationForCurrent()
    {
        if (currentDraggable == null)
        {
            Debug.LogWarning("[SimulationManager] No draggable selected.");
            return 0.64f;
        }

        var shape = currentDraggable.shapeType;
        bool airOn = (airToggle != null) && airToggle.isOn;
        int airIndex = airOn ? 1 : 0;
        int paraIndex = (selectedParachute == ParachuteSize.Small) ? 0 : 1;

        if (!durationTable.ContainsKey(shape))
        {
            Debug.LogWarning($"No duration data for shape {shape}. Using fallback 0.64s.");
            return 0.64f;
        }

        var table = durationTable[shape];
        return table[airIndex, paraIndex];
    }

    private void OnSimulateClicked()
    {
        if (currentDraggable == null)
        {
            Debug.LogWarning("[SimulationManager] No draggable instance assigned.");
            return;
        }

        // If user didn't pick a parachute, default to Small right now
        if (selectedParachute == ParachuteSize.None)
        {
            Debug.Log("No parachute selected. Defaulting to SMALL parachute.");
            smallparachute.SetActive(true);
            selectedParachute = ParachuteSize.Small;
            // reflect visually on the draggable
            currentDraggable.SetParachuteActive(true, false);
            // (Optional) update UI smallParaButton highlight
        }

        // disable locked UI immediately
        if (uiController != null) uiController.SetLocked();

        // find drop duration
        float dropDuration = LookupDurationForCurrent();

       Vector3 slideTarget;
if (simulateSlideTarget != null)
{
    slideTarget = simulateSlideTarget.position;
    // keep the Y of the draggable so it doesn't jump up/down
    slideTarget.y = currentDraggable.transform.position.y;
}
else
{
    slideTarget = currentDraggable.transform.position;
}
       currentDraggable.StartSlideAndDrop(this, slideTarget, slideDuration, dropDuration, (realTime) =>
{
    Debug.Log($"[SimulationManager] Simulation completed. real drop time: {realTime:F2}s (table: {dropDuration:F2}s)");
   // if (uiController != null) uiController.UnlockControls();

    // Add an entry to the data table:
    var dt = FindObjectOfType<DataTableManager>();
    if (dt != null)
    {
        string shapeText = currentDraggable.shapeType.ToString();
        string airText = (airToggle != null && airToggle.isOn) ? "On" : "Off";
        string paraText = (selectedParachute == ParachuteSize.Small) ? "Small" : "Large";
        dt.AddRow(shapeText, airText, paraText, realTime);
    }
});
    }
}
