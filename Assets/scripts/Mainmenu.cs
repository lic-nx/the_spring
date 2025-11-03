using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class Mainmenu : MonoBehaviour
{
    //public void Start() 
    //{
    //    PlayerPrefs.DeleteAll();
    //    PlayerPrefs.Save();
    //}
    public void PlayGame()
    {
        SceneManager.LoadScene(YG2.saves.CompletedLevel + 2);
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
