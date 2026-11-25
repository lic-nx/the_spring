using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelSkript : MonoBehaviour
{ 
    public GameObject ShopPanel;
    public GameObject SeedPanel;
    public GameObject PotPanel;

    [Header("Кнопки вкладок")]
    public Image SeedButtonImage;
    public Image PotButtonImage;

    [Header("Спрайты вкладок")]
    public Sprite DefaultTabSprite;
    public Sprite SelectedTabSprite;
    
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
        SelectTab(SeedButtonImage, PotButtonImage);
    }

    public void PotButtonPressed()
    {
        SeedPanel.SetActive(false);
        PotPanel.SetActive(true);
        SelectTab(PotButtonImage, SeedButtonImage);
    }

    private void SelectTab(Image selectedButton, Image otherButton)
    {
        if (selectedButton != null) selectedButton.sprite = SelectedTabSprite;
        if (otherButton != null) otherButton.sprite = DefaultTabSprite;
    }

}
