// Scripts/Bees/PassiveBee.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Essencial para Coroutines

[RequireComponent(typeof(NavMeshAgent))]
public class PassiveBee : MonoBehaviour
{
    [Header("Identificação")]
    [Tooltip("Nome único do tipo de abelha, deve corresponder ao do BeeManager.")]
    public string beeType; 

    [Header("Geração de Renda")]
    [Tooltip("Quantidade de Mel que esta abelha gera por segundo.")]
    public float baseIncomePerSecond = 1f;

    [Header("Comportamento de Movimento")]
    [Tooltip("O modelo 3D da abelha que irá 'tremer' durante o voo estacionário.")]
    public Transform beeVisuals;
    [Tooltip("Raio para posicionamento aleatório próximo ao destino.")]
    public float destinationOffsetRadius = 1f;
    [Tooltip("A chance (de 0 a 1) de ir para uma flor em vez da colmeia.")]
    [Range(0f, 1f)]
    public float chanceDeVisitarFlor = 0.85f;

    [Header("Configuração do Voo Estacionário")]
    [Tooltip("A velocidade do 'tremor'. Valores maiores = mais rápido.")]
    public float hoverNoiseSpeed = 2f;
    [Tooltip("A distância máxima do 'tremor'. Valores maiores = mais intenso.")]
    public float hoverAmount = 0.08f;
    [Tooltip("Velocidade para suavizar o reset da posição visual.")]
    public float visualResetSpeed = 5f;

    private NavMeshAgent _agent;
    private Coroutine _patrolCoroutine;
    private Coroutine _hoverCoroutine;
    private Coroutine _resetVisualsCoroutine;
    
    // Sementes aleatórias para que cada abelha se mova de forma única
    private float _perlinSeedX, _perlinSeedY, _perlinSeedZ;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        
        // Inicializa as sementes de ruído com valores aleatórios
        _perlinSeedX = Random.Range(0f, 100f);
        _perlinSeedY = Random.Range(0f, 100f);
        _perlinSeedZ = Random.Range(0f, 100f);

        if (LocationsManager.Instancia == null || beeVisuals == null)
        {
            Debug.LogError($"Configuração incompleta na abelha {gameObject.name}! Verifique se LocationsManager existe e se 'beeVisuals' está atribuído. Desativando...", this);
            this.enabled = false;
            return;
        }
        
        // Inicia a rotina principal
        _patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    void OnEnable()
    {
        if (PassiveIncomeManager.Instancia != null)
        {
            PassiveIncomeManager.Instancia.RegisterPassiveBee(this);
        }
        if (BeeVisualsManager.Instancia != null)
        {
            BeeVisualsManager.Instancia.RegisterPassiveBee(this);
        }
    }

    void OnDisable()
    {
        if (PassiveIncomeManager.Instancia != null)
        {
            PassiveIncomeManager.Instancia.UnregisterPassiveBee(this);
        }
        if (BeeVisualsManager.Instancia != null)
        {
            BeeVisualsManager.Instancia.UnregisterPassiveBee(this);
        }
    }
    
    /// <summary>
    /// Ativa ou desativa o GameObject que contém o modelo 3D da abelha.
    /// Chamado pelo BeeVisualsManager.
    /// </summary>
    public void SetVisualsActive(bool isActive)
    {
        if (beeVisuals != null && beeVisuals.gameObject.activeSelf != isActive)
        {
            beeVisuals.gameObject.SetActive(isActive);
        }
    }

    private IEnumerator PatrolRoutine()
    {
        // Adiciona um delay inicial aleatório para que as abelhas não comecem em sincronia
        yield return new WaitForSeconds(Random.Range(0f, 2f));

        while (this.enabled)
        {
            // Garante que o tremor parou e a posição visual foi resetada ANTES de mover
            StopHovering();
            if (_resetVisualsCoroutine != null)
            {
                yield return _resetVisualsCoroutine;
            }

            // 1. Decide o próximo destino (flor ou colmeia)
            Transform targetObject;
            if (Random.value < chanceDeVisitarFlor)
            {
                targetObject = LocationsManager.Instancia.GetRandomFlowerTarget();
            }
            else
            {
                targetObject = LocationsManager.Instancia.hiveTarget;
            }

            if (targetObject == null)
            {
                Debug.LogWarning("Não foi possível encontrar um destino válido. Tentando novamente em 5s.", this);
                yield return new WaitForSeconds(5f);
                continue; // Tenta novamente no próximo ciclo
            }

            // 2. Calcula um ponto aleatório ao redor do destino
            Vector3 finalDestination = GetRandomPointAround(targetObject.position);
            _agent.SetDestination(finalDestination);

            // 3. Espera chegar ao destino
            yield return new WaitUntil(() => !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance);

            // 4. Chegou! Começa a pairar e espera um tempo aleatório
            StartHovering();
            yield return new WaitForSeconds(Random.Range(3f, 8f));
        }
    }

    private Vector3 GetRandomPointAround(Vector3 basePosition)
    {
        Vector2 randomOffset = Random.insideUnitCircle * destinationOffsetRadius;
        Vector3 searchPosition = basePosition + new Vector3(randomOffset.x, 0, randomOffset.y);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(searchPosition, out hit, destinationOffsetRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return basePosition; // Retorna a posição central se não encontrar um ponto válido
    }
    
    private void StartHovering()
    {
        if (_hoverCoroutine == null)
        {
            _hoverCoroutine = StartCoroutine(HoverCoroutine());
        }
    }

    private void StopHovering()
    {
        if (_hoverCoroutine != null)
        {
            StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = null;
        }
        // Inicia a coroutine para resetar a posição suavemente
        if (_resetVisualsCoroutine == null)
        {
            _resetVisualsCoroutine = StartCoroutine(SmoothlyResetVisualsPosition());
        }
    }

    // Coroutine para resetar suavemente a posição do modelo visual, evitando o "teleporte"
    private IEnumerator SmoothlyResetVisualsPosition()
    {
        while (beeVisuals.localPosition.sqrMagnitude > 0.0001f)
        {
            beeVisuals.localPosition = Vector3.Lerp(beeVisuals.localPosition, Vector3.zero, Time.deltaTime * visualResetSpeed);
            yield return null;
        }
        beeVisuals.localPosition = Vector3.zero; // Garante a posição final exata
        _resetVisualsCoroutine = null;
    }

    // Coroutine para o efeito de voo estacionário usando Ruído de Perlin
    private IEnumerator HoverCoroutine()
    {
        Vector3 originalPosition = beeVisuals.localPosition;

        while (true)
        {
            // Usa Ruído de Perlin para um movimento suave e pseudo-aleatório
            float time = Time.time * hoverNoiseSpeed;
            // Mapeia o resultado de Perlin (0 a 1) para o intervalo de -1 a 1
            float xOffset = (Mathf.PerlinNoise(time, _perlinSeedX) * 2 - 1) * hoverAmount;
            float yOffset = (Mathf.PerlinNoise(time, _perlinSeedY) * 2 - 1) * hoverAmount;
            float zOffset = (Mathf.PerlinNoise(time, _perlinSeedZ) * 2 - 1) * hoverAmount;
            
            beeVisuals.localPosition = originalPosition + new Vector3(xOffset, yOffset, zOffset);
            yield return null;
        }
    }
}