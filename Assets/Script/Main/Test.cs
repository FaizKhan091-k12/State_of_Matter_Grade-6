using UnityEngine;

public class Test : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter");
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit");
    }
}
