using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TMP_Text))]
public class TMPValueLerp : MonoBehaviour
{
     [Header("Target Text (if left empty, will use TMP on this GameObject)")]
    public TMP_Text targetText;

    [Header("Inspector helper (for Button calls)")]
    public float inspectorMin = 0f;
    public float inspectorMax = 100f;
    [Tooltip("Seconds for the full transition")]
    public float inspectorDuration = 1f;

    [Header("Display")]
    [Tooltip("If true, value will be rounded to integer when shown.")]
    public bool displayAsInteger = false;

    [Tooltip("Custom format string for floats (e.g. F2) or leave empty for default.")]
    public string floatFormat = "F2";

    [Tooltip("Add an SI unit or symbol that appears after the value (e.g. °C, m, kg, s).")]
    public string siUnit = "°C";

    [Tooltip("Add a space before the unit (for readability).")]
    public bool addSpaceBeforeUnit = true;

    [Tooltip("If true uses unscaled time (ignores Time.timeScale).")]
    public bool useUnscaledTime = false;

    [Header("Events")]
    public UnityEvent onComplete;

    // Internal state
    private Coroutine runningCoroutine;

    void Reset()
    {
        targetText = GetComponent<TMP_Text>();
    }

    void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// Start lerping value from min -> max over duration seconds.
    /// </summary>
    public void StartLerp(float min, float max, float duration)
    {
        if (targetText == null)
        {
            Debug.LogWarning("TMPValueLerpWithUnit: targetText not assigned.");
            return;
        }

        if (runningCoroutine != null)
            StopCoroutine(runningCoroutine);

        runningCoroutine = StartCoroutine(LerpRoutine(min, max, Mathf.Max(0.0001f, duration)));
    }

    /// <summary>
    /// Convenience method for calling via Button (uses inspectorMin/Max/Duration).
    /// </summary>
    public void StartLerpWithInspectorValues()
    {
        StartLerp(inspectorMin, inspectorMax, inspectorDuration);
    }

    /// <summary>
    /// Stop the current animation and set to final value.
    /// </summary>
    public void StopAndSetToMax()
    {
        if (runningCoroutine != null)
            StopCoroutine(runningCoroutine);

        runningCoroutine = null;
        SetTextValue(inspectorMax);
    }

    IEnumerator LerpRoutine(float min, float max, float duration)
    {
        float t = 0f;
        float start = min;
        float range = max - min;

        while (t < duration)
        {
            t += (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
            float normalized = Mathf.Clamp01(t / duration);

            // Smooth in/out interpolation
            float eased = Mathf.SmoothStep(0f, 1f, normalized);
            float value = start + range * eased;

            SetTextValue(value);
            yield return null;
        }

        SetTextValue(max);
        onComplete?.Invoke();
        runningCoroutine = null;
    }

    void SetTextValue(float v)
    {
        string formattedValue;

        if (displayAsInteger)
            formattedValue = Mathf.RoundToInt(v).ToString();
        else if (!string.IsNullOrEmpty(floatFormat))
            formattedValue = v.ToString(floatFormat);
        else
            formattedValue = v.ToString();

        string unitText = siUnit;
        if (!string.IsNullOrEmpty(unitText))
        {
            if (addSpaceBeforeUnit && !unitText.StartsWith(" "))
                unitText = " " + unitText;
        }

        targetText.text = formattedValue + unitText;
    }
}
