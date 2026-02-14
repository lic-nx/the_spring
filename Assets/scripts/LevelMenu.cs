using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using YG;
using UnityEngine.Rendering.PostProcessing;

public class LevelMenu : MonoBehaviour
{
    public Button[] buttons;
    public Image Complite;
    public Image Lock;         // Объект с компонентом Image
    public Image Unlock;
    public GameObject levelButtons;
    public GameObject openLevel;
    private int highesLevel; // максимально открытый уровень у игрока

    private void Awake()
    {
        ButtonsToArray();
        int unlockedLevel = YG2.saves.UnlockedLevel;
        int CompletedLevel = YG2.saves.CompletedLevel;
        Debug.Log($"unlockedLevel {unlockedLevel}");
        Debug.Log($"CompletedLevel {CompletedLevel}");
        Debug.Log($"buttons.Length {buttons.Length}");
        for (int i = unlockedLevel; i < buttons.Length ; i++)
        {
            buttons[i].enabled = false;
            buttons[i].GetComponent<Image>().sprite = Lock.sprite; // ✅ Правильное обращение к спрайту
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }

        for (int i = CompletedLevel; i < unlockedLevel && i < buttons.Length; i++)
        {
            buttons[i].enabled = true;
            buttons[i].GetComponent<Image>().sprite = Unlock.sprite; // ✅ Здесь тоже
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "" + (i + 1);
            if (i == CompletedLevel && openLevel != null)
            {
                Transform buttonParent = buttons[i].transform.parent;
                GameObject instance = Instantiate(openLevel, buttonParent);

                // Устанавливаем мировую позицию напрямую
                instance.transform.position = buttons[i].transform.position;

                // Остальное без изменений...
                instance.transform.SetSiblingIndex(Mathf.Max(0, buttons[i].transform.GetSiblingIndex() - 1));
                Image bg = instance.GetComponent<Image>();
                if (bg != null) bg.raycastTarget = false;
            }
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
    public void OpenLevelWitTutorial(int levelId)
    {
        // если у уровня есть туториал то вызываем этот метод
        string levelName = "tutorial_" + levelId;
        SceneManager.LoadScene(levelName);
    }

    public void OpenLevel(int levelId)
    {
        string levelName = "level_" + levelId;        
        SceneManager.LoadScene(levelName);
    }

    public void Restart_button()
    {
        YG2.saves.UnlockedLevel = 1;     // Исправлена опечатка!
        YG2.saves.CompletedLevel = 0;
        YG2.saves.ReachedIndex = 0;
        YG2.SaveProgress();                      // Сохраняем в YG
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Обновляем UI
    }
}