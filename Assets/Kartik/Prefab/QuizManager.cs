using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.ProceduralImage;
using DG.Tweening;
using DG.Tweening.Core;

[System.Serializable]
public class Question
{
    public string topic;                        // Topic or category (e.g., Solid, Liquid, Gas)
    [TextArea(2, 3)] public string prompt;      // Question text
    public string[] options;                    // Array of option texts
    public int correctIndex;                    // Index of correct option
    [TextArea(1, 2)] public string correctFeedback;
    [TextArea(1, 2)] public string wrongFeedback;
}

public class QuizManager : MonoBehaviour
{
    [Header("External")]
    public TypewriterTMP typewriterTMP;
   // public BoyDialogueBehaviour boyDialogueBehaviour;
    public AudioSource youdid, greatEfforts;

    [Header("UI References")]
    public TextMeshProUGUI topicText;
    public TextMeshProUGUI questionText;
    public Transform optionsParent;          // Empty object to hold buttons
    public GameObject optionButtonPrefab;    // Button prefab (Button + TMP child + Image)
    public TextMeshProUGUI feedbackText;
    public Button nextButton;

    [Header("Timer (per question)")]
    [Tooltip("UI Image that will act as the timer (Image.Type = Filled).")]
    public Image timerImage;
    [Tooltip("Child TMP text below/inside the timer showing seconds left.")]
    public TextMeshProUGUI timerText;
    [Tooltip("Time per question in seconds.")]
    public float questionTime = 20f;
    [Tooltip("Automatically start timer when question appears.")]
    public bool autoStartTimer = true;
    [Tooltip("Color at full time (green).")]
    public Color timerFullColor = new Color(0.211f, 0.635f, 0.255f, 1f); // green
    [Tooltip("Color at zero time (red).")]
    public Color timerEmptyColor = new Color(0.878f, 0.212f, 0.122f, 1f); // red

    [Header("Sprites (optional feedback)")]
    public Sprite greenTestTubeSprite;
    public Sprite redTestTubeSprite;
    public ProceduralImage happy, sad;
    public float ease_Speed;


    [Header("Badge System")]
    public Image badgeImage;                 // Optional image to show badge
    public Sprite matterMasterBadge;         // Sprite for badge

    [Header("Data")]
    public List<Question> questions = new List<Question>();

    [Header("Visual Settings (Inspector)")]
    [Tooltip("Default color for answer buttons.")]
    public Color defaultButtonColor = Color.white;
    [Tooltip("Text color for selected (clicked) option.")]
    public Color selectedTextColor = Color.white;
    [Tooltip("Default text color for options.")]
    public Color defaultTextColor = new Color(0.1f, 0.1f, 0.1f, 1f);

    [Tooltip("If true, use greenTestTubeSprite / redTestTubeSprite on feedback instead of tinting color.")]
    public bool useSpritesForFeedback = true;

    [Tooltip("If true, disable the clicked Button component (button.enabled = false). Other buttons become interactable = false.")]
    public bool disableClickedButtonComponent = true;

    // internal quiz state
    private int currentQuestion = 0;
    private bool answered = false;
    private List<Button> spawnedButtons = new List<Button>();

    // results tracking
    private int correctCount = 0;
    private int wrongCount = 0;

    // timer coroutine handle
    private Coroutine timerCoroutine;
    public GameObject crossButton, clock, tryAgainBadge;
    public GameObject completed_Quiz, select_Text,lastGameObject;
    public TextMeshProUGUI duplicateText;
    public GameObject right, wrong;

    void OnEnable()
    {
        // Make sure quiz restarts cleanly
        RestartQuiz();
    }

    void Start()
    {
        correctCount = 0;
        wrongCount = 0;
        nextButton.gameObject.SetActive(false);
        if (badgeImage != null) badgeImage.gameObject.SetActive(false);
        ShowQuestion();
    }

    void ShowQuestion()
    {
        // Reset answered state & feedback
        answered = false;
        feedbackText.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);

