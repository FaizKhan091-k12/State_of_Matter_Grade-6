using UnityEngine;
using UnityEngine.Events;

public class DialerClickable : MonoBehaviour
{
    public static DialerClickable Instance;
    public int clickCount;
    public UnityEvent clickEvent1,clickEvent2;
    void OnEnable()
    {
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<OutlineBehaviour>().enabled = true;
        
    }

    void Awake()
    {
        Instance = this;
    }
    void OnMouseUpAsButton()
    {
        if (clickCount == 1)
        {
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<OutlineBehaviour>().enabled = false;
            DoTweenManager.Instance.PlayMeltingV2();
            clickEvent1.Invoke();
            Invoke(nameof(ClickCount2), .1f);

        }
        if (clickCount == 2)
        {
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<OutlineBehaviour>().enabled = false;
            DoTweenManager.Instance.Boiling();
            clickEvent2.Invoke();
        }
    }

    public void ClickCount2()
    {
        clickCount = 2;
    }
    public void ClickCount1()
    {
         clickCount = 1;
    }
}
