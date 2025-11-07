using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TMP_Text))]
public class TypewriterTMP : MonoBehaviour
{

    [Header("Target")]
    [SerializeField] public TMP_Text targetText;

    [Header("Timing")]
    [Tooltip("Characters per second (base).")]
    public float charactersPerSecond = 40f;
    [Tooltip("If true, whitespace characters are instant (no delay).")]
    public bool skipWhitespaceDelay = true;
    [Tooltip("Extra multiplier for punctuation pauses (.,!?).")]
    public float punctuationPauseMultiplier = 3f;
    [Tooltip("If true use unscaled time (not affected by Time.timeScale).")]
    public bool useUnscaledTime = false;

    [Header("Behaviour")]
    [Tooltip("Clear the text when typing starts.")]
    public bool clearOnStart = true;
    [Tooltip("If true, HTML/rich-text tags are preserved and not typed char-by-char.")]
    public bool supportRichTextTags = true;

    [Header("Events")]
    public UnityEvent onComplete;
    public UnityEvent onStartTyping;
    public UnityEvent onSkipToEnd;

    // C# callbacks
    public Action onCompleteCallback;
    public Action<char> onCharCallback;

    Coroutine typingCoroutine;
    string fullText = "";

    void Reset()
    {
        targetText = GetComponent<TMP_Text>();
    }

    void Awake()
    {
        if (targetText == null) targetText = GetComponent<TMP_Text>();
        if (clearOnStart) targetText.text = "";
    }

    /// <summary>
    /// Start typing the provided text. Optionally override characters-per-second for this call.
    /// </summary>
    public void TypeText(string text, float? charsPerSecond = null, Action onCompleteOnce = null)
    {
        if (text == null) text = "";
        float cps = charsPerSecond.HasValue && charsPerSecond.Value > 0f ? charsPerSecond.Value : Mathf.Max(0.0001f, charactersPerSecond);
        StartTypingInternal(text, cps, onCompleteOnce);
    }


    /// <summary>
    /// Stop typing and keep current visible text.
    /// </summary>
    public void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    /// <summary>
    /// Immediately show the full text and fire completion events.
    /// </summary>
    public void SkipToEnd()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        targetText.text = fullText;
        onSkipToEnd?.Invoke();
        onComplete?.Invoke();
        onCompleteCallback?.Invoke();
    }

    public bool IsTyping() => typingCoroutine != null;

    void StartTypingInternal(string text, float cps, Action onCompleteOnce = null)
    {
        // cancel any existing
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        fullText = text;
        if (clearOnStart) targetText.text = "";

        typingCoroutine = StartCoroutine(TypeCoroutine(text, cps, onCompleteOnce));
        onStartTyping?.Invoke();
    }

    IEnumerator TypeCoroutine(string text, float charsPerSec, Action onCompleteOnce = null)
    {
        float baseDelay = 1f / Mathf.Max(1e-4f, charsPerSec);
        StringBuilder visible = new StringBuilder();
        int i = 0;

        while (i < text.Length)
        {
            char c = text[i];

            if (supportRichTextTags && c == '<')
            {
                int tagStart = i;
                int tagEnd = text.IndexOf('>', tagStart);
                if (tagEnd >= 0)
                {
                    visible.Append(text.Substring(tagStart, tagEnd - tagStart + 1));
                    i = tagEnd + 1;
                    targetText.text = visible.ToString();
                    continue;
                }
            }

            visible.Append(c);
            targetText.text = visible.ToString();
            onCharCallback?.Invoke(c);

            float delay = skipWhitespaceDelay && char.IsWhiteSpace(c)
                ? 0f
                : (1f / charsPerSec) * ((c == '.' || c == '!' || c == '?' || c == ',') ? punctuationPauseMultiplier : 1f);

            if (delay > 0f)
            {
                if (useUnscaledTime) yield return new WaitForSecondsRealtime(delay);
                else yield return new WaitForSeconds(delay);
            }
            else yield return null;

            i++;
        }

        typingCoroutine = null;
        onComplete?.Invoke();              // fires global listeners
        onCompleteCallback?.Invoke();      // fires global code listeners
        onCompleteOnce?.Invoke();          // fires this-call-only callback ✅
    }

}
