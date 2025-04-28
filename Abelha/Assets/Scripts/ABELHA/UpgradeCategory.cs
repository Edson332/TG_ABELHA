using UnityEngine;
using System; // Necessário para [Serializable]

[Serializable] // Permite que esta classe seja visível e editável no Inspector dentro de BeeData
public class UpgradeCategory
{
    public string categoryName = "Nova Categoria"; // Ex: "Carga", "Velocidade"
    public int maxLevel = 3;
    public int baseCost = 100; // Custo para o nível 1
    public float costIncreaseFactor = 1.5f; // Quanto o custo aumenta por nível (ex: 100, 150, 225...)

    // Função para calcular o custo de um nível específico
    public int GetCostForLevel(int targetLevel)
    {
        if (targetLevel <= 0 || targetLevel > maxLevel) return -1; // Nível inválido ou já no máximo
        // Calcula o custo baseado no nível anterior (custo exponencial)
        return Mathf.CeilToInt(baseCost * Mathf.Pow(costIncreaseFactor, targetLevel - 1));
    }
}
