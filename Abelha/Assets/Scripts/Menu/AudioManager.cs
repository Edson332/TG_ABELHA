// Scripts/Managers/AudioManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia { get; private set; }

    [Header("Música de Fundo (BGM)")]
    public AudioClip backgroundMusic;

    // --- NOVA VARIÁVEL ADICIONADA ---
    [Header("Controle de Volume Máximo")]
    [Tooltip("O volume máximo real que a música pode atingir (0.0 a 1.0). O slider do menu será uma porcentagem deste valor.")]
    [Range(0f, 1f)]
    public float maxMusicVolume = 0.7f; // Exemplo: 70% do volume máximo

    private AudioSource _musicSource;
    private AudioSource _sfxSource;

    private const string MUSIC_VOLUME_KEY = "MusicVolumePercentage"; // Renomeado para clareza
    private const string SFX_VOLUME_KEY = "SfxVolume";

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

        _musicSource = gameObject.AddComponent<AudioSource>();
        _sfxSource = gameObject.AddComponent<AudioSource>();

        _musicSource.loop = true;
        _musicSource.playOnAwake = false;

        LoadVolumeSettings();
    }

    void Start()
    {
        PlayMusic(backgroundMusic);
    }
    
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null || (_musicSource.clip == musicClip && _musicSource.isPlaying)) return;
        _musicSource.clip = musicClip;
        _musicSource.Play();
    }
    
    public void PlaySFX(AudioClip sfxClip)
    {
        if (sfxClip == null) return;
        _sfxSource.PlayOneShot(sfxClip);
    }

    /// <summary>
    /// Define o volume da música com base na porcentagem do slider (0.0 a 1.0).
    /// </summary>
    public void SetMusicVolume(float volumePercentage)
    {
        volumePercentage = Mathf.Clamp01(volumePercentage);
        
        // A lógica principal está aqui: o volume real é a porcentagem * o máximo permitido
        _musicSource.volume = volumePercentage * maxMusicVolume;

        // Salva a preferência do jogador (a porcentagem, não o valor final)
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volumePercentage);
    }

    public void SetSfxVolume(float volume)
    {
        // (Lógica para SFX permanece a mesma por enquanto, mas poderia ser adaptada)
        volume = Mathf.Clamp01(volume);
        _sfxSource.volume = volume;
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
    }

    private void LoadVolumeSettings()
    {
        // Carrega a PORCENTAGEM salva, ou usa 1.0 (100%) como padrão se não houver save
        float musicVolumePercentage = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
        
        // Aplica o volume carregado, já calculando com base no máximo permitido
        _musicSource.volume = musicVolumePercentage * maxMusicVolume;

        // Lógica para SFX
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.75f);
        _sfxSource.volume = sfxVolume;
    }
    
    /// <summary>
    /// Retorna a porcentagem de volume salva (0 a 1) para a UI.
    /// </summary>
    public float GetMusicVolumePercentage()
    {
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
    }
}