using UnityEngine;

public class PlateClick : MonoBehaviour
{
    void OnEnable()
    {
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<OutlineBehaviour>().enabled = true;

    }

    void OnMouseUpAsButton()
    {

        GetComponent<BoxCollider>().enabled = false;
        GetComponent<OutlineBehaviour>().enabled = false;
        DoTweenManager.Instance.Condensation();

    }
        
}
