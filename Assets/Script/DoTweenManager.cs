using DG.Tweening;
using UnityEngine;

public class DoTweenManager : MonoBehaviour
{
    [SerializeField] GameObject gamePlanCanvas;

    public void GamePlanDeactivate()
    {
        gamePlanCanvas.transform.localScale = Vector3.one;
        gamePlanCanvas.transform.DOScale(Vector3.zero, .25f).SetEase(Ease.InBack);
    }
}
