using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelMenu : MonoBehaviour
{
    public Button[] buttons;
    public Image Complite;
    public Image Lock;         // Объект с компонентом Image
    public Image Unlock;
    public GameObject levelButtons;
    private int highesLevel; // максимально открытый уровень у игрока

    private void Awake()
    {
        ButtonsToArray();
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        int CompletedLevel = PlayerPrefs.GetInt("CompletedLevel", 0);
        Debug.Log($"unlockedLevel {unlockedLevel}");
        Debug.Log($"CompletedLevel {CompletedLevel}");
        Debug.Log($"buttons.Length {buttons.Length}");
        for (int i = unlockedLevel; i < buttons.Length; i++)
        {
            buttons[i].enabled = false;
            buttons[i].GetComponent<Image>().sprite = Lock.sprite; // ✅ Правильное обращение к спрайту
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }

        for (int i = CompletedLevel; i < unlockedLevel; i++)
        {
            buttons[i].enabled = true;
            buttons[i].GetComponent<Image>().sprite = Unlock.sprite; // ✅ Здесь тоже
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "" + (i + 1);
        }
        for (int i = 0; i < CompletedLevel; i++)
        {
            buttons[i].enabled = true;
            buttons[i].GetComponent<Image>().sprite = Complite.sprite; // ✅ Здесь тоже
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "" + (i + 1);
        }
    }

    void ButtonsToArray()
    {
        int childCount = levelButtons.transform.childCount;
        buttons = new Button[childCount];
        for (int i = 0; i < childCount; i++)
        {
            buttons[i] = levelButtons.transform.GetChild(i).GetComponent<Button>();
        }
    }

    public void OpenLevel(int levelId)
    {
        string levelName = "level" + levelId;
        SceneManager.LoadScene(levelName);
    }
}