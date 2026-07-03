using UnityEngine;

public class GardenUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject flowerView;
    [SerializeField] private GameObject potView;
    private bool showShop = true;
    private bool showFlowersView = true;
    private bool showPotsView = false;
    [Header("Gameplay Buttons")]
    [SerializeField] private GameObject gameplayButtons;

    public void ShowHideShop()
    {
        shopPanel.SetActive(showShop);
        showShop = !showShop;
    }

    public void ShowView()
    {
        flowerView.SetActive(showFlowersView);
        potView.SetActive(showPotsView);

    }
    public void flowerViewShow()
   {
        showFlowersView = true;
        showPotsView = false;
        ShowView();
   }
        public void potViewShow()
   {
        showFlowersView = false;
        showPotsView = true;
        ShowView();
   }
}