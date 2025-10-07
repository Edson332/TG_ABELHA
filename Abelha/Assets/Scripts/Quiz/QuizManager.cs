// Scripts/Quiz/QuizManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Para usar Action

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instancia { get; private set; }

    [Header("Referências da UI")]
    public GameObject quizPanel;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons = new Button[3];
    public TextMeshProUGUI[] answerTexts = new TextMeshProUGUI[3];

    private Action<bool> _onQuizCompleted; // O callback para notificar o resultado

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        quizPanel.SetActive(false);
    }

    /// <summary>
    /// Inicia o quiz com uma pergunta e uma ação de callback.
    /// </summary>
    public void StartQuiz(QuizQuestionSO question, Action<bool> onCompletedCallback)
    {
        _onQuizCompleted = onCompletedCallback;

        questionText.text = question.questionText;
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerTexts[i].text = question.answers[i];
            int buttonIndex = i; // Essencial para o closure do listener
            
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => AnswerSelected(buttonIndex == question.correctAnswerIndex));
        }
        
        Time.timeScale = 0f; // Pausa o jogo
        quizPanel.SetActive(true);
    }

    private void AnswerSelected(bool wasCorrect)
    {
        Debug.Log($"Resposta foi: {(wasCorrect ? "Correta" : "Incorreta")}");
        quizPanel.SetActive(false);
        Time.timeScale = 1f; // Despausa o jogo

        // Chama o callback, informando se a resposta foi correta
        _onQuizCompleted?.Invoke(wasCorrect);
    }
}