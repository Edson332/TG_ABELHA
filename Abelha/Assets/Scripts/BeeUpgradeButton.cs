// BeeUpgradeButton.cs
using UnityEngine;

public class BeeUpgradeButton : MonoBehaviour
{
    /// <summary>
    /// Chamado pelo botão de upgrade da WorkerBee (defina no Inspector).
    /// </summary>
    public void OnWorkerUpgradeButtonClicked()
    {
        bool comprou = BeeManager.Instancia.TrySpawnBee("WorkerBee");
        if (!comprou)
        {
            // Aqui você pode disparar um feedback de erro (som, UI, etc.)
        }
    }

    /// <summary>
    /// Chamado pelo botão de upgrade da ProducerBee (defina no Inspector).
    /// </summary>
    public void OnProducerUpgradeButtonClicked()
    {
        bool comprou = BeeManager.Instancia.TrySpawnBee("ProducerBee");
        if (!comprou)
        {
            // Feedback de erro
        }
    }

    public void OnQueenBeeUpgradeButtonClicked()
    {
        bool comprou = BeeManager.Instancia.TrySpawnBee("QueenBee");
        if (!comprou)
        {
            // Feedback de erro
        }
    }
}
