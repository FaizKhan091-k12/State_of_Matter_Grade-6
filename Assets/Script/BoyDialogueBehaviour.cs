using DG.Tweening;
using UnityEngine;

public class BoyDialogueBehaviour : MonoBehaviour
{
    public static BoyDialogueBehaviour Instance;
    public bool isOpen;
    public Transform dialogueBox;

    void Awake()
    {
        Instance = this;
    }
    public void OpenDialogueBox()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            dialogueBox.localScale = Vector3.zero;
            dialogueBox.DOScale(Vector2.one, .2f).SetEase(Ease.InOutFlash);
        }
        else
        {
            dialogueBox.localScale = Vector3.one;
            dialogueBox.DOScale(Vector2.zero, .2f).SetEase(Ease.InOutFlash);
        }
    }
}
