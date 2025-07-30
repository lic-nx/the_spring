using UnityEngine;

public class dont_destroy_music : MonoBehaviour
{
    public static dont_destroy_music instance = null;
    public AudioSource musicSource;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

    }
}
