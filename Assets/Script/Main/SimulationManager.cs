using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public enum ParachuteSize { None, Small, Large }

public class SimulationManager : MonoBehaviour
{
    public bool isLevelone;
    public bool dataFirst;
    public bool nextLevel;
    public Button dataBtn;

public GameObject datahigh,dataCloseHigh;
    [Header("Level 2 Audio Config")]
    [SerializeField] bool temp;
    [Header("General References")]
    public ShapeSpawner spawner;
    public Draggable currentDraggable;
    public UIController uiController;
    public DataTableManager dataTable;
    public Toggle airToggle;

    [Header("UI / Controls")]
    public Button simulateButton;
    public Button smallParaButton;
    public Button largeParaButton;
    public Toggle airResistanceButton;
    public Button[] lockedUI;

    [Header("Slide / Timing")]
    public Transform simulateSlideTarget;
    public float slideDuration = 0.6f;

    public int dropCount;

    [Header("Level 2 (simultaneous)")]
    public bool isLevelTwo = false;
    public Draggable[] level2Draggables = new Draggable[0];
    public Transform[] level2SlideTargets = new Transform[0];

    private ParachuteSize selectedParachute = ParachuteSize.None;
    private Dictionary<ShapeType, float[,]> durationTable;
    public bool _isSimulating = false;

    // NEW: Track whether level2 simulation already ran (prevent auto re-enable)
    private bool level2HasRun = false;
    // NEW: Track last placed states to detect when user changes placements again
    private bool[] lastPlacedStates;

    private void Awake()
    {
        BuildDurationTable();

        if (simulateButton != null)
            simulateButton.onClick.AddListener(OnSimulateClicked);

        if (smallParaButton != null) smallParaButton.onClick.AddListener(() => SelectParachute(ParachuteSize.Small));
        if (largeParaButton != null) largeParaButton.onClick.AddListener(() => SelectParachute(ParachuteSize.Large));

        if (simulateButton != null) simulateButton.interactable = false;
    }

    private void Start()
    {
        if (currentDraggable != null)
            SyncParachuteVisualsToCurrent();

        if (isLevelTwo)
            InitLevel2State();
    }

    private void InitLevel2State()
    {
        if (level2Draggables == null) level2Draggables = new Draggable[0];
        lastPlacedStates = new bool[level2Draggables.Length];
        for (int i = 0; i < lastPlacedStates.Length; i++)
            lastPlacedStates[i] = level2Draggables[i] != null && level2Draggables[i].IsPlaced;
        level2HasRun = false;
    }

    private void Update()
    {
        if (!isLevelTwo) return;

        // detect if placed states changed -> reset level2HasRun so simulate can become available next time
        bool anyChanged = false;
        for (int i = 0; i < level2Draggables.Length; i++)
        {
            var d = level2Draggables[i];
            bool now = (d != null && d.IsPlaced);
            if (now != lastPlacedStates[i])
            {
                anyChanged = true;
                lastPlacedStates[i] = now;
            }
        }
        if (anyChanged)
        {
            // user changed a placement (picked up or moved something) — allow simulate again after re-placement
            level2HasRun = false;
        }

        // Level 2: Simulate button is enabled only when:
        // - all draggables assigned and placed
        // - not currently simulating
        // - AND we have not already run level2 (level2HasRun == false)
        bool allAssigned = level2Draggables != null && level2Draggables.Length > 0 && level2SlideTargets != null && level2SlideTargets.Length == level2Draggables.Length;
        if (!allAssigned)
        {
            if (simulateButton != null) simulateButton.interactable = false;
            return;
        }

        bool allPlaced = true;
        foreach (var d in level2Draggables)
        {
            if (d == null || !d.IsPlaced)
            {
                allPlaced = false;
                break;
            }
        }

       
        
        

        if (simulateButton != null)
        {
             //L2Audiomanager.Instance.PlaySpecificAudio(0);
            simulateButton.interactable = (allPlaced && !_isSimulating && !level2HasRun);
            smallParaButton.interactable = (allPlaced && !_isSimulating && !level2HasRun);
            largeParaButton.interactable = (allPlaced && !_isSimulating && !level2HasRun);
           
            if(airResistanceButton != null)
            {
                  airResistanceButton.interactable = (allPlaced && !_isSimulating && !level2HasRun);
            }
        }
    }

