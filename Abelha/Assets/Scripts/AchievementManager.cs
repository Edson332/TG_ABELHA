using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; 
using System.Linq;

[System.Serializable]
public class Achievement
{
    public string title;
    public string description;
    public Sprite icon;
    public bool isUnlocked;

    

    public bool hasBeenViewed;
    // public float progress; // 0-1 for progress bars (opcional)
}

public class AchievementManager : MonoBehaviour
{

    public static AchievementManager Instancia { get; private set; }
    private void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            // Se já existe uma instância e não sou eu, me destruo.
            Debug.LogWarning("Instância duplicada do AchievementManager encontrada. Destruindo esta.", this.gameObject);
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        // Garante que este objeto não seja destruído ao carregar uma nova cena
        // (essencial se você tiver um menu principal e uma cena de jogo)
        DontDestroyOnLoad(gameObject);
        Debug.Log("AchievementManager inicializado como Singleton persistente.");
    }
    public List<AchievementSaveData> GetAchievementsForSave()
    {
        List<AchievementSaveData> dataToSave = new List<AchievementSaveData>();
        for (int i = 0; i < achievements.Length; i++)
        {
            dataToSave.Add(new AchievementSaveData {
                index = i,
                isUnlocked = achievements[i].isUnlocked,
                hasBeenViewed = achievements[i].hasBeenViewed
            });
        }
        return dataToSave;
    }

    // --- NOVO MÉTODO PARA CARREGAR ---
    /// <summary>
    /// Aplica o estado salvo às conquistas.
    /// </summary>
    public void LoadAchievementStatus(List<AchievementSaveData> savedData)
    {
        if (savedData == null) return;

        foreach (var savedEntry in savedData)
        {
            // Encontra a conquista correspondente pelo índice
            if (savedEntry.index >= 0 && savedEntry.index < achievements.Length)
            {
                achievements[savedEntry.index].isUnlocked = savedEntry.isUnlocked;
                achievements[savedEntry.index].hasBeenViewed = savedEntry.hasBeenViewed;
            }
        }
        Debug.Log("Estado das conquistas carregado.");
    }

    public Achievement[] achievements;
    
    /// <summary>
    /// Desbloqueia o achievement no índice especificado, se ainda não estiver desbloqueado.
    /// </summary>
public void UnlockAchievement(int index)
{
    if (index >= 0 && index < achievements.Length)
    {
        if (!achievements[index].isUnlocked)
        {
            achievements[index].isUnlocked = true;
            achievements[index].hasBeenViewed = false; // <<<--- ADICIONE ESTA LINHA
            Debug.Log("Achievement desbloqueado: " + achievements[index].title);
            // Aqui você pode disparar uma notificação na UI, salvar o progresso, etc.
        }
    }
}

    public bool HasUnviewedAchievements()
    {
        foreach (Achievement ach in achievements)
        {
            // Se encontrarmos UMA conquista que está desbloqueada E não foi vista,
            // já podemos retornar true.
            if (ach.isUnlocked && !ach.hasBeenViewed)
            {
                return true;
            }
        }
        // Se o loop terminar, significa que não há nenhuma nova para ver.
        return false;
    }
    /// <summary>
    /// Método para checar as condições de desbloqueio dos achievements.
    /// Esse método pode ser chamado a partir de eventos do jogo ou periodicamente.
    /// </summary>
    public void CheckAchievements()
    {

        int statusDaRainha = PlayerPrefs.GetInt("QueenPurchased", 0);
        int statusDaGuarda = PlayerPrefs.GetInt("GuardPurchased", 0);
        int statusDaEuropa = PlayerPrefs.GetInt("EuropaPurchased", 0);
        int statusDaAsiatica = PlayerPrefs.GetInt("AsianPurchased", 0);
        int statusDaJandaira = PlayerPrefs.GetInt("JandairaPurchased", 0);
        int statusDaBora = PlayerPrefs.GetInt("BoraPurchased", 0);

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

        if (CombatManager.Instancia.fatorconquista == 1)
        {
            UnlockAchievement(2);
        }

        if (CombatManager.Instancia.fatorconquista == 2)
        {
            UnlockAchievement(3);
        }

        if (GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Mel) >= 100)
        {
            UnlockAchievement(4);
        }

        if (statusDaRainha == 1)
        {
            UnlockAchievement(5);
        }

        if (statusDaGuarda == 1)
        {
            UnlockAchievement(6);
        }

        if (statusDaEuropa == 1)
        {
            UnlockAchievement(7);
        }

        if (statusDaAsiatica == 1)
        {
            UnlockAchievement(8);
        }

        if (statusDaJandaira == 1)
        {
            UnlockAchievement(9);
        }


        if (statusDaBora == 1)
        {
            UnlockAchievement(10);
        }
        


    }


    void Update()
    {
        CheckAchievements();
    }
}
