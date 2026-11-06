using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Clips")]
    public AudioClip mainMenuMusic;   // Scene 0
    public AudioClip gamePickMusic;   // Scene 1
    public AudioClip snakeMusic;      // Scene 2
    public AudioClip racingMusic;     // Scene 3
    public AudioClip tycoonMusic;     // Scene 4

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = 0.5f;

            SceneManager.sceneLoaded += OnSceneLoaded; // listen for scene change
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.buildIndex)
        {
            case 0: PlayMusic(mainMenuMusic); break;
            case 1: PlayMusic(gamePickMusic); break;
            case 2: PlayMusic(snakeMusic); break;
            case 3: PlayMusic(racingMusic); break;
            case 4: PlayMusic(tycoonMusic); break;
            default: break;
        }
    }

    void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource.clip == clip && audioSource.isPlaying) return; // don’t restart if already same clip

        audioSource.clip = clip;
        audioSource.Play();
    }
}
