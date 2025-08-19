// BeeUpgradeButton.cs
using UnityEngine;

public class BeeUpgradeButton : MonoBehaviour
{
    // ===================================================================
    // SEÇÃO PARA COMPRAR NOVAS ABELHAS (SEU CÓDIGO ATUAL - ESTÁ PERFEITO)
    // ===================================================================

    /// <summary>
    /// Chamado pelo botão para comprar uma nova WorkerBee.
    /// </summary>
    public void OnBuyWorkerBeeClicked()
    {
        bool comprou = BeeManager.Instancia.TrySpawnBee("WorkerBee");
        if (!comprou)
        {
            // Feedback de erro (ex: som, UI)
            Debug.Log("Não foi possível comprar WorkerBee.");
        }
    }

    /// <summary>
    /// Chamado pelo botão para comprar uma nova ProducerBee.
    /// </summary>
    public void OnBuyProducerBeeClicked()
    {
        bool comprou = BeeManager.Instancia.TrySpawnBee("ProducerBee");
        if (!comprou)
        {
            // Feedback de erro
            Debug.Log("Não foi possível comprar ProducerBee.");
        }
    }

    public void OnBuyAsiaticaBeeClicked()
    {
        bool comprou = BeeManager.Instancia.TrySpawnBee("AsiaticaBee");
        if (!comprou) { Debug.Log("Não foi possível comprar AsiaticaBee."); }
    }

    public void OnBuyEuropaBeeClicked()
    {
        bool comprou = BeeManager.Instancia.TrySpawnBee("EuropaBee");
        if (!comprou)
        {
            // Feedback de erro
            Debug.Log("Não foi possível comprar a Abelha Europa.");
        }
    }

    /// <summary>
    /// Chamado pelo botão para comprar a QueenBee.
    /// </summary>
    public void OnBuyQueenBeeClicked()
    {
        bool comprou = BeeManager.Instancia.TrySpawnBee("QueenBee");
        if (!comprou)
        {
            // Feedback de erro
            Debug.Log("Não foi possível comprar QueenBee.");
        }
    }
    
    // Adicione aqui outros botões para comprar novos tipos de abelhas (ex: GuardBee)
    public void OnBuyGuardBeeClicked()
    {
        bool comprou = BeeManager.Instancia.TrySpawnBee("GuardBee");
        if (!comprou)
        {
            Debug.Log("Não foi possível comprar GuardBee.");
        }
    }


    // ===================================================================
    // NOVA SEÇÃO PARA MELHORAR ATRIBUTOS DAS ABELHAS
    // ===================================================================

    // --- Upgrades da Abelha Trabalhadora (WorkerBee) ---
    public void OnUpgradeWorkerHealth()
    {
        GerenciadorUpgrades.Instancia.TentarComprarUpgrade("WorkerBee", TipoUpgrade.VidaCombate);
    }
    public void OnUpgradeWorkerAttack()
    {
        GerenciadorUpgrades.Instancia.TentarComprarUpgrade("WorkerBee", TipoUpgrade.AtaqueCombate);
    }
    public void OnUpgradeWorkerSpeed()
    {
        GerenciadorUpgrades.Instancia.TentarComprarUpgrade("WorkerBee", TipoUpgrade.VelocidadeMovimento);
    }
    public void OnUpgradeWorkerNectar()
    {
        GerenciadorUpgrades.Instancia.TentarComprarUpgrade("WorkerBee", TipoUpgrade.NectarColetado);
    }

    // --- Upgrades da Abelha Produtora (ProducerBee) ---
    public void OnUpgradeProducerHealth()
    {
        GerenciadorUpgrades.Instancia.TentarComprarUpgrade("ProducerBee", TipoUpgrade.VidaCombate);
    }
    public void OnUpgradeProducerAttack()
    {
        GerenciadorUpgrades.Instancia.TentarComprarUpgrade("ProducerBee", TipoUpgrade.AtaqueCombate);
    }
    public void OnUpgradeProducerSpeed()
    {
        GerenciadorUpgrades.Instancia.TentarComprarUpgrade("ProducerBee", TipoUpgrade.VelocidadeMovimento);
    }
    public void OnUpgradeProducerProduction()
    {
        GerenciadorUpgrades.Instancia.TentarComprarUpgrade("ProducerBee", TipoUpgrade.MelProduzido);
    }

    // --- Upgrades da Abelha Guarda (GuardBee) ---
    // (Exemplo para a nova abelha que criamos)
    public void OnUpgradeGuardHealth()
    {
        GerenciadorUpgrades.Instancia.TentarComprarUpgrade("GuardBee", TipoUpgrade.VidaCombate);
    }
    public void OnUpgradeGuardAttack()
    {
        GerenciadorUpgrades.Instancia.TentarComprarUpgrade("GuardBee", TipoUpgrade.AtaqueCombate);
    }
     public void OnUpgradeGuardSpeed()
    {
        GerenciadorUpgrades.Instancia.TentarComprarUpgrade("GuardBee", TipoUpgrade.VelocidadeMovimento);
    }
}
