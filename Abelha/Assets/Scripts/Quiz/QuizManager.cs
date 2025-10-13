// Scripts/Quiz/QuizManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections; // Necessário para Coroutines

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instancia { get; private set; }

    [Header("Referências da UI")]
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons = new Button[3];
    public TextMeshProUGUI[] answerTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI resultText; 

    [Header("Feedback Visual")]
    public Color correctColor = new Color(0.2f, 0.8f, 0.2f); // Verde
    public Color incorrectColor = new Color(0.9f, 0.2f, 0.2f); // Vermelho
    public Color neutralColor = Color.white;
    public float feedbackDelay = 2.0f; // Tempo que o feedback fica na tela

    private Action<bool> _onQuizCompleted;
    private QuizQuestionSO _currentQuestion;

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        if(quizPanel != null) quizPanel.SetActive(false);
    }

    /// <summary>
    /// Inicia o quiz com uma pergunta e uma ação de callback.
    /// </summary>
    public void StartQuiz(QuizQuestionSO question, Action<bool> onCompletedCallback)
    {
        _onQuizCompleted = onCompletedCallback;
        _currentQuestion = question;

        // Reseta o estado da UI
        resultText.gameObject.SetActive(false);
        questionText.text = question.questionText;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].interactable = true; // Garante que os botões estejam clicáveis
            answerButtons[i].GetComponent<Image>().color = neutralColor; // Reseta a cor
            answerTexts[i].text = question.answers[i];
            
            int buttonIndex = i;
            
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(buttonIndex));
        }
        
        Time.timeScale = 0f; // Pausa o jogo
        quizPanel.SetActive(true);
    }

    /// <summary>
    /// Chamado quando um dos botões de resposta é clicado.
    /// </summary>
    private void OnAnswerSelected(int selectedIndex)
    {
        // Desativa todos os botões para impedir cliques múltiplos
        foreach (var button in answerButtons)
        {
            button.interactable = false;
        }

        bool wasCorrect = selectedIndex == _currentQuestion.correctAnswerIndex;

        // Inicia a coroutine que mostra o feedback e finaliza o quiz
        StartCoroutine(ShowFeedbackSequence(wasCorrect, selectedIndex));
    }

    /// <summary>
    /// Coroutine para mostrar o feedback visual e depois fechar o quiz.
    /// </summary>
    private IEnumerator ShowFeedbackSequence(bool wasCorrect, int selectedIndex)
    {
        // Mostra o feedback de cor nos botões
        answerButtons[_currentQuestion.correctAnswerIndex].GetComponent<Image>().color = correctColor;
        if (!wasCorrect)
        {
            answerButtons[selectedIndex].GetComponent<Image>().color = incorrectColor;
        }
        
        // Mostra o texto de resultado
        if (wasCorrect)
        {
            resultText.text = "Correto!";
            resultText.color = correctColor;
        }
        else
        {
            resultText.text = "Incorreto...";
            resultText.color = incorrectColor;
        }
        resultText.gameObject.SetActive(true);

        // Espera um tempo para o jogador ver o resultado
        // Usamos WaitForSecondsRealtime porque o Time.timeScale está em 0
        yield return new WaitForSecondsRealtime(feedbackDelay);

        // Finaliza o quiz
        quizPanel.SetActive(false);
        Time.timeScale = 1f; // Despausa o jogo

        // Chama o callback, informando se a resposta foi correta
        _onQuizCompleted?.Invoke(wasCorrect);
    }
}