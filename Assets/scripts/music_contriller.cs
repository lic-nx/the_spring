using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MusicController : MonoBehaviour
{
    [SerializeField] private Toggle musicToggle; // UI Toggle ������ ������

    private bool isEnabled = true;
    private AudioSource[] musicSources;

    private static readonly string MUSIC_ENABLED_KEY = "MusicEnabled"; // ���� � PlayerPrefs

    void Start()
    {
        // ��������� ���������� ��������� (�� ��������� � ��������)
        isEnabled = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1) == 1;

        // ������� ��� ��������� ������ �� �����
        FindMusicSources();

        // ��������� ������� ��������� (��������������� ��� �����)
        ApplyMusicState();

        // ����������� UI Toggle
        if (musicToggle != null)
        {
            musicToggle.isOn = isEnabled; // ������������� ��������� ��������
            musicToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    // ����� ���� �������� � ����� "Music" � ��������� �� AudioSource
    private void FindMusicSources()
    {
        GameObject[] musicObjects = GameObject.FindGameObjectsWithTag("Music");
        musicSources = new AudioSource[musicObjects.Length];

        for (int i = 0; i < musicObjects.Length; i++)
        {
            musicSources[i] = musicObjects[i].GetComponent<AudioSource>();
            if (musicSources[i] == null)
            {
                Debug.LogWarning($"������ {musicObjects[i].name} ����� ��� 'Music', �� �� ����� ���������� AudioSource.");
            }
        }
    }

    // ��������� ��������� ������: �������� ��� ��������� �� �����
    private void ApplyMusicState()
    {
        if (!isEnabled)
        {
            ResumeAll();
        }
        else
        {
            PauseAll();
        }
    }

    // ���������� ��������� ��������� Toggle
    private void OnToggleValueChanged(bool isOn)
    {
        isEnabled = isOn;

        if (!isEnabled)
        {
            ResumeAll();
        }
        else
        {
            PauseAll();
        }

        // ��������� ����� ���������
        PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, isEnabled ? 1 : 0);
        PlayerPrefs.Save(); // ������� ����������
    }

    // ���������� ��������������� ���� ����������
    private void ResumeAll()
    {
        foreach (AudioSource source in musicSources)
        {
            if (source != null && !source.isPlaying)
            {
                source.UnPause();
            }
        }
    }

    // ��������� �� ����� ��� ���������
    private void PauseAll()
    {
        foreach (AudioSource source in musicSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Pause();
            }
        }
    }
}