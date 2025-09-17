// Scripts/Managers/AudioManager.cs
using UnityEngine;
using System.Collections.Generic; // Para usar Dicionários
using System.Collections; // Para usar Coroutines

// Uma classe auxiliar para organizar os clipes de áudio no Inspector
[System.Serializable]
public class Sound
{
    public string name; // O nome que usaremos para chamar o som (ex: "MainBGM", "ButtonClick")
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instancia { get; private set; }

    [Header("Biblioteca de Sons")]
    [Tooltip("Lista de todas as músicas de fundo do jogo.")]
    public Sound[] musicTracks;
    [Tooltip("Lista de todos os efeitos sonoros do jogo.")]
    public Sound[] sfxClips;

    [Header("Controle de Volume Máximo")]
    [Range(0f, 1f)]
    public float maxMusicVolume = 0.7f;

    private AudioSource _musicSource;
    private AudioSource _sfxSource;
    private Dictionary<string, AudioClip> _musicDictionary;
    private Dictionary<string, AudioClip> _sfxDictionary;
    private Coroutine _musicFadeCoroutine;

    private const string MUSIC_VOLUME_KEY = "MusicVolumePercentage";
    private const string SFX_VOLUME_KEY = "SfxVolume";

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

        _musicSource = gameObject.AddComponent<AudioSource>();
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;

        // Preenche os dicionários para acesso rápido aos clipes por nome
        _musicDictionary = new Dictionary<string, AudioClip>();
        foreach (var sound in musicTracks) { _musicDictionary[sound.name] = sound.clip; }

        _sfxDictionary = new Dictionary<string, AudioClip>();
        foreach (var sound in sfxClips) { _sfxDictionary[sound.name] = sound.clip; }
        
        LoadVolumeSettings();
    }

    void Start()
    {
        // Toca a música de fundo principal ao iniciar o jogo
        PlayMusic("MainBGM");
    }

    /// <summary>
    /// Toca uma música pelo seu nome, com um fade suave.
    /// </summary>
    public void PlayMusic(string musicName, float fadeDuration = 1.0f)
    {
        if (_musicDictionary.TryGetValue(musicName, out AudioClip clipToPlay))
        {
            if (_musicSource.isPlaying && _musicSource.clip == clipToPlay) return; // Já está tocando

            if (_musicFadeCoroutine != null) StopCoroutine(_musicFadeCoroutine);
            _musicFadeCoroutine = StartCoroutine(FadeAndPlayMusic(clipToPlay, fadeDuration));
        }
        else
        {
            Debug.LogWarning($"Música com o nome '{musicName}' não encontrada na biblioteca.");
        }
    }

    private IEnumerator FadeAndPlayMusic(AudioClip newClip, float duration)
    {
        // Fade out
        float startVolume = _musicSource.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            _musicSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }
        _musicSource.Stop();
        
        // Troca o clipe e faz o fade in
        _musicSource.clip = newClip;
        _musicSource.Play();
        
        float targetVolumePercentage = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
        float finalVolume = targetVolumePercentage * maxMusicVolume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            _musicSource.volume = Mathf.Lerp(0f, finalVolume, t / duration);
            yield return null;
        }
        _musicSource.volume = finalVolume;
    }


    /// <summary>
    /// Toca um efeito sonoro pelo seu nome.
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        if (_sfxDictionary.TryGetValue(sfxName, out AudioClip clipToPlay))
        {
            _sfxSource.PlayOneShot(clipToPlay);
        }
        else
        {
            Debug.LogWarning($"Efeito sonoro com o nome '{sfxName}' não encontrado na biblioteca.");
        }
    }

    public void SetMusicVolume(float volumePercentage)
    {
        volumePercentage = Mathf.Clamp01(volumePercentage);
        _musicSource.volume = volumePercentage * maxMusicVolume;
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volumePercentage);
    }

    public void SetSfxVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        _sfxSource.volume = volume;
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
    }

    private void LoadVolumeSettings()
    {
        float musicVolumePercentage = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
        _musicSource.volume = musicVolumePercentage * maxMusicVolume;
        _sfxSource.volume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.75f);
    }
    
    public float GetMusicVolumePercentage()
    {
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
    }
}