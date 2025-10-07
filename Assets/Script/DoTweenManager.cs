using DG.Tweening;
using UnityEngine;

public class DoTweenManager : MonoBehaviour
{
    [SerializeField] GameObject gamePlanCanvas;
    [SerializeField] GameObject dialogueBox;
    [SerializeField] AudioSource lets_Begin;
    [SerializeField] TypewriterTMP typewriter;
    [SerializeField] GameObject solidBeaker;
    [SerializeField] AudioSource notice;
    [SerializeField] GameObject liquidBeaker;
    [SerializeField] AudioSource lets_Look_Liq;

    [SerializeField] GameObject gasBeaker;
    [SerializeField] AudioSource gas_Audio;
    [SerializeField] AudioSource gas_Completed;

    [SerializeField] Animation anim;
    [SerializeField] string anim_Name;

    void Start()
    {
        dialogueBox.transform.localScale = Vector3.zero;
        gamePlanCanvas.SetActive(true);
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
            solidBeaker.GetComponent<BoxCollider>().enabled = true; solidBeaker.GetComponent<OutlineBehaviour>().enabled = true;
        });
        lets_Begin.Play();
    }

    public void SolidStateComplete()
    {
        BoyDialogueBehaviour.Instance.isOpen = false;
        BoyDialogueBehaviour.Instance.OpenDialogueBox();
        typewriter.TypeText("Notice how they are packed closely and only vibrate. That’s why solids have a fixed shape and volume.", 15f);
        notice.Play();
        Invoke(nameof(LetStartLiqState), 7f);
    }

    public void LetStartLiqState()
    {
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

        Invoke(nameof(LetStartGasState), 9f);
    }

    public void LetStartGasState()
    {
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
        Invoke(nameof(ReturnOrbitTarget), 7f);
    }

    public void ReturnOrbitTarget()
    {
        anim.Play(anim_Name);
    }
}
