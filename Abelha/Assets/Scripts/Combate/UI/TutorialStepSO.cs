// Scripts/GameSystems/TutorialStepSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NovoTutorialStep", menuName = "Idle Bee Game/Tutorial Step")]
public class TutorialStepSO : ScriptableObject
{
    [Header("Identificação do Tutorial")]
    [Tooltip("ID único para este passo do tutorial. Ex: 'PRIMEIRO_MEL', 'COMO_LUTAR'. Usado para não mostrá-lo novamente.")]
    public string tutorialID;

    [Header("Conteúdo do Tutorial")]
    [TextArea(5, 15)] // Faz a caixa de texto no Inspector ser maior e mais fácil de editar
    public string tutorialText;

    [Tooltip("Se marcado, este tutorial só será mostrado uma vez por jogo salvo.")]
    public bool showOnlyOnce = true;
}
