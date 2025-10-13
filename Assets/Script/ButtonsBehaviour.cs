using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
public class ButtonsBehaviour : MonoBehaviour
{
    public UIPanelPivotLerp uIPanelPivotLerp;
    [SerializeField] Button solid, liquid, gas;
    [SerializeField] GameObject[] threeHighlighted;

    public UnityEvent SolidEvent, LiquidEvent, GasEvent;

    void Start()
    {
   liquid.transform.localScale = Vector3.zero;
  solid.transform.localScale = Vector3.zero;
  gas.transform.localScale = Vector3.zero;
        solid.onClick.AddListener(delegate
        {
            foreach (var item in threeHighlighted)
            {
                item.SetActive(false);
                threeHighlighted[0].SetActive(true);
            }
            SolidEvent.Invoke();
        });

        liquid.onClick.AddListener(delegate
               {

                   foreach (var item in threeHighlighted)
                   {
                       item.SetActive(false);
                       threeHighlighted[1].SetActive(true);
                   }
                   LiquidEvent.Invoke();
               });


        gas.onClick.AddListener(delegate
        {
            foreach (var item in threeHighlighted)
            {
                item.SetActive(false);
                threeHighlighted[2].SetActive(true);
            }
            GasEvent.Invoke();
        });

    }
    public void ButtonPOP()
    {
        Invoke(nameof(SolidPOP), 1f);
        Invoke(nameof(TogglePanel), 1f);
    }

    private  void SolidPOP()
    {
        solid.transform.localScale = Vector3.zero;
        solid.transform.DOScale(Vector3.one, .2f).SetEase(Ease.OutFlash);
        Invoke(nameof(LiquidPOP), .1f);
    }

    private void LiquidPOP()
    {
        liquid.transform.localScale = Vector3.zero;
        liquid.transform.DOScale(Vector3.one, .2f).SetEase(Ease.OutFlash);
        Invoke(nameof(GasPOP), .1f);
    }


    private void GasPOP()
    {
        gas.transform.localScale = Vector3.zero;
        gas.transform.DOScale(Vector3.one, .2f).SetEase(Ease.OutFlash);
    }
    public void TogglePanel()
    {
        uIPanelPivotLerp.TogglePanel();
    }
}
