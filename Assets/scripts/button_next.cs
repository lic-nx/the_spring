using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
public class button_next : MonoBehaviour
{
    public void NextLevel()
    {
        int nextIndex = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings;
        SceneManager.LoadScene(nextIndex);
        Time.timeScale = 1f;
    }
}
