// Scripts/UI/IncomeDisplayUI.cs
using UnityEngine;
using TMPro;

public class IncomeDisplayUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI incomeText;

    [Header("Configuração")]
    [Tooltip("Com que frequência (em segundos) o texto de renda deve ser atualizado.")]
    public float updateInterval = 0.5f;

    private float _timer;

    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            UpdateIncomeDisplay();
            _timer = updateInterval;
        }
    }

    private void UpdateIncomeDisplay()
    {
        if (incomeText == null) return;

        float totalIncomePerSecond = 0f;

        // 1. Renda das Abelhas Passivas (já inclui bônus da rainha)
        if (PassiveIncomeManager.Instancia != null)
        {
            totalIncomePerSecond += PassiveIncomeManager.Instancia.GetTotalPassiveIncomePerSecond();
        }

        // 2. Renda Estimada das Abelhas Primárias
        // Percorre todas as abelhas ativas na cena (pode ser pesado se houver muitas)
        // Alternativa: O BeeManager poderia manter listas separadas por tipo.
        GameObject[] allBees = GameObject.FindGameObjectsWithTag("Bee");
        foreach (var beeGO in allBees)
        {
            // Tenta obter o componente WorkerBee e adiciona sua renda
            var worker = beeGO.GetComponent<WorkerBee>();
            if (worker != null)
            {
                totalIncomePerSecond += worker.GetAverageHoneyPerSecond();
                continue; // Pula para a próxima abelha
            }

            // Tenta obter o componente ProducerBee e adiciona sua renda
            var producer = beeGO.GetComponent<ProducerBee>();
            if (producer != null)
            {
                totalIncomePerSecond += producer.GetAverageHoneyPerSecond();
                continue;
            }
            // Adicione aqui outros tipos de abelhas primárias se houver
        }

        // 3. Aplica Bônus Globais (Geleia Real)
        if (RoyalJellyShopManager.Instancia != null)
        {
            float globalBonusMultiplier = 1f + RoyalJellyShopManager.Instancia.GetGlobalProductionBonus();
            totalIncomePerSecond *= globalBonusMultiplier;
        }

        // 4. Atualiza o Texto da UI
        incomeText.text = $"Mel/s: {totalIncomePerSecond:F1}"; // Formata para 1 casa decimal
    }
}
