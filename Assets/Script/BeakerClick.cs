using UnityEngine;
using UnityEngine.Events;

public class BeakerClick : MonoBehaviour
{
    /// <summary>
    /// OnMouseUpAsButton is only called when the mouse is released over
    /// the same GUIElement or Collider as it was pressed.
    /// </summary>
    public bool one, two, three, oneTime;
    public UnityEvent OnClickEvent;

    void OnMouseUpAsButton()
    {
        OnClickEvent.Invoke();
        gameObject.GetComponent<OutlineBehaviour>().enabled = false;
       // gameObject.transform.localScale = Vector3.zero;

        // if (one)
        // {
   
        //     DoTweenManager.Instance.one = true;
        // }
        // if (two)
        // {

        //     DoTweenManager.Instance.two = true;
        // }
        // if (three)
        // {

        //     DoTweenManager.Instance.three = true;
        // }


    }

   

}
