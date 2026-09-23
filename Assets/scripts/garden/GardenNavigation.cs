using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Подключает Garden к общему сценарию навигации без ручной настройки сцены.</summary>
public static class GardenNavigation
{
    private const string GardenSceneName = "Garden";
    private const string MenuSceneName = "menu";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateHomeButton()
    {
        if (SceneManager.GetActiveScene().name != GardenSceneName) return;
        if (GameObject.Find("GardenHomeButton") != null) return;

        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[GardenNavigation] Canvas не найден — кнопка возврата не создана.");
            return;
        }

        var buttonObject = new GameObject(
            "GardenHomeButton",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        buttonObject.layer = canvas.gameObject.layer;
        buttonObject.transform.SetParent(canvas.transform, false);
        buttonObject.transform.SetAsLastSibling();

        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(24f, -24f);
        rect.sizeDelta = new Vector2(96f, 96f);

        var image = buttonObject.GetComponent<Image>();
        image.sprite = Resources.Load<Sprite>("UI/UI_Home");
        image.preserveAspect = true;
        image.color = image.sprite != null ? Color.white : new Color(0.2f, 0.6f, 0.25f, 0.9f);

        buttonObject.GetComponent<Button>().onClick.AddListener(ReturnToMenu);
    }

    private static void ReturnToMenu()
    {
        if (GameSaveManager.Instance != null)
            GameSaveManager.Instance.SaveAllGardenState();

        SceneManager.LoadScene(MenuSceneName);
    }
}
