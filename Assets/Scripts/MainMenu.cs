using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SnakeGame()
    {
        SceneManager.LoadSceneAsync(2);
    }

    public void RacingGame()
    {
        SceneManager.LoadSceneAsync(3);
    }

    public void TycoonGame()
    {
        SceneManager.LoadSceneAsync(4);
    }
    
     public void BackGame()
    {
        SceneManager.LoadSceneAsync(0);
    }



    
}
