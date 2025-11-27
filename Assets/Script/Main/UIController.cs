using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting;
public class UIController : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] lockedUI; // Surface Area buttons, Air toggle, Simulate button
    public Button[] shapeButtons; // shape selection buttons (kept enabled)
    public Toggle airToggle;
    public GameObject dataTable;
    public Ease ease;

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
    foreach (var go in lockedUI)
        go.interactable = true;

    airToggle.interactable = true;
}

public void OpenDataPanel()
    {
        dataTable.transform.localScale =Vector3.zero;
        dataTable.transform.DOScale(Vector3.one,.2f).SetEase(ease);
    }

    public void CloseDataPanel()
    {
        dataTable.transform.localScale =Vector3.one;
        dataTable.transform.DOScale(Vector3.zero,.2f).SetEase(ease);
    }
}
