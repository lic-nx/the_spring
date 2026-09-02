using UnityEngine;
using System.Collections.Generic;

public class ActionButtonObjectPool : MonoBehaviour
{
    public static ActionButtonObjectPool Instance { get; private set; }

    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private int initialPoolSize = 5;

    private Queue<GameObject> availableButtons = new Queue<GameObject>();
    private List<GameObject> allButtons = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewButton();
        }
    }

    private GameObject CreateNewButton()
    {
        GameObject button = Instantiate(buttonPrefab, transform);
        button.SetActive(false);
        allButtons.Add(button);
        availableButtons.Enqueue(button);
        return button;
    }

    public GameObject GetButton()
    {
        if (availableButtons.Count == 0)
        {
            CreateNewButton();
        }

        GameObject button = availableButtons.Dequeue();
        button.SetActive(true);
        return button;
    }

    public void ReturnButton(GameObject button)
    {
        if (button == null || !allButtons.Contains(button)) return;

        button.SetActive(false);
        availableButtons.Enqueue(button);
    }

    public void ReturnAllButtons()
    {
        foreach (var button in allButtons)
        {
            if (button.activeSelf)
            {
                button.SetActive(false);
                availableButtons.Enqueue(button);
            }
        }
    }
}