        // Cancel any existing timer
        StopTimer();

        // Destroy old buttons and clear list
        foreach (Transform child in optionsParent)
            Destroy(child.gameObject);
        spawnedButtons.Clear();

        if (questions == null || questions.Count == 0)
        {
            topicText.text = "";
            questionText.text = "No questions available.";
            return;
        }

        currentQuestion = Mathf.Clamp(currentQuestion, 0, Mathf.Max(0, questions.Count - 1));

        Question q = questions[currentQuestion];
        topicText.text = q.topic;
        questionText.text = q.prompt;

        // Spawn answer buttons
        for (int i = 0; i < q.options.Length; i++)
        {
            GameObject btnObj = Instantiate(optionButtonPrefab, optionsParent);
            Button btn = btnObj.GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogError("Option button prefab must have a Button component.");
                Destroy(btnObj);
                continue;
            }

            // Reset visual state
            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = defaultButtonColor;
                // optional: reset sprite to null if you replaced previously
                // img.sprite = null;
            }

            TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = q.options[i];
                label.color = defaultTextColor;
            }

            int index = i;
            // ensure no lingering listeners
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnOptionClicked(index));
            btn.interactable = true;
            btn.enabled = true; // re-enable component if disabled earlier
            spawnedButtons.Add(btn);
        }

        // Setup timer UI visuals
        if (timerImage != null)
        {
            timerImage.fillAmount = 1f;
            timerImage.color = timerFullColor;
        }
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(questionTime).ToString();
        }

        // Start timer automatically if requested
        if (autoStartTimer)
            StartTimer(questionTime);
    }

    /// <summary>
    /// Called when an answer button is clicked.
    /// </summary>
    void OnOptionClicked(int index)
    {
        if (answered) return;
        answered = true;

        // Stop timer (user answered)
        StopTimer();

        Question q = questions[currentQuestion];
        bool correct = index == q.correctIndex;

        // Track score
        if (correct) correctCount++;
        else wrongCount++;

        // Update feedback text and color
        Color greenColor = timerFullColor;
        Color redColor = timerEmptyColor;

        feedbackText.text = correct ? q.correctFeedback : q.wrongFeedback;
        feedbackText.color = correct ? greenColor : redColor;
        feedbackText.gameObject.SetActive(true);

        // Visual feedback for clicked button
        if (index >= 0 && index < spawnedButtons.Count)
        {
            Button clickedBtn = spawnedButtons[index];
            Image btnImage = clickedBtn.GetComponent<Image>();
            TextMeshProUGUI clickedLabel = clickedBtn.GetComponentInChildren<TextMeshProUGUI>();

            // change clicked text color to selected color (white)
            if (clickedLabel != null)
                clickedLabel.color = selectedTextColor;

            // apply sprite or color feedback
            if (useSpritesForFeedback && btnImage != null)
            {
                btnImage.sprite = correct ? greenTestTubeSprite : redTestTubeSprite;
                btnImage.color = Color.white; // ensure sprite shows correctly
            }
            else if (btnImage != null)
            {
                btnImage.color = correct ? greenColor : redColor;
            }

            // disable only the clicked Button component if requested
            if (disableClickedButtonComponent)
                clickedBtn.enabled = false;
            else
                clickedBtn.interactable = false;


            if (correct)
            {
                happy.transform.localScale = Vector3.zero;
                sad.transform.localScale = Vector3.zero;

                happy.transform.DOScale(new Vector3(3,3,3), ease_Speed).SetEase(Ease.InOutFlash);

            }
            else
            {
                happy.transform.localScale = Vector3.zero;
                sad.transform.localScale = Vector3.zero;

                sad.transform.DOScale(new Vector3(3,3,3), ease_Speed).SetEase(Ease.InOutFlash);
            }
        }

        // Make other buttons not interactable (so user cannot change answer)
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (i == index) continue;
            Button other = spawnedButtons[i];
            if (other == null) continue;

            Image otherImg = other.GetComponent<Image>();
            TextMeshProUGUI otherLabel = other.GetComponentInChildren<TextMeshProUGUI>();

            if (otherImg != null)
            {
                Color tint = otherImg.color;
                tint.a = 0.6f;
                otherImg.color = tint;
            }

            if (otherLabel != null)
            {
                otherLabel.color = new Color(otherLabel.color.r, otherLabel.color.g, otherLabel.color.b, 0.6f);
            }

            other.interactable = false;
        }

        // Show Next button
        nextButton.gameObject.SetActive(true);
    }

    public void ImageScale()
    {
         happy.transform.localScale = Vector3.zero;
        sad.transform.localScale = Vector3.zero;
    }

    #region Timer API

    /// <summary>
    /// Start the per-question countdown for 'seconds'.
    /// </summary>
    public void StartTimer(float seconds)
    {
        StopTimer(); // stop existing
        timerCoroutine = StartCoroutine(TimerCoroutine(seconds));
    }

    /// <summary>
    /// Stop the timer if running.
    /// </summary>
    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    IEnumerator TimerCoroutine(float seconds)
    {
        float elapsed = 0f;
        float total = Mathf.Max(0.0001f, seconds);

        while (elapsed < total)
        {
            if (answered) // if user answered while this loop runs, exit
                yield break;

            elapsed += Time.deltaTime;
            float remaining = Mathf.Clamp01(1f - (elapsed / total));
            // update fill (1 -> 0)
            if (timerImage != null)
                timerImage.fillAmount = remaining;

            // update color from green -> red
            if (timerImage != null)
                timerImage.color = Color.Lerp(timerEmptyColor, timerFullColor, remaining); // remaining near 1 = green

            // update countdown integer in text
            if (timerText != null)
            {
                int secsLeft = Mathf.CeilToInt((total - elapsed));
                secsLeft = Mathf.Max(0, secsLeft);
                timerText.text = secsLeft.ToString();
            }

            yield return null;
        }

        // Timer finished and user hasn't answered
        timerCoroutine = null;
        OnTimerExpired();
    }

    /// <summary>
    /// Called when the question timer expires without an answer.
    /// This will reveal the correct answer, disable buttons and show feedback.
    /// </summary>
    void OnTimerExpired()
    {
        if (answered) return;
        answered = true;

        // Play encouragement or reveal sound (optional)
        // (you could play a sound here)

        // Reveal correct answer visually
        Question q = questions[currentQuestion];
        int correctIndex = q.correctIndex;

        // Mark correct answer visually
        if (correctIndex >= 0 && correctIndex < spawnedButtons.Count)
        {
            Button correctBtn = spawnedButtons[correctIndex];
            Image btnImage = correctBtn.GetComponent<Image>();
            TextMeshProUGUI label = correctBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.color = selectedTextColor;

            if (useSpritesForFeedback && btnImage != null && greenTestTubeSprite != null)
            {
                btnImage.sprite = greenTestTubeSprite;
                btnImage.color = Color.white;
            }
            else if (btnImage != null)
            {
                btnImage.color = timerFullColor;
            }

            // optional disable clicked component
            if (disableClickedButtonComponent) correctBtn.enabled = false;
            else correctBtn.interactable = false;
        }

        // Mark other options visually as disabled
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (i == correctIndex) continue;
            Button other = spawnedButtons[i];
            if (other == null) continue;
            Image otherImg = other.GetComponent<Image>();
            TextMeshProUGUI otherLabel = other.GetComponentInChildren<TextMeshProUGUI>();

            if (otherImg != null)
            {
                Color tint = otherImg.color;
                tint.a = 0.6f;
                otherImg.color = tint;
            }
            if (otherLabel != null)
            {
                otherLabel.color = new Color(otherLabel.color.r, otherLabel.color.g, otherLabel.color.b, 0.6f);
            }

            other.interactable = false;
        }

        // Show feedback text telling correct answer
        feedbackText.gameObject.SetActive(true);
        feedbackText.color = timerFullColor;
        feedbackText.text = $"Time's up! The correct answer was highlighted.";

        // Show Next button
        nextButton.gameObject.SetActive(true);
    }

    #endregion

    public void OnNextQuestion()
    {
        // stop timer just in case
        StopTimer();

        currentQuestion++;

        if (currentQuestion < questions.Count)
        {
            ShowQuestion();
        }
        else
        {
            // --- End of Quiz ---
            ShowFinalResults();
        }
    }

    void ShowFinalResults()
    {
        topicText.text = "Quiz Completed!";
        questionText.text = "Results:";
        feedbackText.gameObject.SetActive(true);
        feedbackText.color = defaultTextColor;

        int total = questions.Count;
        int score = correctCount;
        bool perfectScore = score == total;

        if (perfectScore)
        {
            // 🎖️ Matter Master unlocked
            feedbackText.text = $" All {score}/{total} correct!\n Badge Unlocked: <b>Matter Master!</b>";
            lastGameObject.SetActive(true);
            duplicateText.text = feedbackText.text;
            right.SetActive(false);
            wrong.SetActive(false);
            if (badgeImage != null && matterMasterBadge != null)
            {
                badgeImage.sprite = matterMasterBadge;
                badgeImage.gameObject.SetActive(true);
               // if (boyDialogueBehaviour != null) { boyDialogueBehaviour.isOpen = false; boyDialogueBehaviour.OpenDialogueBox(); }
                if (typewriterTMP != null)
                {
                    clock.SetActive(false);
                    crossButton.SetActive(true);
                    completed_Quiz.SetActive(true);
                    select_Text.SetActive(false);
                  //  boyDialogueBehaviour.isOpen = false; boyDialogueBehaviour.OpenDialogueBox();
                    typewriterTMP.TypeText("You did it, Particle Explorer! You’ve mastered the States of Matter.", 15f);
                }
                if (youdid != null) youdid.Play();
            }
        }
        else
        {
           // if (boyDialogueBehaviour != null) { boyDialogueBehaviour.isOpen = false; boyDialogueBehaviour.OpenDialogueBox(); }
            if (typewriterTMP != null)
            {
              //  boyDialogueBehaviour.isOpen = false; boyDialogueBehaviour.OpenDialogueBox();
                tryAgainBadge.SetActive(true);
                typewriterTMP.TypeText("Great effort! Try again to earn your <b>Matter Master</b> badge.", 15f);
                clock.SetActive(false);
                crossButton.SetActive(true);
                completed_Quiz.SetActive(true);
                select_Text.SetActive(false);
            }
                
            if (greatEfforts != null) greatEfforts.Play();

            // Encouragement message
            feedbackText.text =
                $"Correct Answers: {correctCount}\nWrong Answers: {wrongCount}\n"
                ;
            duplicateText.text = feedbackText.text;
                    right.SetActive(true);
        wrong.SetActive(true);
            lastGameObject.SetActive(true);
            if (badgeImage != null)
                badgeImage.gameObject.SetActive(false);
        }

        // Cleanup buttons
        foreach (Transform child in optionsParent)
            Destroy(child.gameObject);
        spawnedButtons.Clear();

        nextButton.gameObject.SetActive(false);
    }

    public void RestartQuiz()
    {
        if (!gameObject.activeInHierarchy) return;
        ImageScale();
        correctCount = 0;
        clock.SetActive(true);
        crossButton.SetActive(false);
        tryAgainBadge.SetActive(false);
        completed_Quiz.SetActive(false);
        lastGameObject.SetActive(false);
        select_Text.SetActive(true);
        right.SetActive(false);
        wrong.SetActive(false);
        wrongCount = 0;
        currentQuestion = 0;
        if (badgeImage != null) badgeImage.gameObject.SetActive(false);
        ShowQuestion();
    }
}
