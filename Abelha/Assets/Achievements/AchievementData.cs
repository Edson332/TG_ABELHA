using UnityEngine;

[CreateAssetMenu(fileName = "NewAchievement", menuName = "Achievement System/Achievement")]
public class AchievementData : ScriptableObject
{
    public string title;           // Nome da conquista
    [TextArea]
    public string description;     // Descrição da conquista
    public int requiredHoney;      // Quantidade de mel necessária para desbloquear a conquista
    public bool unlocked;          // Indica se a conquista já foi desbloqueada

    public void Unlock()
    {
        unlocked = true;
    }
}
