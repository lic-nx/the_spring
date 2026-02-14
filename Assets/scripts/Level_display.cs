using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;

public class Level_display : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private string prefix = "Уровень ";

    private void Start()
    {
        var match = Regex.Match(SceneManager.GetActiveScene().name, @"\d+$");
        levelText.text = match.Success
            ? $"{prefix}{match.Value}"
            : $"{prefix}?";
    }
}