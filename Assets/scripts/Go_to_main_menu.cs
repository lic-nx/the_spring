using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Go_to_main_menu : MonoBehaviour
{
    public void Mainmenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }
}
