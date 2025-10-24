using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MainMenuButtonsBehaviour : MonoBehaviour
{

    public ButtonsBehaviour buttonsBehaviour;
    [SerializeField] Button[] menuBtn;
    [SerializeField] GameObject[] highlighted;

    [SerializeField] public bool[] buttonclicked;
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
            buttonsBehaviour.ButtonScaleToZero();
            for (int i = 0; i < buttonclicked.Length; i++)
            {
                buttonclicked[i] = false;
            }
            buttonclicked[0] = true;
        });


        menuBtn[1].onClick.AddListener(delegate
        {
            foreach (var item in highlighted)
            {
                item.SetActive(false);
                highlighted[1].SetActive(true);
            }
           // CompareButtonClick();
            buttonsBehaviour.ButtonScaleToZero();
            for (int i = 0; i < buttonclicked.Length; i++)
            {
                buttonclicked[i] = false;
            }
            buttonclicked[1] = true;
        });


        menuBtn[2].onClick.AddListener(delegate
        {
            foreach (var item in highlighted)
            {
                item.SetActive(false);
                highlighted[2].SetActive(true);
            }
           // StageButtonClick();
            for (int i = 0; i < buttonclicked.Length; i++)
            {
                buttonclicked[i] = false;
            }
            buttonclicked[2] = true;
        });

        menuBtn[3].onClick.AddListener(delegate
        {
            foreach (var item in highlighted)
            {
                item.SetActive(false);
                highlighted[3].SetActive(true);
            }
            for (int i = 0; i < buttonclicked.Length; i++)
            {
                buttonclicked[i] = false;
            }
            buttonclicked[3] = true;
        });
    }


    public void CompareButtonClick()
    {
        CompareEvent.Invoke();


    }

    public void StageButtonClick()
    {
        StagesEvent.Invoke();
    }
    public void IntroButtonClicked()
    {
        IntroductionEvent.Invoke();
    }
    public void QuizButtonClicked()
    {
        QuizEvent.Invoke();
    }
}
