// Scripts/CombatSystem/Combatant3D.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Para interagir com o Slider da UI

public class Combatant3D : MonoBehaviour
{
    [Header("Status de Combate (Runtime)")]
    public string combatantName; // Nome para exibição ou debug
    public int maxHP { get; private set; }
    public int currentHP { get; private set; }
    public int attackPower { get; private set; }
    public bool isPlayerTeam { get; private set; } // Para saber a qual time pertence

    [Header("Referências da UI de Vida")]
    [Tooltip("O componente Slider que representa a barra de vida.")]
    public Slider healthBarSlider;
    [Tooltip("O GameObject pai da barra de vida (geralmente um painel ou imagem no Canvas World Space) que deve encarar a câmera.")]
    public GameObject healthBarCanvasElement;
    [Tooltip("Ponto de ancoragem no modelo 3D onde a barra de vida deve aparecer (um GameObject filho vazio).")]
    public Transform healthBarAnchor; // Atribua no Inspector do prefab

    [Header("Configurações de Morte")]
    [Tooltip("Tempo para o objeto ser desativado/destruído após a 'morte'.")]
    public float deathEffectDelay = 1.5f;

    private Camera _activeCombatCamera; // Câmera de combate ativa, passada pelo CombatManager
    // private Animator _animator; // Descomente quando adicionar animações

    void Awake()
    {
        // _animator = GetComponentInChildren<Animator>(); // Ou GetComponent<Animator>(); Descomente depois
        // A referência da câmera (_activeCombatCamera) será definida no Initialize.
    }

    void LateUpdate()
    {
        // Faz a UI da barra de vida (o elemento Canvas) encarar a câmera e se posicionar no anchor
        if (healthBarCanvasElement != null && healthBarCanvasElement.activeSelf && _activeCombatCamera != null)
        {
            if (healthBarAnchor != null)
            {
                // Posiciona a barra de vida no ponto de ancoragem
                healthBarCanvasElement.transform.position = healthBarAnchor.position;
            }
            else
            {
                // Fallback caso o anchor não esteja definido (posiciona um pouco acima do pivot do objeto)
                healthBarCanvasElement.transform.position = transform.position + Vector3.up * 1.2f; // Ajuste a altura (Y) conforme necessário
            }

            // Faz a UI da barra de vida encarar a câmera
            healthBarCanvasElement.transform.LookAt(
                healthBarCanvasElement.transform.position + _activeCombatCamera.transform.rotation * Vector3.forward,
                _activeCombatCamera.transform.rotation * Vector3.up
            );
        }
    }

    /// <summary>
    /// Inicializa os status do combatente. Chamado pelo CombatManager ao criar a instância.
    /// </summary>
    public void Initialize(string name, int calculatedMaxHP, int calculatedAttackPower, bool isPlayer,
                           Slider uiSliderInstance, GameObject uiCanvasElementInstance, Camera activeCombatCam)
    {
        combatantName = name;
        // Define o nome do GameObject na hierarquia para fácil identificação durante o debug
        gameObject.name = $"{name}_{(isPlayer ? "Player" : "Enemy")}_{GetInstanceID()}"; 
        maxHP = calculatedMaxHP;
        currentHP = maxHP;
        attackPower = calculatedAttackPower;
        isPlayerTeam = isPlayer;
        _activeCombatCamera = activeCombatCam; // Recebe a câmera de combate ativa

        // Atribui as instâncias da UI de vida passadas pelo CombatManager
        healthBarSlider = uiSliderInstance;
        healthBarCanvasElement = uiCanvasElementInstance;

        // Tenta encontrar o Slider se apenas o elemento canvas foi passado
        if (healthBarSlider == null && healthBarCanvasElement != null)
        {
            healthBarSlider = healthBarCanvasElement.GetComponentInChildren<Slider>();
        }
        
        // Logs de aviso se algo essencial da UI estiver faltando
        if (healthBarSlider == null && healthBarCanvasElement != null) // Apenas avisa se o canvas element foi fornecido mas o slider não foi encontrado
        {
             Debug.LogWarning($"HealthBarSlider não pôde ser encontrado em {healthBarCanvasElement.name} para {gameObject.name}");
        }
        if (healthBarCanvasElement == null)
        {
             Debug.LogWarning($"HealthBarCanvasElement não atribuído para {gameObject.name}. Barra de vida não funcionará corretamente.");
        }
        if (healthBarAnchor == null)
        {
            Debug.LogWarning($"HealthBarAnchor não atribuído para {gameObject.name}. Barra de vida usará posição de fallback.");
        }

        UpdateHealthBarVisuals();

        if (healthBarCanvasElement != null)
        {
            healthBarCanvasElement.SetActive(IsAlive()); // Mostra a barra de vida se o combatente estiver vivo
        }
    }

