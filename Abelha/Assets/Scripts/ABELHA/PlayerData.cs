using UnityEngine;
using System.Collections.Generic;
using System; // Para Action (eventos)

public class PlayerData : MonoBehaviour // Pode ser um Singleton ou parte de um GameManager
{
    public static PlayerData Instance { get; private set; } // Exemplo Singleton simples

    public int currentHoney = 1000; // Mel inicial

    // Dicionário para guardar quantas de cada abelha (pelo ScriptableObject) o jogador tem
    public Dictionary<BeeData, int> ownedBees = new Dictionary<BeeData, int>();

    // Dicionário para guardar os níveis de upgrade de cada abelha
    // Estrutura: BeeData -> (NomeCategoria -> NivelAtual)
    public Dictionary<BeeData, Dictionary<string, int>> beeUpgradeLevels = new Dictionary<BeeData, Dictionary<string, int>>();

    // Evento para notificar a UI quando os dados mudam (opcional, mas bom)
    public event Action OnPlayerDataUpdated;

    void Awake()
    {
        // Configuração Singleton simples
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Se precisar persistir entre cenas
        }
    }

    // --- Métodos de Acesso e Modificação ---

    public int GetHoney() => currentHoney;

    public bool HasEnoughHoney(int amount) => currentHoney >= amount;

    public void SpendHoney(int amount)
    {
        if (HasEnoughHoney(amount))
        {
            currentHoney -= amount;
            NotifyUpdate();
        }
        // Considerar lançar um erro ou retornar false se não tiver mel suficiente
    }

     public void AddHoney(int amount)
    {
        currentHoney += amount;
        NotifyUpdate();
    }

    public int GetBeeCount(BeeData beeType)
    {
        ownedBees.TryGetValue(beeType, out int count);
        return count;
    }

    public void AddBee(BeeData beeType, int amount = 1)
    {
        if (!ownedBees.ContainsKey(beeType))
        {
            ownedBees[beeType] = 0;
            // Inicializa os níveis de upgrade para esta abelha se for a primeira vez
            if (!beeUpgradeLevels.ContainsKey(beeType))
            {
                beeUpgradeLevels[beeType] = new Dictionary<string, int>();
                foreach (var category in beeType.upgradeCategories)
                {
                    beeUpgradeLevels[beeType][category.categoryName] = 0; // Começa no nível 0
                }
            }
        }
        ownedBees[beeType] += amount;
        NotifyUpdate();
    }

    public int GetUpgradeLevel(BeeData beeType, string categoryName)
    {
        if (beeUpgradeLevels.TryGetValue(beeType, out var upgrades) &&
            upgrades.TryGetValue(categoryName, out int level))
        {
            return level;
        }
        return 0; // Retorna 0 se não encontrar (nível base)
    }

    public void SetUpgradeLevel(BeeData beeType, string categoryName, int newLevel)
    {
        if (!beeUpgradeLevels.ContainsKey(beeType)) return; // Abelha não possuída?
        if (!beeUpgradeLevels[beeType].ContainsKey(categoryName)) return; // Categoria inválida?

        beeUpgradeLevels[beeType][categoryName] = newLevel;
        NotifyUpdate();
    }

    // Notifica os listeners (como a UI da loja) que os dados mudaram
    private void NotifyUpdate()
    {
        OnPlayerDataUpdated?.Invoke();
    }
}