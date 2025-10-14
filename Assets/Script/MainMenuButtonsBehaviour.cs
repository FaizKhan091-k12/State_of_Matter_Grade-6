using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuButtonsBehaviour : MonoBehaviour
{
    public ButtonsBehaviour buttonsBehaviour;
    [SerializeField] Button[] menuBtn;
    [SerializeField] GameObject[] highlighted;

    public UnityEvent IntroductionEvent, CompareEvent, StagesEvent, QuizEvent;

    void Start()
    {
        menuBtn[0].onClick.AddListener(delegate
        {
            foreach (var item in highlighted)
            {
                item.SetActive(false);
                highlighted[0].SetActive(true);
            }
            IntroductionEvent.Invoke();
        });


        menuBtn[1].onClick.AddListener(delegate
        {
            foreach (var item in highlighted)
            {
                item.SetActive(false);
                highlighted[1].SetActive(true);
            }
            CompareButtonClick();
            buttonsBehaviour.ButtonScaleToZero();
        });


        menuBtn[2].onClick.AddListener(delegate
        {
            foreach (var item in highlighted)
            {
                item.SetActive(false);
                highlighted[2].SetActive(true);
            }
        });

        menuBtn[3].onClick.AddListener(delegate
        {
            foreach (var item in highlighted)
            {
                item.SetActive(false);
                highlighted[3].SetActive(true);
            }
        });
    }


    public void CompareButtonClick()
    {
        CompareEvent.Invoke();
        
    }
}
