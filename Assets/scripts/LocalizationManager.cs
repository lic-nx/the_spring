using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    private static LocalizationManager _instance;

    public static LocalizationManager Instance
{
    get
    {
        if (isQuitting)
            return null;

        if (_instance == null)
        {
            _instance = FindObjectOfType<LocalizationManager>();

            if (_instance == null)
            {
                GameObject obj = new GameObject("LocalizationManager");
                _instance = obj.AddComponent<LocalizationManager>();
            }
        }
        return _instance;
    }
}

    private string currentLang;
    public System.Action OnLanguageChanged;

    private static bool isQuitting = false;

    private Dictionary<string, string> ru = new Dictionary<string, string>()
    {
        {"play", "Играть"},
        {"settings", "Настройки" },
        {"level", "Уровень {0}" },
        {"levels", "Уровни"},
        {"help", "Помоги <color=#EE5B71>ЦВЕТКУ</color> добрать до <color=#FFE177>СОЛНЦА</color>"},
        {"t1", "Нажимай на треснувшую почву, пока она не сломается"},
        {"t2_1", "Зеленый листик можно двигать"},
        {"t2_2", "Зажми его и потяни"},
        {"t3_1", "<color=#84EE54>ГУСЕНИЦА</color> поедает растения на своем пути"},
        {"t3_2", "Постарайся с ней не сталкиваться"},
        {"loose", "Уровень {0} \nне пройден"},
        {"win", "Уровень {0} \nпройден!"},
        {"thanks", "Спасибо \nза прохождение \nвсех уровней!"},
        {"build", "Мы уже \nстроим новые"},
    };

    private Dictionary<string, string> en = new Dictionary<string, string>()
    {
        {"play", "Play"},
        {"settings", "Settings" },
        {"level", "Level {0}" },
        {"levels", "Levels"},
        {"help", "Help the <color=#EE5B71>FLOWER</color> reach the <color=#FFE177>SUN</color>" },
        {"sun", ""},
        {"t1", "Tap the cracked soil until it breaks"},
        {"t2_1", "The green leaf can be moved"},
        {"t2_2", "Press and drag it"},
        {"t3_1", "The <color=#A4EC75>CATERPILLAR</color> eats plants in its path"},
        {"t3_2", "Try to avoid it"},
        {"loose", "Level {0} \nfailed"},
        {"win", "Level {0} \ncompleted!"},
        {"thanks", "Thank you \nfor completing \nall the levels!"},
        {"build", "We’re already \nbuilding new ones"},
    };

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (PlayerPrefs.HasKey("lang"))
            currentLang = PlayerPrefs.GetString("lang");
        else
            DetectLanguage();
    }
    


    void DetectLanguage()
    {
        if (Application.systemLanguage == SystemLanguage.Russian)
            currentLang = "ru";
        else
            currentLang = "en";
    }

    public string GetText(string key)
    {
        if (currentLang == "ru" && ru.ContainsKey(key))
            return ru[key];

        if (currentLang == "en" && en.ContainsKey(key))
            return en[key];

        return key;
    }

    public void SetLanguage(string lang)
    {
        currentLang = lang;
        PlayerPrefs.SetString("lang", currentLang);
        OnLanguageChanged?.Invoke();
    }

    public string GetText(string key, params object[] args)
    {
        string value = GetText(key);
        return string.Format(value, args);
    }

    void OnApplicationQuit()
{
    isQuitting = true;
}

}
