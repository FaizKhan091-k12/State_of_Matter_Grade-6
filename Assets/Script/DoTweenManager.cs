using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DoTweenManager : MonoBehaviour
{
    public bool isDebugState;
    public static DoTweenManager Instance;
    public TMPValueLerp tMPValueLerp;
    public TypewriterTMP typewriterTMPboard;
    [SerializeField] GameObject gamePlanCanvas;
    [SerializeField] GameObject dialogueBox;
    [SerializeField] AudioSource lets_Begin;
    [SerializeField] TypewriterTMP typewriter;
    [SerializeField] GameObject solidBeakeroutline;
    [SerializeField] AudioSource notice;
    [SerializeField] GameObject liquidBeaker;
    [SerializeField] AudioSource lets_Look_Liq;

    [SerializeField] GameObject gasBeaker;
    [SerializeField] AudioSource gas_Audio;
    [SerializeField] AudioSource gas_Completed;
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
    public AudioSource compareState_Audio, click_Next;

    public bool one, two, three, compareState;



    [Header("State Changes")]
    public UIPanelPivotLerp uIPanelPivotLerp;
    public GameObject stateChange;
    public AudioSource clickHeat;
    public AudioSource clickCold;
    public AudioSource mealting, boiling, condensation, freezing, greatWork;

    public Animator animator;
    public Button nextBtn, clickHeatBtn;
    public Transform orbitTarget, ice_Cubes, seconBeaker,newTarget;

    [Header("Quiz")]
    public AudioSource lets_See;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {


        if (isDebugState) return;


        nextBtn.gameObject.SetActive(false);
        stateChange.gameObject.SetActive(false);
        solid.SetActive(true);
        solidLabel.SetActive(true);
        solidBeakeroutline.gameObject.SetActive(true);

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
            Invoke(nameof(ActivateNextBtn), 5f);
            one = false;
            two = false;
            three = false;
        }
    }

    public void ActivateNextBtn()
    {

        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();

        typewriter.TypeText("Click on the highlighted next button to move forward with the simulation.", 15f, () => { nextBtn.gameObject.SetActive(true); });
        click_Next.Play();
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
        typewriter.TypeText("Let’s begin with solids. Look at these particles. Click on them to observe their motion.", 15f, () =>
        {
            solidBeakeroutline.GetComponent<BoxCollider>().enabled = true; solidBeakeroutline.GetComponent<OutlineBehaviour>().enabled = true;
        });
        lets_Begin.Play();
    }

    public void SolidStateComplete()
    {
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        typewriter.TypeText("Notice how they are packed closely and only vibrate. That’s why solids have a fixed shape and volume.", 15f);
        notice.Play();
        Invoke(nameof(LetStartLiqState), 10f);
    }

    public void LetStartLiqState()
    {
        solid.SetActive(false);
        solidParticleStacker.gameObject.SetActive(false);
        solidLabel.SetActive(false);
        liqudLabel.SetActive(true);
        liquid.SetActive(true);
        liquidParticleStacker.gameObject.SetActive(true);
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        typewriter.TypeText("Now, let’s look at liquids. Tap the container wall and see what happens.", 15f, () =>
        {
            liquidBeaker.GetComponent<MeshCollider>().enabled = true; liquidBeaker.GetComponent<OutlineBehaviour>().enabled = true;
        });
        lets_Look_Liq.Play();
    }

    public void LiquidStateComplete()
    {
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        typewriter.TypeText("See that? Liquid particles can move past each other. That’s why liquids don’t have a fixed shape—but they do have a fixed volume.", 15f);

        Invoke(nameof(LetStartGasState), 12f);
    }

    public void LetStartGasState()
    {
        liquid.SetActive(false);
        liquidParticleStacker.gameObject.SetActive(false);
        liqudLabel.SetActive(false);
        gasLabel.SetActive(true);
        gas.SetActive(true);
        gasParticleStacker.gameObject.SetActive(true);
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        typewriter.TypeText("Finally, let’s check out gases. Tap the container wall and see what happens.", 15f, () =>
        {
            gasBeaker.GetComponent<MeshCollider>().enabled = true; gasBeaker.GetComponent<OutlineBehaviour>().enabled = true;
        });
        gas_Audio.Play();

    }

    public void GasStateCompleted()
    {
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        typewriter.TypeText("Gas particles move freely and spread to fill the whole container. That’s why gases have no fixed shape or volume!", 15f);
        gas_Completed.Play();
        Invoke(nameof(StartCompareStates), 9f);

    }

    public void StartCompareStates()
    {
        gasLabel.SetActive(false);
        gas.SetActive(false);
        gasParticleStacker.gameObject.SetActive(false);
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        typewriter.TypeText("Click on each state to compare them. Observe how particle arrangement and motion differ.", 15f);
        compareState_Audio.Play();
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
        uIPanelPivotLerp.TogglePanel();
        typewriter.TypeText("Click Heat to warm the ice and watch the particles change.", 15f, () => { clickHeatBtn.interactable = true; });
        clickHeat.Play();
    }

    public void HeatButton()
    {
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        orbitTarget.SetParent(ice_Cubes);
        orbitTarget.transform.localPosition = Vector3.zero;
        animator.SetBool("Mealting", true);
        typewriter.TypeText("Heating makes ice melt into liquid water.", 15f);
        mealting.Play();
        Invoke(nameof(Temp), 1.5f);
        Invoke(nameof(Boiling), 10f);
    }
    public void Temp()
    {
        tMPValueLerp.StartLerp(-10, 0, 4.5f);
    }
    public void Boiling()
    {
        animator.SetBool("Boiling", true);
        tMPValueLerp.StartLerp(0, 40, 3f);
        Invoke(nameof(BoilingAudio), 3f);

    }
    public void BoilingAudio()
    {
        tMPValueLerp.StartLerp(40, 100, 3f);
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        typewriter.TypeText("More heat turns water into gas.", 15f);
        boiling.Play();
    }
    public void CoolButton()
    {
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        typewriter.TypeText(" Click Cool to lower the temperature and reverse the changes.", 15f);
        clickCold.Play();

    }

    public void Condensation()
    {
        animator.SetBool("Condensation", true);
        Invoke(nameof(CondensationAudio), 4f);

    }

    public void CondensationAudio()
    {
        tMPValueLerp.StartLerp(100, 25, 8);
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        typewriter.TypeText("Cooling gas condenses into liquid.", 15f);
        condensation.Play(); Debug.Log("Cooling");
        Invoke(nameof(Freezing), 10f);
    }

    public void Freezing()
    {
        Debug.Log("Freezing");
        orbitTarget.SetParent(seconBeaker);
        orbitTarget.transform.localPosition = Vector3.zero;
        Invoke(nameof(LastTemp), 2.5f);
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        animator.SetBool("Freezing", true);
        typewriter.TypeText("Further cooling freezes liquid into solid ice.", 15f);
        freezing.Play();
        Invoke(nameof(StateChangesEND), 12f);

    }
    public void LastTemp()
    {
        tMPValueLerp.StartLerp(25, -100, 6.5f);
    }
    public void StateChangesEND()
    {
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();

        typewriter.TypeText("Great work! Here are the takeaways from our simulation.", 15f);
        greatWork.Play();
        Invoke(nameof(BlackBoardText), 1f);
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
        nextBtn.onClick.AddListener(delegate { nextBtn.transform.localScale = Vector3.zero; });
        
        
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();

        typewriter.TypeText("Let’s see what you’ve learned. Click on the highlighted next button for an exciting quiz.", 15f,()=> { nextBtn.transform.localScale = Vector3.one; });
        lets_See.Play();
        
    }

    IEnumerator targetShift()
    {

        float t = 0f;
        while (t < 1)
        {
            orbitTarget.transform.localPosition = Vector3.Lerp(orbitTarget.transform.localPosition, newTarget.transform.localPosition, t * .2f);
            yield return null;
        }
    }
}
