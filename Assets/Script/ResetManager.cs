using UnityEngine;
using UnityEngine.SceneManagement;
public class ResetManager : MonoBehaviour
{
   public void RestartSimulation()
    {
        SceneManager.LoadScene(0);
    }
}
