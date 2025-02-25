using UnityEngine;
using UnityEngine.UI;

public class BeeUpgradeButton : MonoBehaviour
{
    [Header("Configuração para WorkerBee")]
    public GameObject workerBeePrefab;   // Prefab da WorkerBee
    public int workerUpgradeCost = 10;     // Custo para comprar uma nova WorkerBee

    [Header("Configuração para ProducerBee")]
    public GameObject producerBeePrefab;   // Prefab da ProducerBee
    public int producerUpgradeCost = 15;   // Custo para comprar uma nova ProducerBee

    [Header("Configuração do Spawn e Atraso")]
    public Transform spawnPoint;           // Local onde a nova abelha será instanciada
    public float minInitialDelay = 0.5f;     // Atraso mínimo para a nova abelha
    public float maxInitialDelay = 2f;       // Atraso máximo para a nova abelha

    /// <summary>
    /// Método para spawnar uma nova WorkerBee.
    /// Associe esse método ao botão de upgrade para WorkerBee.
    /// </summary>
    public void OnWorkerUpgradeButtonClicked()
    {
        if (GerenciadorRecursos.Instancia != null)
        {
            if (GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Mel) >= workerUpgradeCost)
            {
                // Remove o custo da compra
                GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Mel, workerUpgradeCost);
                // Instancia a nova WorkerBee
                GameObject newBee = Instantiate(workerBeePrefab, spawnPoint.position, spawnPoint.rotation);
                WorkerBee beeScript = newBee.GetComponent<WorkerBee>();
                if (beeScript != null)
                {
                    // Configura um atraso inicial aleatório para evitar sobreposição
                    beeScript.initialDelay = Random.Range(minInitialDelay, maxInitialDelay);
                }
            }
            else
            {
                Debug.Log("Recursos insuficientes para comprar uma nova WorkerBee!");
            }
        }
    }

    /// <summary>
    /// Método para spawnar uma nova ProducerBee.
    /// Associe esse método ao botão de upgrade para ProducerBee.
    /// </summary>
    public void OnProducerUpgradeButtonClicked()
    {
        if (GerenciadorRecursos.Instancia != null)
        {
            if (GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Mel) >= producerUpgradeCost)
            {
                // Remove o custo da compra
                GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Mel, producerUpgradeCost);
                // Instancia a nova ProducerBee
                GameObject newBee = Instantiate(producerBeePrefab, spawnPoint.position, spawnPoint.rotation);
                ProducerBee producerScript = newBee.GetComponent<ProducerBee>();
                if (producerScript != null)
                {
                    // Configura um atraso inicial aleatório para evitar sobreposição com outras abelhas
                    producerScript.initialDelay = Random.Range(minInitialDelay, maxInitialDelay);
                }
            }
            else
            {
                Debug.Log("Recursos insuficientes para comprar uma nova ProducerBee!");
            }
        }
    }
}
