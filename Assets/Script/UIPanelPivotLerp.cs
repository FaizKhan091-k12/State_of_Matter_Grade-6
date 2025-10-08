using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIPanelPivotLerp : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How fast the pivot moves between open and closed states.")]
    public float lerpSpeed = 5f;

    [Tooltip("Pivot position when panel is closed (e.g. left/top).")]
    public Vector2 closedPivot = new Vector2(0f, 1f);

    [Tooltip("Pivot position when panel is open (e.g. right/bottom).")]
    public Vector2 openPivot = new Vector2(1f, 0f);

    [Header("State")]
    [Tooltip("True = Open; False = Closed.")]
    public bool isOpen = false;

    private RectTransform rectTransform;
    private Vector2 targetPivot;
    public GameObject arrowImage;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetPivot = isOpen ? openPivot : closedPivot;
        rectTransform.pivot = targetPivot;
    }

    void Update()
    {
        // Smoothly Lerp pivot to target position
        rectTransform.pivot = Vector2.Lerp(rectTransform.pivot, targetPivot, Time.deltaTime * lerpSpeed);

        if (isOpen)
        {
            arrowImage.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            arrowImage.transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    /// <summary>
    /// Toggle between open and closed.
    /// </summary>
    public void TogglePanel()
    {
        isOpen = !isOpen;
        targetPivot = isOpen ? openPivot : closedPivot;
    }

    /// <summary>
    /// Open the panel smoothly.
    /// </summary>
    public void OpenPanel()
    {
        isOpen = true;
        targetPivot = openPivot;
    }

    /// <summary>
    /// Close the panel smoothly.
    /// </summary>
    public void ClosePanel()
    {
        isOpen = false;
        targetPivot = closedPivot;
    }
}
