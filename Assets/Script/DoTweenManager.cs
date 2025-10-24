using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class DoTweenManager : MonoBehaviour
{
    public bool isDebugState;
    public static DoTweenManager Instance;

    public AudioManager audioManager;
    public TMPValueLerp tMPValueLerp;
    public TMPValueLerp tMPValueLerp3d;
    public TypewriterTMP typewriterTMPboard;
    public RectTransform inst; 
    [SerializeField] GameObject gamePlanCanvas;
    [SerializeField] GameObject dialogueBox;
    [SerializeField] AudioSource lets_Begin;
    [SerializeField] TypewriterTMP typewriter;
    [SerializeField] GameObject solidBeakeroutline;
    //[SerializeField] AudioSource notice;
    [SerializeField] GameObject liquidBeaker;
  //  [SerializeField] AudioSource lets_Look_Liq;

    [SerializeField] GameObject gasBeaker;
    //[SerializeField] AudioSource gas_Audio;
   // [SerializeField] AudioSource gas_Completed;
    public SolidParticleStacker solidParticleStacker;
    public LiquidParticleStacker liquidParticleStacker;
    public GasParticleStacker gasParticleStacker;
    public GameObject solidLabel;
    public GameObject liqudLabel;
    public GameObject gasLabel;
    public GameObject solid, liquid, gas;


    [Header("Compare States")]
    public GameObject compareStates;
    public Collider[] colliders;
   // public AudioSource compareState_Audio, click_Next;

    public bool one, two, three, compareState;



    [Header("State Changes")]
    public UIPanelPivotLerp uIPanelPivotLerp;
    public GameObject stateChange;
    // public AudioSource clickHeat;
    // public AudioSource clickCold;
    // public AudioSource mealting, boiling, condensation, freezing, greatWork;

    public Animator animator;
    public Button nextBtn, clickHeatBtn;
    public Transform orbitTarget, ice_Cubes, seconBeaker,newTarget;
    public ProceduralImage thermo;
    [Header("Quiz")]
    // public AudioSource lets_See;
    public GameObject quiz_Panel;

    [Header("Dialogues Clips")]
    [SerializeField]
    AudioClip click_Solid, click_Beaker1, click_Beaker2, solid_Dialogue, click_Liquid, liquid_Dialogue, click_Gas, gas_Dialogue, comapre,
                                tryHeating, clickHeat, meltingV2, increaseTemp, boiling, clickCold, clickPlate, liqtoIce, greatWork;
    [Header("HighLightes")]
    [SerializeField] GameObject solidBtn, liquidBtn, gasBtn, compareBtn, stagesBtn, heatHigh;
    [SerializeField] Button heatBtn;
    [HideInInspector] public bool state_S, state_L, state_G,comapreDialogueCompleted;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        thermo.fillAmount = 0.35f;
        if (isDebugState) return;


        nextBtn.gameObject.SetActive(false);
        stateChange.gameObject.SetActive(false);
       // solid.SetActive(true);
        solidLabel.SetActive(true);
      //  solidBeakeroutline.gameObject.SetActive(true);

        dialogueBox.transform.localScale = Vector3.zero;
        gamePlanCanvas.SetActive(true);
        nextBtn.onClick.AddListener(delegate
        {
            nextBtn.transform.localScale = Vector3.zero; StartStateChange();
            AdvancedOrbitCamera.instance.defaultZoom = 1.75f; AdvancedOrbitCamera.instance.defaultHorizontalRotation = 180f;
            AdvancedOrbitCamera.instance.defaultVerticalRotation = 20f; AdvancedOrbitCamera.instance.ApplyInitialView();
        });
    }

    void Update()
    {
        if (one && two && three)
        {
            Debug.Log("Send");
            compareState = true;
            one = false;
            two = false;
            three = false;
        }



        CompareCompleted();
    }

    public void ActivateNextBtn()
    {

        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();

        typewriter.TypeText("Click on the highlighted next button to move forward with the simulation.", 15f, () => { nextBtn.gameObject.SetActive(true); });
        //click_Next.Play();
    }

    public void GamePlanDeactivate()
    {
        gamePlanCanvas.transform.localScale = Vector3.one;
        gamePlanCanvas.transform.DOScale(Vector3.zero, .25f).SetEase(Ease.InBack);
        Invoke(nameof(OpenDialogueBox), 1f);
    }

    public void OpenDialogueBox()
    {
        dialogueBox.transform.localScale = Vector3.zero;
        dialogueBox.transform.DOScale(Vector3.one, .15f).SetEase(Ease.OutFlash);
        // typewriter.TypeText("Click on any of these given buttons.", 15f);
        solidBtn.SetActive(true);
        typewriter.TypeText("Let’s begin with solids. Click on solid", 15f, () =>
        {
            // solidBeakeroutline.GetComponent<MeshCollider>().enabled = true; solidBeakeroutline.GetComponent<OutlineBehaviour>().enabled = true;
        });
        audioManager.PlaySpecificDialogue(click_Solid);
        //lets_Begin.Play();
    }

    public void ClickBeaker()
    {
    
        if (state_S || state_G)
        {
            BoyWindowPopUp();
            typewriter.TypeText("Now tap the beaker to see the particles move", 15f);
            audioManager.PlaySpecificDialogue(click_Beaker1);
            solidBtn.SetActive(false);
        }
        if (state_L)
        {

            BoyWindowPopUp();
            typewriter.TypeText("Tap the beaker and see what happens", 15f);
            audioManager.PlaySpecificDialogue(click_Beaker2);
            solidBtn.SetActive(false);
        }
        else
        {
                      BoyWindowPopUp();
            typewriter.TypeText("Now tap the beaker to see the particles move", 15f);
            audioManager.PlaySpecificDialogue(click_Beaker1);
            solidBtn.SetActive(false);
        }
  
    
        
    }

    public void SolidStateComplete()
    {

        BoyWindowPopUp();
        typewriter.TypeText("See how they are packed closely and only vibrate? That’s why solids have a fixed shape and volume.", 15f);

        audioManager.PlaySpecificDialogue(solid_Dialogue);
        Invoke(nameof(LetStartLiqState), 10f);
    }

    private static void BoyWindowPopUp()
    {
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
    }

    public void LetStartLiqState()
    {
        if (!state_S) return;
        liquidBtn.SetActive(true);
       // solid.SetActive(false);
      //  solidParticleStacker.gameObject.SetActive(false);
      //  solidLabel.SetActive(false);
      //  liqudLabel.SetActive(true);
       // liquid.SetActive(true);
       // liquidParticleStacker.gameObject.SetActive(true);
        BoyWindowPopUp();
        typewriter.TypeText("Now, let’s look at liquids.", 15f, () =>
        {

            // liquidBeaker.GetComponent<MeshCollider>().enabled = true; liquidBeaker.GetComponent<OutlineBehaviour>().enabled = true;
        });
        audioManager.PlaySpecificDialogue(click_Liquid);
    }

    public void LiquidStateComplete()
    {

        BoyWindowPopUp();
        typewriter.TypeText("See that? Liquid particles can move past each other. That’s why liquids don’t have a fixed shape—but they do have a fixed volume.", 15f);

        audioManager.PlaySpecificDialogue(liquid_Dialogue);
        Invoke(nameof(LetStartGasState), 12f);
    }

    public void LetStartGasState()
    {
        if (!state_L) return;
        gasBtn.SetActive(true);
       // liquid.SetActive(false);
       // liquidParticleStacker.gameObject.SetActive(false);
       // liqudLabel.SetActive(false);
        //gasLabel.SetActive(true);
        // gas.SetActive(true);
        // gasParticleStacker.gameObject.SetActive(true);
      
        typewriter.TypeText("Finally, check out gases.", 15f, () =>
        {
          //  gasBeaker.GetComponent<MeshCollider>().enabled = true; gasBeaker.GetComponent<OutlineBehaviour>().enabled = true;
        });
        audioManager.PlaySpecificDialogue(click_Gas);

    }

    public void GasStateCompleted()
    {
        state_S = false;
        state_L = false;
        state_G = true;
        BoyWindowPopUp();
        typewriter.TypeText("Gas particles move freely and spread to fill the whole container. That’s why gases have no fixed shape or volume!", 15f, () => compareBtn.SetActive(true));
        audioManager.PlaySpecificDialogue(gas_Dialogue);


    }

    public void Compare()
    {
        BoyWindowPopUp();
        typewriter.TypeText("Let’s compare them! Click each state to see how particle arrangement and motion are different.", 15f, () => comapreDialogueCompleted = true);
        audioManager.PlaySpecificDialogue(comapre);
    }
    public void CompareCompleted()
    {
        if (compareState && comapreDialogueCompleted)
        {
            stagesBtn.SetActive(true);
            comapreDialogueCompleted = false;
        }
    }

    public void StateChangesIntro()
    {
        heatHigh.SetActive(false);
        heatBtn.interactable = false;
        audioManager.PlaySpecificDialogue(tryHeating);
        BoyWindowPopUp();
        typewriter.TypeText("Now let’s see how matter changes from one state to another. Try heating or cooling the particles!", 15f, () => ClickHeat());
    }

    public void ClickHeat()
    {
        heatBtn.interactable = true;
        heatHigh.SetActive(true);
        audioManager.PlaySpecificDialogue(clickHeat);
        BoyWindowPopUp();
        typewriter.TypeText("Click Heat, then click the highlighted button to increase the temperature", 15f);
    }

    public void StartCompareStates()
    {
        gasLabel.SetActive(false);
        gas.SetActive(false);
        gasParticleStacker.gameObject.SetActive(false);
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        typewriter.TypeText("Click on each state to compare them. Observe how particle arrangement and motion differ.", 15f);
      //  compareState_Audio.Play();
        compareStates.SetActive(true);
        foreach (Collider item in colliders)
        {
            item.enabled = true;
        }
    }

    public void StartStateChange()
    {
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        compareStates.SetActive(false);
        stateChange.SetActive(true);
        uIPanelPivotLerp.transform.localScale = Vector3.one;
        uIPanelPivotLerp.TogglePanel();
      //  inst.anchoredPosition = new Vector2(15, 15);
        typewriter.TypeText("Click Heat to warm the ice and watch the particles change.", 15f, () => { clickHeatBtn.interactable = true; });
      //  clickHeat.Play();
    }

    public void HeatButton()
    {
        BoyWindowPopUp();
        orbitTarget.SetParent(ice_Cubes);
        orbitTarget.transform.localPosition = Vector3.zero;
        animator.Play("Mealting");
      //  typewriter.TypeText("Heating makes ice melt into liquid water.", 15f);
      //  mealting.Play();
        //Invoke(nameof(Temp), 1.5f);
        //Invoke(nameof(Boiling), 10f);
    }

    public void PlayMeltingV2()
    {
        animator.Play("MealtingV2");
        typewriter.TypeText("Watch the particles change from solid to liquid.", 15f);
        audioManager.PlaySpecificDialogue(meltingV2);

        Temp();
    }
    public void Temp()
    {
        tMPValueLerp.StartLerp(-10, 0, 10f);
        tMPValueLerp3d.StartLerp(-10, 0, 10f);
        Invoke(nameof(IncreaseTempMore), 13f);
    }

    public void IncreaseTempMore()
    {
        audioManager.PlaySpecificDialogue(increaseTemp);
        typewriter.TypeText("Increase the temperature more to show boiling.", 15f);
    }
    
    public void Boiling()
    {

        animator.Play("Boiling");
        tMPValueLerp.StartLerp(0, 100, 11f);
        tMPValueLerp3d.StartLerp(0, 100, 11f);
        Invoke(nameof(BoilingAudio), 3f);

    }
    public void BoilingAudio()
    {
        BoyWindowPopUp();
        typewriter.TypeText("Heating makes ice melt into water. More heat turns water into gas.", 15f);
        audioManager.PlaySpecificDialogue(boiling);
        //  boiling.Play();
    }

    public void ClickCool()
    {
        audioManager.PlaySpecificDialogue(clickCold);
        BoyWindowPopUp();
        typewriter.TypeText("Click Cold, then click the highlighted ice plate to start the condensation process", 15f);
    }

    public void ClickPlate()
    {
        audioManager.PlaySpecificDialogue(clickPlate);
        BoyWindowPopUp();
        typewriter.TypeText("Cooling changes gas into liquid.", 15f);
        Invoke(nameof(ClickBeakerLiqtoIce), 18f);
    }
    public void ClickBeakerLiqtoIce()
    {
        audioManager.PlaySpecificDialogue(liqtoIce);
        BoyWindowPopUp();
        typewriter.TypeText("Click on the beaker to further cool. This freezes the liquid into solid ice.", 15f);
    }
    public void CoolButton()
    {
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        typewriter.TypeText("Click Cool to lower the temperature and reverse the changes.", 15f);
      //  clickCold.Play();

    }

    public void Condensation()
    {
        animator.Play("Condensation");
        Invoke(nameof(CondensationAudio), 5f);

    }

    public void CondensationAudio()
    {
        tMPValueLerp.StartLerp(100, 25, 11);
        // BoyDialogueBehaviour.Instance.isOpen = false;
        // BoyDialogueBehaviour.Instance.OpenDialogueBox();
        // typewriter.TypeText("Cooling gas condenses into liquid.", 15f);
       // condensation.Play();
       // Invoke(nameof(Freezing), 10f);
    }

    public void Freezing()
    {
        Debug.Log("Freezing");
        orbitTarget.SetParent(seconBeaker);
        orbitTarget.transform.localPosition = Vector3.zero;
        Invoke(nameof(LastTemp), 2.5f);

        animator.Play("Freezing");
        // typewriter.TypeText("Further cooling freezes liquid into solid ice.", 15f);
      //  freezing.Play();
        Invoke(nameof(StateChangesEND), 12f);

    }
    public void LastTemp()
    {
        tMPValueLerp.StartLerp(25, -10, 4f);
        Invoke(nameof(End), 7f);
    }

    public void End()
    {
        BoyWindowPopUp();
        typewriter.TypeText("Great work! Here are the takeaways from our simulation.", 15f);
        audioManager.PlaySpecificDialogue(greatWork);
    }
    public void StateChangesEND()
    {
        // BoyDialogueBehaviour.Instance.isOpen = false;
        // BoyDialogueBehaviour.Instance.OpenDialogueBox();

        // typewriter.TypeText("Great work! Here are the takeaways from our simulation.", 15f);
       // greatWork.Play();
       // Invoke(nameof(BlackBoardText), 1f);
    }
    public void BlackBoardText()
    {

        typewriterTMPboard.TypeText("1. In solids, particles are tightly packed, giving a fixed shape and volume.\n \n2. In liquids, particles slide past one another—fixed volume but no fixed shape.\n \n3. In gases, particles are far apart and move freely—no fixed shape or volume.\n \n4. Heating or cooling changes the speed of particles, leading to changes of state.", 40f);
        //        StartCoroutine(targetShift());
        Invoke(nameof(QuizState), 10f);
    }


    public void QuizState()
    {

        nextBtn.onClick.RemoveAllListeners();
        nextBtn.onClick.AddListener(delegate
        {
            nextBtn.transform.localScale = Vector3.zero;uIPanelPivotLerp.gameObject.transform.localScale = Vector3.zero;
            quiz_Panel.SetActive(true);
            BoyDialogueBehaviour.Instance.isOpen = true; BoyDialogueBehaviour.Instance.OpenDialogueBox();
        });
        
        
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();

        typewriter.TypeText("Let’s see what you’ve learned. Click on the highlighted next button for an exciting quiz.", 15f,()=> { nextBtn.transform.localScale = Vector3.one; });
      //  lets_See.Play();
        
    }
    public void ResetTargetPosition()
    {
        orbitTarget.SetParent(newTarget.transform);
        orbitTarget.transform.localPosition = Vector3.zero;
    }

}