    /// <summary>
    /// Aplica dano ao combatente.
    /// </summary>
    public void TakeDamage(int damageAmount)
    {
        if (!IsAlive()) return; // Já está morto, não pode tomar mais dano

        currentHP -= damageAmount;
        currentHP = Mathf.Max(currentHP, 0); // Garante que o HP não fique negativo

        // Debug.Log($"{combatantName} tomou {damageAmount} de dano, HP restante: {currentHP}");
        UpdateHealthBarVisuals();
        // _animator?.SetTrigger("Hit"); // Descomente quando tiver animação de tomar dano

        if (!IsAlive())
        {
            Die();
        }
    }

    /// <summary>
    /// Atualiza a representação visual da barra de vida.
    /// </summary>
    private void UpdateHealthBarVisuals()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = (float)currentHP / maxHP;
        }
    }

    /// <summary>
    /// Chamado quando o HP do combatente chega a zero.
    /// </summary>
    private void Die()
    {
        Debug.Log($"{combatantName} foi derrotado!");
        // _animator?.SetTrigger("Death"); // Descomente quando tiver animação de morte

        if (healthBarCanvasElement != null)
        {
            healthBarCanvasElement.SetActive(false); // Esconde a barra de vida
        }

        // Desabilita o collider para não ser mais alvo
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Inicia a rotina para desativar/destruir o objeto após um tempo
        StartCoroutine(HandleDeathEffect());
    }

    private IEnumerator HandleDeathEffect()
    {
        // Futuramente, aqui você poderia tocar um efeito de partícula de morte, etc.
        yield return new WaitForSeconds(deathEffectDelay);
        gameObject.SetActive(false); // Simplesmente desativa o objeto por enquanto
        // Ou Destroy(gameObject); se preferir destruir permanentemente (cuidado com referências no CombatManager)
    }

    /// <summary>
    /// Verifica se o combatente ainda está vivo.
    /// </summary>
    public bool IsAlive()
    {
        return currentHP > 0;
    }

    /// <summary>
    /// Ação de ataque conceitual. O CombatManager chamará esta função.
    /// </summary>
    public void PerformAttackAction(Combatant3D target)
    {
        if (!IsAlive()) return; // Não pode atacar se estiver morto

        if (target != null && target.IsAlive())
        {
            // Faz a abelha encarar o alvo antes de "atacar"
            Vector3 directionToTarget = target.transform.position - transform.position;
            directionToTarget.y = 0; // Mantém a abelha reta, sem inclinar no eixo Y
            if (directionToTarget != Vector3.zero) // Evita erro se estiverem na mesma posição
            {
                transform.rotation = Quaternion.LookRotation(directionToTarget);
            }

            Debug.Log($"{this.combatantName} (ATK:{attackPower}) ataca {target.combatantName} (HP:{target.currentHP}) conceitualmente.");
            // _animator?.SetTrigger("Attack"); // Descomente para animação de ataque
        }
        else
        {
            // Se não houver alvo vivo, pode ter uma ação padrão ou apenas logar
            Debug.Log($"{this.combatantName} (ATK:{attackPower}) não encontrou um alvo vivo para atacar ou o alvo é nulo.");
        }
        // A aplicação REAL do dano será orquestrada pelo CombatManager,
        // que pode acontecer logo após esta chamada ou após um delay para simular animação/projétil.
    }
}