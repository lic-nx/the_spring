using UnityEngine;
using YG;

public class button_pushsound : MonoBehaviour
{
    [SerializeField] private AudioClip soundClip;
    private AudioSource audioSource; // ← Ссылка на компонент

    private void Awake()
    {
        // Получаем AudioSource с этого же объекта
        audioSource = GetComponent<AudioSource>();

        // Если компонента нет — создаём его программно
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Настройки для 2D звука (чтобы громкость не зависела от расстояния)
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
    }

    public void ButtonPush()
    {
        Debug.Log($"звук играет");
        if (!YG2.saves.EffectMusicEnabled || soundClip == null) return;

        float volume = Mathf.Clamp01(0.5f);

        // ✅ Вызываем метод у экземпляра (audioSource), а не у класса
        audioSource.PlayOneShot(soundClip, volume);
        Debug.Log($"конец звука");
    }
}