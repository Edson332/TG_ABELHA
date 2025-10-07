// Scripts/Quiz/QuizQuestionSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NovaPerguntaQuiz", menuName = "Idle Bee Game/Quiz Question")]
public class QuizQuestionSO : ScriptableObject
{
    [Header("Conteúdo da Pergunta")]
    [TextArea(3, 5)]
    public string questionText;

    [Tooltip("Lista de 3 respostas possíveis.")]
    public string[] answers = new string[3];

    [Tooltip("O índice (0, 1 ou 2) da resposta correta na lista acima.")]
    [Range(0, 2)]
    public int correctAnswerIndex;
}