using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using NUnit.Framework.Internal;

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
    public GameObject resetBtn;
    public TypewriterTMP typewriterTMP;
    public BoyDialogueBehaviour boyDialogueBehaviour;
    public AudioSource youdid, greatEfforts;
    [Header("UI References")]
    public TextMeshProUGUI topicText;
    public TextMeshProUGUI questionText;
    public Transform optionsParent;          // Empty object to hold buttons
    public GameObject optionButtonPrefab;    // Button prefab (Button + TMP child + Image)
    public TextMeshProUGUI feedbackText;
    public Button nextButton;

    [Header("Sprites")]
    public Sprite greenTestTubeSprite;
    public Sprite redTestTubeSprite;

    [Header("Badge System")]
    public Image badgeImage;                 // Optional image to show badge
    public Sprite matterMasterBadge;         // Sprite for badge

    [Header("Data")]
    public List<Question> questions = new List<Question>();

    private int currentQuestion = 0;
    private bool answered = false;
    private List<Button> spawnedButtons = new List<Button>();

    // Results tracking
    private int correctCount = 0;
    private int wrongCount = 0;

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
        answered = false;
        feedbackText.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);

        // Destroy old buttons
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
            TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = q.options[i];

            int index = i;
            btn.onClick.AddListener(() => CheckAnswer(index));
            spawnedButtons.Add(btn);
        }
    }

    void CheckAnswer(int index)
    {
        if (answered) return;
        answered = true;

        Question q = questions[currentQuestion];
        bool correct = index == q.correctIndex;

        // Track score
        if (correct) correctCount++;
        else wrongCount++;

        // Define custom colors
        Color greenColor, redColor;
        ColorUtility.TryParseHtmlString("#36A241", out greenColor); // green
        ColorUtility.TryParseHtmlString("#E0361F", out redColor);   // red

        feedbackText.text = correct ? q.correctFeedback : q.wrongFeedback;
        feedbackText.color = correct ? greenColor : redColor;
        feedbackText.gameObject.SetActive(true);

        // Change clicked button sprite
        Button clickedBtn = spawnedButtons[index];
        Image btnImage = clickedBtn.GetComponent<Image>();
        if (btnImage != null)
            btnImage.sprite = correct ? greenTestTubeSprite : redTestTubeSprite;

        // Disable all buttons
        foreach (Button btn in spawnedButtons)
            btn.interactable = false;

        nextButton.gameObject.SetActive(true);
    }

    public void OnNextQuestion()
    {
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
        topicText.text = "🎉 Quiz Completed!";
        questionText.text = "Results:";
        feedbackText.gameObject.SetActive(true);
        feedbackText.color = Color.blue;

        int total = questions.Count;
        int score = correctCount;
        bool perfectScore = score == total;

        if (perfectScore)
        {
            // 🎖️ Matter Master unlocked
            feedbackText.text = $"✅ All {score}/{total} correct!\n🏅 Badge Unlocked: <b>Matter Master!</b>";
            if (badgeImage != null && matterMasterBadge != null)
            {
                badgeImage.sprite = matterMasterBadge;
                badgeImage.gameObject.SetActive(true);
                BoyDialogueBehaviour.Instance.isOpen = false;
                BoyDialogueBehaviour.Instance.OpenDialogueBox();

                typewriterTMP.TypeText("You did it, Particle Explorer! You’ve mastered the States of Matter.", 15f);
                youdid.Play();
                Invoke(nameof(ResetBtn), 6f);
        
            }
        }
        else
        {
            BoyDialogueBehaviour.Instance.isOpen = false;
            BoyDialogueBehaviour.Instance.OpenDialogueBox();
            typewriterTMP.TypeText("Great effort! Try again to earn your <b>Matter Master</b> badge.", 15f);
            greatEfforts.Play();
            Invoke(nameof(ResetBtn), 6f);
            // Encouragement message
            feedbackText.text =
                $"Correct Answers: {correctCount}\nWrong Answers: {wrongCount}\n" +
                "Great effort! Try again to earn your <b>Matter Master</b> badge.";
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
        correctCount = 0;
        wrongCount = 0;
        currentQuestion = 0;
        if (badgeImage != null) badgeImage.gameObject.SetActive(false);
        ShowQuestion();
    }

    public void ResetBtn()
    {
        resetBtn.SetActive(true);
    }
}
