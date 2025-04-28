// BeeManager.cs
using System.Collections.Generic;
using UnityEngine;

public class BeeManager : MonoBehaviour
{
    public static BeeManager Instancia;

    [System.Serializable]
    public class BeeData
    {
        [Tooltip("Nome único do tipo de abelha (ex: WorkerBee, ProducerBee)")]
        public string beeType;
        [Tooltip("Prefab que será instanciado ao comprar esta abelha")]
        public GameObject prefab;
        [Tooltip("Custo em Mel para comprar uma unidade")]
        public int cost;
        [Tooltip("Quantidade máxima de unidades que o jogador pode ter")]
        public int maxCount;
        [HideInInspector]
        public int currentCount;
    }

    [Header("Configuração de tipos de abelha")]
    public List<BeeData> beeTypes = new List<BeeData>();

    [Header("Spawn e atraso inicial")]
    public Transform spawnPoint;
    public float minInitialDelay = 0.5f;
    public float maxInitialDelay = 2f;

    private void Awake()
    {
        if (Instancia == null)
            Instancia = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Tenta comprar e spawnar uma abelha do tipo especificado.
    /// Retorna true se a compra e instância ocorreram com sucesso.
    /// </summary>
    public bool TrySpawnBee(string beeType)
    {
        var data = beeTypes.Find(b => b.beeType == beeType);
        if (data == null)
        {
            Debug.LogWarning($"[BeeManager] Tipo de abelha '{beeType}' não cadastrado.");
            return false;
        }

        if (data.currentCount >= data.maxCount)
        {
            Debug.Log($"[BeeManager] Limite de {beeType} atingido ({data.currentCount}/{data.maxCount}).");
            return false;
        }

        if (GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Mel) < data.cost)
        {
            Debug.Log("[BeeManager] Mel insuficiente para compra.");
            return false;
        }

        // Remove o custo e instancia a abelha
        GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Mel, data.cost);
        var newBee = Instantiate(data.prefab, spawnPoint.position, spawnPoint.rotation);
        float delay = Random.Range(minInitialDelay, maxInitialDelay);

        // Ajusta o delay no script correto
        switch (beeType)
        {
            case "WorkerBee":
                var w = newBee.GetComponent<WorkerBee>();
                if (w != null) w.initialDelay = delay;
                break;
            case "ProducerBee":
                var p = newBee.GetComponent<ProducerBee>();
                if (p != null) p.initialDelay = delay;
                break;
            // adicione mais cases para outros tipos
        }

        data.currentCount++;
        return true;
    }

    /// <summary>
    /// Retorna uma string com a contagem de todas as abelhas (ex: "WorkerBee 2/3\nProducerBee 1/5").
    /// </summary>
    public string GetBeeCountString()
    {
        var lines = new List<string>();
        foreach (var data in beeTypes)
            lines.Add($"{data.beeType} {data.currentCount}/{data.maxCount}");
        return string.Join("\n", lines);
    }

    /// <summary>
    /// Aumenta o limite máximo de um tipo de abelha (para upgrades futuros).
    /// </summary>
    public void AumentarLimite(string beeType, int quantidade)
    {
        var data = beeTypes.Find(b => b.beeType == beeType);
        if (data != null)
            data.maxCount += quantidade;
    }
}
