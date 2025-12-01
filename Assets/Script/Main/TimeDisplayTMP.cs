using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class TimeDisplayTMP : MonoBehaviour
{
    public Draggable draggable;

    [Tooltip("If true, timer is hidden until simulation starts. AFTER landing it stays visible.")]
    public bool hideUntilSimStart = true;

    public string prefix = "";
    public string suffix = "s";
    public string numberFormat = "F2";

    private TextMeshPro _tmp;

    void Awake()
    {
        _tmp = GetComponent<TextMeshPro>();
        if (draggable == null)
            draggable = GetComponentInParent<Draggable>();
    }

    void Start()
    {
        if (hideUntilSimStart)
            _tmp.enabled = false;
    }

    void Update()
    {
        if (draggable == null) return;

        // While simulating → update every frame
        if (draggable.IsSimulating)
        {
            if (!_tmp.enabled) _tmp.enabled = true;

            float t = draggable.SimulationElapsed;
            _tmp.text = $"{prefix}{t.ToString(numberFormat)}{suffix}";
        }
        else
        {
            // Simulation ended → keep the text visible with final value
            float t = draggable.SimulationElapsed;
            _tmp.text = $"{prefix}{t.ToString(numberFormat)}{suffix}";

            // DO NOT disable text anymore
            if (!_tmp.enabled) _tmp.enabled = true;
        }
    }
}
