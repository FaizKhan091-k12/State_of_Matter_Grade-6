using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class UIController : MonoBehaviour
{
    public bool isLevelone;
    public SimulationManager simulationManager;
    public static UIController Instance;
    public GameObject streamLineHigh,sphereHigh;
    [Header("UI Elements")]
    public Button[] lockedUI; // Surface Area buttons, Air toggle, Simulate button
    public Button[] shapeButtons; // shape selection buttons (kept enabled)
    public Toggle airToggle;
    public Button resetButton;
    public GameObject dataTable;
    public Ease ease;

    public GameObject smallBalloongHighLighted;
    public GameObject airResHigh;
    public GameObject LargeBalloongHighLighted;
    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        SetLocked();
    }

    public void SetLocked()
    {
        foreach (var go in lockedUI)
            go.interactable = false;

        airToggle.interactable = false;

    }

    public void UnlockControls()
    {
        if (isLevelone)
        {
            if (simulationManager.dropCount == 0)
            {
                Debug.Log("First Drop");
                lockedUI[2].interactable = true;
                airToggle.interactable = false;
            }
            if (simulationManager.dropCount == 1)
            {
                foreach (var go in lockedUI)
                    go.interactable = true;
                airToggle.interactable = false;
 LargeBalloongHighLighted.SetActive(true);
            }
            if (simulationManager.dropCount == 2)
            {
                lockedUI[0].interactable = true;
                lockedUI[2].interactable = true;
                smallBalloongHighLighted.SetActive(true);
                airResHigh.SetActive(true);
                airToggle.interactable = true;
            }
            if (simulationManager.dropCount == 3)
            {
                foreach (var go in lockedUI)
                    go.interactable = true;

                airToggle.interactable = true;
                LargeBalloongHighLighted.SetActive(true);
            }
            if (simulationManager.dropCount == 4)
            {
                foreach (var go in lockedUI)
                    go.interactable = true;

                airToggle.interactable = true;
              
                //   LargeBalloongHighLighted.SetActive(true);
            }

            if (simulationManager.dropCount == 5)
            {
                foreach (var go in lockedUI)
                    go.interactable = true;
  
                airToggle.interactable = true;

       
                //   LargeBalloongHighLighted.SetActive(true);
            }
        }
        else
        {
            // foreach (var go in lockedUI)
            //     go.interactable = true;

            // airToggle.interactable = true;
        }

    }

    public void OpenDataPanel()
    {
        dataTable.transform.localScale = Vector3.zero;
        dataTable.transform.DOScale(Vector3.one, .2f).SetEase(ease);
    }

    public void CloseDataPanel()
    {
        dataTable.transform.localScale = Vector3.one;
        dataTable.transform.DOScale(Vector3.zero, .2f).SetEase(ease);
    }

    public void ResetButtonControls(bool isActive)
    {
        resetButton.interactable = isActive;
    }
    public void Starthighlighted()
    {
                   streamLineHigh.SetActive(true);
                sphereHigh.SetActive(true);
    }
}
