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
        
        // --- CAMPOS MODIFICADOS/ADICIONADOS ---
        [Tooltip("O custo para comprar a PRIMEIRA abelha deste tipo.")]
        public int baseCost; // RENOMEADO de 'cost'
        [Tooltip("O fator pelo qual o custo aumenta a cada compra. Ex: 1.07 = 7% de aumento.")]
        public float costMultiplier = 1.07f; // ADICIONADO (1.07 é um bom valor inicial)
        // --- FIM DAS MUDANÇAS ---

        [Tooltip("Quantidade máxima de unidades que o jogador pode ter")]
        public int maxCount;
        [Tooltip("Nome de exibição para a UI (ex: Abelha Trabalhadora).")]
        public string displayName;
        [Tooltip("Para abelhas passivas, a quantidade de Mel que geram por segundo. Deixe 0 para outras.")]
        public float incomePerSecond = 0f;
        [Tooltip("Marque esta opção se esta for uma das abelhas primárias (Worker, Producer, Guard).")]
        public bool isPrimary = false;

        
        
        [HideInInspector]
        public int currentCount;
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

    public double GetCurrentBeeCost(string beeType)
    {
        var data = beeTypes.Find(b => b.beeType == beeType);
        if (data == null)
        {
            return double.MaxValue; // Retorna um valor enorme se o tipo não for encontrado
        }

        // Fórmula: Custo Atual = Custo Base * (Multiplicador ^ Quantidade Atual)
        // Usamos double para lidar com números potencialmente muito grandes no futuro.
        return data.baseCost * System.Math.Pow(data.costMultiplier, data.currentCount);
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
            Debug.Log($"[BeeManager] Limite de {beeType} atingido.");
            return false;
        }

        // --- LÓGICA DE CUSTO MODIFICADA ---
        double currentCost = GetCurrentBeeCost(beeType);

        if (GerenciadorRecursos.Instancia.ObterRecurso(TipoRecurso.Mel) < currentCost)
        {
            Debug.Log($"[BeeManager] Mel insuficiente para comprar {beeType}. Custo: {currentCost:F0}");
            return false;
        }
        // --- FIM DA MODIFICAÇÃO ---

        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Remove o custo dinâmico e instancia a abelha
        GerenciadorRecursos.Instancia.RemoverRecurso(TipoRecurso.Mel, (float)currentCost);
        var newBee = Instantiate(data.prefab, spawnPosition, spawnPoint.rotation);
        
        ApplyInitialDelay(newBee, beeType);

        data.currentCount++;
        

        if (data.beeType == "QueenBee")
        {
            // PlayerPrefs salva um par de "chave" e "valor" no dispositivo.
            // A chave é "QueenPurchased", o valor é 1.
            PlayerPrefs.SetInt("QueenPurchased", 1);
            PlayerPrefs.Save(); // Força o salvamento imediato no disco.
            Debug.Log("FLAG SALVA: O status da Abelha Rainha foi salvo como 1.");

            
        }

        if (data.beeType == "GuardBee")
        {
            // PlayerPrefs salva um par de "chave" e "valor" no dispositivo.
            // A chave é "QueenPurchased", o valor é 1.
            PlayerPrefs.SetInt("GuardPurchased", 1);
            PlayerPrefs.Save(); // Força o salvamento imediato no disco.
            Debug.Log("FLAG SALVA: O status da Abelha Guarda foi salvo como 1.");
        }
        // Anuncia a mudança na contagem para o sistema de achievements, etc.

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