using UnityEngine;
using UnityEngine.Events;

public class BeakerClick : MonoBehaviour
{
    /// <summary>
    /// OnMouseUpAsButton is only called when the mouse is released over
    /// the same GUIElement or Collider as it was pressed.
    /// </summary>
   public UnityEvent OnClickEvent;
    public Animation anim;
    public string anim_Clip;
    void OnMouseUpAsButton()
    {
        OnClickEvent.Invoke();
        gameObject.transform.localScale = Vector3.zero;
        anim.Play(anim_Clip);
     
    }
   
}
