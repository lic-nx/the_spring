using UnityEngine;
using UnityEngine.UI;

public class FlowerRemovalManager : MonoBehaviour
{
    public static FlowerRemovalManager Instance { get; private set; }

    [Header("UI Элементы")]
    [SerializeField] private GameObject confirmationPanel; // Панель с вопросом "Вы уверены?"
    [SerializeField] private Button confirmButton;         // Кнопка "Да, удалить"
    [SerializeField] private Button cancelButton;          // Кнопка "Нет, отмена"

    private Pot pendingPot; // Горшок, который ожидает подтверждения удаления

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);

        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Вызывается лопаткой, чтобы показать окно подтверждения для конкретного горшка
    /// </summary>
    public void ShowConfirmation(Pot targetPot)
    {
        if (targetPot == null || targetPot.CurrentFlower == null) return;

        pendingPot = targetPot;
        
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(true);
        }
        
        // Ставим игру на паузу, чтобы игрок не мог кликать по другим объектам или двигать что-то еще
        Time.timeScale = 0f; 
    }

    private void OnConfirm()
    {
        if (pendingPot != null)
        {
            pendingPot.RemoveFlower();
        }
        ClosePanel();
    }

    private void OnCancel()
    {
        ClosePanel();
    }

    private void ClosePanel()
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
        
        pendingPot = null;
        Time.timeScale = 1f; // Возвращаем нормальное течение времени
    }

    private void OnDestroy()
    {
        // Гарантируем, что время вернется в норму, если объект будет уничтожен
        Time.timeScale = 1f;
    }
}