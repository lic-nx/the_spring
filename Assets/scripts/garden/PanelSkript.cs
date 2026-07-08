using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSkript : MonoBehaviour
{ 
    public GameObject ShopPanel;
    public GameObject SeedPanel;
    public GameObject PotPanel;

    public bool show = false;
    // void Start() {
    //     Debug.Log("payse Camera.main: " + (Camera.main ? Camera.main.name : "null"));
    //     imageComponent = GetComponent<Image>();
    //     level_number.text = "Уровень " + (SceneManager.GetActiveScene().buildIndex - 1);
    // }

    public void ShowHidePanel(){
        ShopPanel.SetActive(show);
        show = !show;
    }

    public void SeedButtonPressed()
    {
        //вызываем остановку
        SeedPanel.SetActive(true);
        PotPanel.SetActive(false);
    }

    public void PotButtonPressed()
    {
        SeedPanel.SetActive(false);
        PotPanel.SetActive(true);
    }

}
