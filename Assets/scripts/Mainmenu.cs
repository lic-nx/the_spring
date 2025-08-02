using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    //public void Start() 
    //{
    //    PlayerPrefs.DeleteAll();
    //    PlayerPrefs.Save();
    //}
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }

    public void ChooseLevel()
    {
        SceneManager.LoadScene("levelSelection");
    }

    public void ExitGame()
    {
        Debug.Log("игра закрыта");
        Application.Quit();
    }
}
