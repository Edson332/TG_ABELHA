// Scripts/GameSystems/TutorialManager.cs
using System; // Adicionado para usar Action
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instancia { get; private set; }

    [Header("Referências da UI")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialTextElement;
    public Button continueButton;

    // A fila agora armazena o tutorial E uma ação de callback opcional
    private Queue<(TutorialStepSO step, Action onCompleted)> _tutorialQueue = new Queue<(TutorialStepSO, Action)>();
    private HashSet<string> _completedTutorials = new HashSet<string>();

    // O passo atual também armazena seu callback
    private (TutorialStepSO step, Action onCompleted) _currentTutorialInfo;

    private bool _isShowingTutorial = false;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        // TODO: Carregar _completedTutorials do sistema de save do jogo
    }

    void Start()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinuePressed);
    }

    /// <summary>
    /// Método principal para solicitar a exibição de um tutorial.
    /// Agora aceita uma ação de callback opcional a ser executada na conclusão.
    /// </summary>
    public void RequestTutorial(TutorialStepSO tutorialStep, Action onCompletedCallback = null)
    {
        if (tutorialStep == null) return;

        if (tutorialStep.showOnlyOnce && _completedTutorials.Contains(tutorialStep.tutorialID))
        {
            // Se o tutorial já foi feito, mas há um callback, executa o callback imediatamente.
            // Útil para garantir que o fluxo do jogo continue.
            onCompletedCallback?.Invoke();
            return;
        }

        // Adiciona à fila
        _tutorialQueue.Enqueue((tutorialStep, onCompletedCallback));
        Debug.Log($"Tutorial '{tutorialStep.tutorialID}' adicionado à fila.");

        if (!_isShowingTutorial)
        {
            ShowNextTutorialInQueue();
        }
    }

    private void ShowNextTutorialInQueue()
    {
        if (_tutorialQueue.Count == 0)
        {
            _isShowingTutorial = false;
            return;
        }

        _currentTutorialInfo = _tutorialQueue.Dequeue();
        _isShowingTutorial = true;

        if (tutorialTextElement != null) tutorialTextElement.text = _currentTutorialInfo.step.tutorialText;
        if (tutorialPanel != null) tutorialPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    private void OnContinuePressed()
    {
        if (!_isShowingTutorial) return;

        // Marca o tutorial atual como concluído
        if (_currentTutorialInfo.step != null)
        {
            if (_currentTutorialInfo.step.showOnlyOnce && !_completedTutorials.Contains(_currentTutorialInfo.step.tutorialID))
            {
                _completedTutorials.Add(_currentTutorialInfo.step.tutorialID);
                Debug.Log($"Tutorial '{_currentTutorialInfo.step.tutorialID}' marcado como concluído.");
                // TODO: Salvar o _completedTutorials
            }

            // --- A MUDANÇA IMPORTANTE ---
            // Executa a ação de callback que foi passada, se houver uma.
            _currentTutorialInfo.onCompleted?.Invoke();
        }

        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        _currentTutorialInfo = (null, null);

        Time.timeScale = 1f;

        if (_tutorialQueue.Count > 0)
        {
            ShowNextTutorialInQueue();
        }
        else
        {
            _isShowingTutorial = false;
        }
    }

    public bool HasCompletedTutorial(string tutorialID)
    {
        if (string.IsNullOrEmpty(tutorialID)) return false;
        return _completedTutorials.Contains(tutorialID);
    }
    
    public HashSet<string> GetCompletedTutorials()
    {
        return _completedTutorials;
    }

    /// <summary>
    /// Carrega um conjunto de IDs de tutoriais concluídos a partir de uma lista.
    /// </summary>
    public void LoadCompletedTutorials(List<string> completedIDs)
    {
        if (completedIDs == null)
        {
            _completedTutorials = new HashSet<string>();
            return;
        }
        // Converte a lista carregada (que veio do JSON) de volta para um HashSet
        _completedTutorials = new HashSet<string>(completedIDs);
        Debug.Log($"{_completedTutorials.Count} tutoriais concluídos foram carregados.");
    }
}