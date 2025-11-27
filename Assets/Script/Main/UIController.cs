using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("UI Elements")]
    public Button[] lockedUI; // Surface Area buttons, Air toggle, Simulate button
    public Button[] shapeButtons; // shape selection buttons (kept enabled)
    public Toggle airToggle;

    private void Start()
    {
        SetLocked();
    }

    public void SetLocked()
    {
        foreach (var go in lockedUI)
            go.interactable =false;

            airToggle.interactable = false;
    }

    // Call when the shape has been correctly placed
    public void UnlockControls()
    {
         foreach (var go in lockedUI)
            go.interactable =true;


        airToggle.interactable = true;
    }
}
