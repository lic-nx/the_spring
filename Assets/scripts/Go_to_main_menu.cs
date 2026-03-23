using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class Go_to_main_menu : Button_sound_controller
{
    public AudioClip restartSound; // Назначьте звук в инспекторе

    public void Mainmenu()
    {
        if (YG2.saves.EffectMusicEnabled)
            PlayMusicWithDelay(
                restartSound,
                restartSound.length + 0.1f,
                () => SceneManager.LoadScene(0),
                volume: 0.5f 
            );
        else SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }
}