    private void BuildDurationTable()
    {
        durationTable = new Dictionary<ShapeType, float[,]>();

        durationTable[ShapeType.Cube] = new float[2, 2] {
            { 0.64f, 0.64f }, // air off small/large
            { 2.03f, 6.15f }  // air on small/large
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

    private void SelectParachute(ParachuteSize size)
    {
        selectedParachute = size;
        if (!isLevelTwo && currentDraggable != null)
        {
            if (size == ParachuteSize.Small) currentDraggable.SetParachuteActive(true, false);
            else if (size == ParachuteSize.Large) currentDraggable.SetParachuteActive(false, true);
            else currentDraggable.SetParachuteActive(false, false);
        }
    }

    private float LookupDurationForDraggable(Draggable d)
    {
        if (d == null) return 0.64f;
        var shape = d.shapeType;
        bool airOn = (airToggle != null && airToggle.isOn);
        int airIndex = airOn ? 1 : 0;
        int paraIndex = (selectedParachute == ParachuteSize.Small) ? 0 : 1;
        if (!durationTable.ContainsKey(shape)) return 0.64f;
        return durationTable[shape][airIndex, paraIndex];
    }

    private void OnSimulateClicked()
    {
        if (_isSimulating) return;

        if (isLevelTwo)
        {
            StartLevel2Simulation();
            return;
        }

        // Level1 unchanged
        if (currentDraggable == null)
        {
            Debug.LogWarning("[SimulationManager] No draggable instance assigned.");
            return;
        }

        if (selectedParachute == ParachuteSize.None)
        {
            Debug.Log("No parachute selected. Defaulting to SMALL parachute.");
            selectedParachute = ParachuteSize.Small;
            currentDraggable.SetParachuteActive(true, false);
        }

        if (uiController != null) uiController.SetLocked();

        float dropDuration = LookupDurationForDraggable(currentDraggable);

        Vector3 slideTarget;
        if (simulateSlideTarget != null)
        {
            slideTarget = simulateSlideTarget.position;
            slideTarget.y = currentDraggable.transform.position.y;
        }
        else slideTarget = currentDraggable.transform.position;

        _isSimulating = true;
        if (simulateButton != null) simulateButton.interactable = false;

        currentDraggable.StartSlideAndDrop(this, slideTarget, slideDuration, dropDuration, (realTime) =>
        {



            Debug.Log($"[SimulationManager] Simulation completed. reported drop time (table): {dropDuration:F2}s (measured: {realTime:F2}s)");
            // if (uiController != null) uiController.UnlockControls();

            if (dataTable != null)
            {
                string shapeText = currentDraggable.shapeType.ToString();
                string airText = (airToggle != null && airToggle.isOn) ? "On" : "Off";
                string paraText = (selectedParachute == ParachuteSize.Small) ? "Small" : "Large";
                dataTable.AddRow(shapeText, airText, paraText, dropDuration);

            }
            UIController.Instance.ResetButtonControls(true);
            if (isLevelone)
            {
                if (dropCount <= 4)
                {

                    dropCount++;
                }
                if (dropCount == 1)
                {
                    L1Audiomanager.Instance.PlaySpecificAudio(4);
                }
                if (dropCount == 2)
                {
                    L1Audiomanager.Instance.PlaySpecificAudio(6);
                }
                if (dropCount == 3)
                {
                    L1Audiomanager.Instance.PlaySpecificAudio(7);
                }
                if (dropCount == 4)
                {
                    L1Audiomanager.Instance.PlaySpecificAudio(8);
                    dataBtn.interactable = true;
                    datahigh.SetActive(true);
                }
            }
            _isSimulating = false;
            // DO NOT set simulateButton.interactable = true here; keep it off until user action
        });
    }

    public void DataShow()
    {
        if (dataFirst)
        {
            L1Audiomanager.Instance.PlaySpecificAudio(9);
           DataCloseHighh();
            dataFirst = false;
        }


    }
    public void DataCloseHighh()
    {
        Invoke(nameof(DataCross),2f);
    }
    public void DataCross()
    {
        dataCloseHigh.SetActive(true);
    }

    public void DataClosed()
    {

        if (nextLevel)
        {
            L1Audiomanager.Instance.PlaySpecificAudio(10);
            uiController.Starthighlighted();
            nextLevel = false;
        }


    }
    public void CubeAudioOneTime()
    {
        if (dropCount < 1)
        {
            L1Audiomanager.Instance.PlaySpecificAudio(2);

        }
    }

    // ---------------- Level 2 ----------------
    private void StartLevel2Simulation()
    {
        if (level2Draggables == null || level2Draggables.Length == 0)
        {
            Debug.LogWarning("[SimulationManager] Level2 enabled but no draggables assigned.");
            return;
        }

        if (level2SlideTargets == null || level2SlideTargets.Length != level2Draggables.Length)
        {
            Debug.LogWarning("[SimulationManager] Level2 slide targets must match draggable count.");
            return;
        }

        if (selectedParachute == ParachuteSize.None)
        {
            Debug.Log("No parachute selected for Level2. Defaulting to SMALL for all.");
            selectedParachute = ParachuteSize.Small;
        }

        foreach (var d in level2Draggables)
        {
            if (d == null) continue;
            if (selectedParachute == ParachuteSize.Small) d.SetParachuteActive(true, false);
            else if (selectedParachute == ParachuteSize.Large) d.SetParachuteActive(false, true);
            else d.SetParachuteActive(false, false);
        }

        if (uiController != null) uiController.SetLocked();
        _isSimulating = true;
        level2HasRun = true;                 // mark we've run Level2 (prevents auto re-enable)
        if (simulateButton != null) simulateButton.interactable = false;

        int remaining = level2Draggables.Length;
        var drags = level2Draggables;
        var targets = level2SlideTargets;

        for (int i = 0; i < drags.Length; i++)
        {
            var d = drags[i];
            if (d == null)
            {
                remaining--;
                continue;
            }

            float dropDuration = LookupDurationForDraggable(d);
            Vector3 slideTarget = (targets != null && i < targets.Length && targets[i] != null) ? targets[i].position : d.transform.position;
            slideTarget.y = d.transform.position.y;

            d.StartSlideAndDrop(this, slideTarget, slideDuration, dropDuration, (realTime) =>
            {
                if (dataTable != null)
                {
                    string shapeText = d.shapeType.ToString();
                    string airText = (airToggle != null && airToggle.isOn) ? "On" : "Off";
                    string paraText = (selectedParachute == ParachuteSize.Small) ? "Small" : "Large";
                    dataTable.AddRow(shapeText, airText, paraText, dropDuration);

                }

                remaining--;
                if (remaining <= 0)
                {
                    _isSimulating = false;
                    //  if (uiController != null) uiController.UnlockControls();
                    // Keep simulateButton disabled — Update() will only re-enable if user changes placements (level2HasRun will be reset)
                    if (simulateButton != null) simulateButton.interactable = false;
                }

                L2Audiomanager.Instance.PlaySpecificAudio(0);
            });
        }

        if (remaining <= 0)
        {
            _isSimulating = false;
            //  if (uiController != null) uiController.UnlockControls();
            if (simulateButton != null) simulateButton.interactable = false;
        }
    }

    private void SyncParachuteVisualsToCurrent()
    {
        if (currentDraggable == null) return;
        if (selectedParachute == ParachuteSize.Small) currentDraggable.SetParachuteActive(true, false);
        else if (selectedParachute == ParachuteSize.Large) currentDraggable.SetParachuteActive(false, true);
        else currentDraggable.SetParachuteActive(false, false);
    }

    public void ResetParachuteSelectionForNewShape(Draggable d)
    {
        selectedParachute = ParachuteSize.None;
        currentDraggable = d;
        if (currentDraggable != null) currentDraggable.SetParachuteActive(false, false);
    }
}
