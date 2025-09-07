using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI; 

public class MainMenuController : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;

    [Header("Controles de Opções")]
    public TMP_Dropdown resolutionDropdown;

    public Slider musicVolumeSlider;

    private Resolution[] _resolutions;
    private List<Resolution> _filteredResolutions;
    private int _currentResolutionIndex = 0;

    void Start()
    {
        // Garante que o painel de opções comece desativado e o principal ativado
        mainMenuPanel.SetActive(true);
        optionsPanel.SetActive(false);

        // --- Lógica de Resolução ATUALIZADA ---
        _resolutions = Screen.resolutions;
        _filteredResolutions = new List<Resolution>();

        resolutionDropdown.ClearOptions();

        // MODIFICADO: Usa a nova propriedade 'refreshRateRatio' que é um struct mais preciso.
        RefreshRate currentRefreshRate = Screen.currentResolution.refreshRateRatio;

        // Filtra para mostrar apenas resoluções com a taxa de atualização do monitor
        for (int i = 0; i < _resolutions.Length; i++)
        {
            // MODIFICADO: Compara os structs 'refreshRateRatio' diretamente.
            if (_resolutions[i].refreshRateRatio.Equals(currentRefreshRate))
            {
                _filteredResolutions.Add(_resolutions[i]);
            }
        }

        // Adiciona as opções de resolução filtradas ao dropdown
        List<string> options = new List<string>();
        for (int i = 0; i < _filteredResolutions.Count; i++)
        {
            string resolutionOption = _filteredResolutions[i].width + " x " + _filteredResolutions[i].height;
            options.Add(resolutionOption);
            // Compara a resolução atual para definir o valor padrão do dropdown
            if (_filteredResolutions[i].width == Screen.width && _filteredResolutions[i].height == Screen.height)
            {
                _currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = _currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        
        if (musicVolumeSlider != null && AudioManager.Instancia != null)
        {
            // MODIFICADO para chamar o novo método
            musicVolumeSlider.value = AudioManager.Instancia.GetMusicVolumePercentage();
            
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        else if (musicVolumeSlider == null)
        {
            Debug.LogWarning("Slider de volume de música não atribuído no MainMenuController.");
        }
    }

    // --- Métodos para os Botões ---
    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.SetMusicVolume(volume);
        }
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("Jogo"); // <<-- CERTIFIQUE-SE DE QUE O NOME DA CENA ESTÁ CORRETO
    }

    public void ShowOptionsPanel()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    // --- Métodos para as Opções ---

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = _filteredResolutions[resolutionIndex];
        // O método SetResolution continua o mesmo e funciona corretamente.
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        Debug.Log($"Resolução alterada para: {resolution.width}x{resolution.height}");
    }
}