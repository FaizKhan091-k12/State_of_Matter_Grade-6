using UnityEngine;
using UnityEngine.SceneManagement;
public class ResetManager : MonoBehaviour
{
    public MainMenuButtonsBehaviour mainMenuButtonsBehaviour;



    public void ResetCurrentLevel()
    {
        if (mainMenuButtonsBehaviour.buttonclicked[0] == true)
        {
            mainMenuButtonsBehaviour.IntroductionEvent.Invoke();
        }
        else if (mainMenuButtonsBehaviour.buttonclicked[1] == true)
        {
            mainMenuButtonsBehaviour.CompareEvent.Invoke();
        }
        else if (mainMenuButtonsBehaviour.buttonclicked[2] == true)
        {
            mainMenuButtonsBehaviour.StagesEvent.Invoke();
        }
        else if (mainMenuButtonsBehaviour.buttonclicked[3] == true)
        {
            mainMenuButtonsBehaviour.QuizEvent.Invoke();
        }
    }

}
