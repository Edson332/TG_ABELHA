using UnityEngine;
using System.Collections.Generic; // Para usar List

[CreateAssetMenu(fileName = "NovaAbelha", menuName = "SeuJogo/Dados Abelha")]
public class BeeData : ScriptableObject
{
    [Header("Informações Básicas")]
    public string beeName = "Nome da Abelha";
    public string beeFunction = "Função da Abelha"; // Ex: Coletora, Produtora
    public Sprite beeIcon; // Arraste o Sprite da abelha aqui no Inspector
    public int buyCost = 50;
    public int maxQuantity = 20; // Quantidade máxima que o jogador pode ter

    [Header("Evolução")]
    public List<UpgradeCategory> upgradeCategories = new List<UpgradeCategory>(); // Lista das categorias de upgrade possíveis

    // Helper para encontrar uma categoria de upgrade pelo nome
    public UpgradeCategory GetUpgradeCategory(string categoryName)
    {
        return upgradeCategories.Find(cat => cat.categoryName == categoryName);
    }
}