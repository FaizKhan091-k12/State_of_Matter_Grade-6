using UnityEngine;

public class DialerClickable : MonoBehaviour
{
    public static DialerClickable Instance;
    public int clickCount;
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

            Invoke(nameof(ClickCount2), 4f);
        }
        if (clickCount == 2)
        {
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<OutlineBehaviour>().enabled = false;
            DoTweenManager.Instance.Boiling();
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
