// Scripts/BeeManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Necessário para NavMesh.SamplePosition

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

        [Tooltip("Nome de exibição para a UI (ex: Abelha Trabalhadora).")]
        public string displayName;
        [Tooltip("Para abelhas passivas, a quantidade de Mel que geram por segundo. Deixe 0 para outras.")]
        public float incomePerSecond = 0f;
        [HideInInspector]
        public int currentCount;

        [Tooltip("Marque esta opção se esta for uma das abelhas primárias (Worker, Producer, Guard).")]
        public bool isPrimary = false; 
        
    }

    [Header("Configuração de tipos de abelha")]
    public List<BeeData> beeTypes = new List<BeeData>();

    [Header("Spawn e atraso inicial")]
    public Transform spawnPoint;
    [Tooltip("Raio ao redor do spawnPoint para evitar que as abelhas apareçam umas sobre as outras.")]
    public float spawnRadius = 1.5f; // <<-- VERIFIQUE ESTE VALOR NO INSPECTOR
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
    /// </summary>
    /// 
    /// 
    public bool AreAllPrimaryBeesUnlocked()
    {
        // Percorre a lista de todos os tipos de abelhas
        foreach (var beeData in beeTypes)
        {
            // Se encontrarmos uma abelha que É primária...
            if (beeData.isPrimary)
            {
                // ...e o jogador NÃO a possui (contagem é 0), então a condição não foi atendida.
                if (beeData.currentCount <= 0)
                {
                    return false; // Retorna falso imediatamente
                }
            }
        }

        // Se o loop terminar sem retornar falso, significa que todas as primárias foram compradas.
        return true;
    }
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

        // --- LÓGICA DE SPAWN COM RAIO (APLICADA AQUI) ---
        Vector3 spawnPosition = GetRandomSpawnPosition();
        // --- FIM DA LÓGICA DE SPAWN COM RAIO ---

        // Remove o custo e instancia a abelha na posição calculada
        GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Mel, data.cost);
        var newBee = Instantiate(data.prefab, spawnPosition, spawnPoint.rotation);
        
        // Aplica o delay inicial
        ApplyInitialDelay(newBee, beeType);

        data.currentCount++;
        return true;
    }

    /// <summary>
    /// Recria as abelhas na cena com base nos dados de contagem atuais.
    /// </summary>
    public void RespawnBeesFromSaveData()
    {
        Debug.Log("Recriando abelhas a partir dos dados salvos...");
        
        GameObject[] existingBees = GameObject.FindGameObjectsWithTag("Bee");
        foreach(GameObject bee in existingBees)
        {
            Destroy(bee);
        }

        foreach (var beeData in beeTypes)
        {
            for (int i = 0; i < beeData.currentCount; i++)
            {
                // --- ADICIONADA VERIFICAÇÃO DE PREFAB ---
                if (beeData.prefab == null)
                {
                    Debug.LogError($"[BeeManager] O PREFAB para o tipo '{beeData.beeType}' está NULO no Inspector! Não é possível criar esta abelha. Por favor, atribua o prefab correto.");
                    continue; // Pula para a próxima iteração, evitando o erro.
                }
                // --- FIM DA VERIFICAÇÃO ---
                
                Vector3 spawnPosition = GetRandomSpawnPosition();
                var newBee = Instantiate(beeData.prefab, spawnPosition, spawnPoint.rotation);
                ApplyInitialDelay(newBee, beeData.beeType);
            }
        }
    }

    /// <summary>
    /// Calcula uma posição de spawn aleatória dentro do spawnRadius e válida no NavMesh.
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
// Gera um ponto em um círculo 2D e o converte para um vetor 3D no plano XZ.
        Vector2 randomCirclePoint = Random.insideUnitCircle * spawnRadius;
        Vector3 randomPositionOffset = new Vector3(randomCirclePoint.x, 0f, randomCirclePoint.y);
        
        // Posição de origem para a busca é o ponto de spawn + o offset aleatório no plano.
        Vector3 searchPosition = spawnPoint.position + randomPositionOffset;

        NavMeshHit hit;
        Vector3 finalPosition = spawnPoint.position; // Posição de fallback caso a busca falhe

        // Encontra a posição válida mais próxima no NavMesh
        // O raio de busca agora pode ser menor, já que estamos mais perto do alvo
        if (NavMesh.SamplePosition(searchPosition, out hit, spawnRadius, NavMesh.AllAreas))
        {
            finalPosition = hit.position;
        }
        else
        {
            Debug.LogWarning("NavMesh.SamplePosition falhou! A abelha será criada no spawnPoint central. Verifique se o spawnPoint está sobre uma área de NavMesh válida.");
        }
        return finalPosition;
    }

    /// <summary>
    /// Aplica o delay inicial a uma abelha recém-criada.
    /// </summary>
    private void ApplyInitialDelay(GameObject beeInstance, string beeType)
    {
        float delay = Random.Range(minInitialDelay, maxInitialDelay);
        
        // Futuramente, isso pode ser melhorado com uma interface ISpawnable
        switch (beeType)
        {
            case "WorkerBee":
                var w = beeInstance.GetComponent<WorkerBee>();
                if (w != null) w.initialDelay = delay;
                break;
            case "ProducerBee":
                var p = beeInstance.GetComponent<ProducerBee>();
                if (p != null) p.initialDelay = delay;
                break;
            case "QueenBee":
                // A Rainha pode não ter um initialDelay, mas se tiver, a lógica vai aqui.
                break;
        }
    }

    // --- MÉTODOS EXISTENTES SEM ALTERAÇÃO ---

    public void SetCurrentCount(string beeType, int count)
    {
        var data = beeTypes.Find(b => b.beeType == beeType);
        if (data != null)
        {
            data.currentCount = count;
        }
        else
        {
            Debug.LogWarning($"[BeeManager] Tentativa de carregar contagem para tipo de abelha desconhecido: {beeType}");
        }
    }
    
    public string GetBeeCountString()
    {
        var lines = new List<string>();
        foreach (var data in beeTypes)
            lines.Add($"{data.beeType} {data.currentCount}/{data.maxCount}");
        return string.Join("\n", lines);
    }

    public void AumentarLimite(string beeType, int quantidade)
    {
        var data = beeTypes.Find(b => b.beeType == beeType);
        if (data != null)
            data.maxCount += quantidade;
    }
}