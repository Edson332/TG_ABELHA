using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Achievement
{
    public string title;
    public string description;
    public Sprite icon;
    public bool isUnlocked;
    // public float progress; // 0-1 for progress bars (opcional)
}

public class AchievementManager : MonoBehaviour
{
    public Achievement[] achievements;
    
    /// <summary>
    /// Desbloqueia o achievement no índice especificado, se ainda não estiver desbloqueado.
    /// </summary>
    public void UnlockAchievement(int index)
    {
        if (index >= 0 && index < achievements.Length)
        {
            // Se ainda não estiver desbloqueado, marca como desbloqueado e dispara uma notificação (por exemplo, log ou animação)
            if (!achievements[index].isUnlocked)
            {
                achievements[index].isUnlocked = true;
                Debug.Log("Achievement desbloqueado: " + achievements[index].title);
                // Aqui você pode disparar uma notificação na UI, salvar o progresso, etc.
            }
        }
    }

    /// <summary>
    /// Método para checar as condições de desbloqueio dos achievements.
    /// Esse método pode ser chamado a partir de eventos do jogo ou periodicamente.
    /// </summary>
    public void CheckAchievements()
    {
        // Exemplo: se houver pelo menos 2 unidades de Mel Processado, desbloqueia o achievement de índice 1.
        if (GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Mel) >= 2)
        {
            UnlockAchievement(0);
        }
        
         //Você pode adicionar outras condições, por exemplo:
         if (GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Nectar) >= 5)
         {
             UnlockAchievement(1);
         }
    }

    // Se você realmente precisar checar no Update, chame o método CheckAchievements,
    // mas considere disparar essa verificação a partir de eventos específicos.
    void Update()
    {
        CheckAchievements();
    }
}
