// Scripts/CombatSystem/CombatManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Para referenciar o Slider nos parâmetros de Initialize

public enum CombatState
{
    NotActive, // Adicionado para um estado inicial claro
    Preparing,
    PlayerTurn,
    EnemyTurn,
    Resolving, 
    Finished // Usado após a resolução, antes de limpar para um novo combate
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instancia { get; private set; }

    [Header("Referências da Cena e Câmeras")]
    public Camera mainGameCamera;
    public Camera combatCamera;
    public Transform worldSpaceCanvasTransform; // O Canvas configurado como World Space
    public GameObject healthBarPrefab; // O Prefab do seu Slider (HealthBar_Element)

    [Header("Dados dos Combatentes do Jogador")]
    [Tooltip("Lista de TODOS os tipos de abelhas do jogador que podem participar de combates. Arraste os PlayerBeeCombatDataSO aqui.")]
    public List<PlayerBeeCombatDataSO> allPlayerBeeCombatData;

    [Header("Pontos de Spawn na Arena")]
    public List<Transform> playerSpawnPoints = new List<Transform>();
    public List<Transform> enemySpawnPoints = new List<Transform>();

    [Header("Configurações de Combate")]
    public EnemyWaveSO currentOperatingWave; // Onda atual em operação (setada ao iniciar combate)
    [Tooltip("Pequeno atraso entre os ataques de cada abelha dentro de um turno.")]
    public float actionDelayBetweenAttacks = 0.5f; 
    [Tooltip("Pequeno atraso após o fim de um turno completo (jogador ou inimigo).")]
    public float turnTransitionDelay = 1.0f;
    [Tooltip("Delay para limpar a cena e trocar câmera após fim do combate.")]
    public float postCombatUIDelay = 2.0f;


    [Header("Dados de Teste (Configurar no Inspector)")]
    [Tooltip("Arraste os ScriptableObjects PlayerBeeCombatDataSO para teste.")]
    public List<PlayerBeeCombatDataSO> testPlayerSquadData = new List<PlayerBeeCombatDataSO>();
    [Tooltip("Arraste um ScriptableObject EnemyWaveSO para teste.")]
    public EnemyWaveSO testWaveToUse;

    [Header("Referências da UI de Combate")]
    public CombatResultsUI combatResultsUI; // A
    private List<Combatant3D> _playerCombatants = new List<Combatant3D>();
    private List<Combatant3D> _enemyCombatants = new List<Combatant3D>();
    public int fatorconquista = 0;
    public CombatState currentCombatState { get; private set; } = CombatState.NotActive;
    public bool isCombatActive { get; private set; } = false;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
    }

    void Start()
    {
        // Garante estado inicial correto das câmeras
        if (combatCamera != null) combatCamera.gameObject.SetActive(false);
        if (mainGameCamera != null) mainGameCamera.gameObject.SetActive(true);

        // Descomente a linha abaixo para iniciar um combate de teste automaticamente ao rodar o jogo
         //Test_StartCombatWithInspectorData();
    }

    // Método de utilidade para iniciar um combate de teste via Inspector ou outro script
    public void Test_StartCombatWithInspectorData()
    {
        if (testWaveToUse != null && testPlayerSquadData.Count > 0)
        {
           StartNewCombat(testWaveToUse, testPlayerSquadData);
        }
        else
        {
            Debug.LogError("Dados de teste (testWaveToUse ou testPlayerSquadData) não configurados no CombatManager Inspector para iniciar combate de teste.");
        }
    }


    public void StartNewCombat(EnemyWaveSO enemyWave, List<PlayerBeeCombatDataSO> playerSelectedBees)
    {
        if (isCombatActive || currentCombatState != CombatState.NotActive && currentCombatState != CombatState.Finished)
        {
            Debug.LogWarning("Tentativa de iniciar combate enquanto um já está ativo ou não finalizado corretamente.");
            return;
        }

        Debug.Log($"Iniciando combate contra a onda: {enemyWave.waveName}");
        currentOperatingWave = enemyWave; // Armazena a onda atual para recompensas
        currentCombatState = CombatState.Preparing;
        isCombatActive = true;

        if (mainGameCamera != null) mainGameCamera.gameObject.SetActive(false);
        else Debug.LogWarning("MainGameCamera não definida no CombatManager.");

        if (combatCamera != null) combatCamera.gameObject.SetActive(true);
        else Debug.LogError("CombatCamera não definida no CombatManager! O combate não pode prosseguir sem ela.");


        ClearPreviousCombatants(); // Limpa combatentes de batalhas anteriores
        SpawnPlayerTeam(playerSelectedBees, playerSpawnPoints);
        SpawnEnemyTeam(enemyWave, enemySpawnPoints);

        if (_playerCombatants.Count == 0) {
            Debug.LogError("Nenhum combatente do jogador foi spawnado! O jogador perde por W.O.");
            ProcessCombatResult(false); 
            return;
        }
        if (_enemyCombatants.Count == 0) {
            Debug.Log("Nenhum inimigo foi spawnado! O jogador vence por W.O.");
            ProcessCombatResult(true); 
            return;
        }

        OrientTeams();
        StartCoroutine(CombatLoopCoroutine());
    }

    private IEnumerator CombatLoopCoroutine()
    {
        Debug.Log("Loop de Combate Iniciado.");
        yield return new WaitForSeconds(0.5f); // Pequena pausa para preparação visual
        currentCombatState = CombatState.PlayerTurn; 

        while (isCombatActive && AreAnyPlayerBeesAlive() && AreAnyEnemiesAlive())
        {
            switch (currentCombatState)
            {
                case CombatState.PlayerTurn:
                    Debug.Log("--- TURNO DO JOGADOR ---");
                    yield return StartCoroutine(ExecuteTurn(_playerCombatants, _enemyCombatants, true));
                    if (!AreAnyEnemiesAlive()) break; 
                    currentCombatState = CombatState.EnemyTurn;
                    yield return new WaitForSeconds(turnTransitionDelay);
                    break;

                case CombatState.EnemyTurn:
                    Debug.Log("--- TURNO DO INIMIGO ---");
                    yield return StartCoroutine(ExecuteTurn(_enemyCombatants, _playerCombatants, false));
                    if (!AreAnyPlayerBeesAlive()) break; 
                    currentCombatState = CombatState.PlayerTurn;
                    yield return new WaitForSeconds(turnTransitionDelay);
                    break;
            }
        }

        if (isCombatActive) 
        {
            ProcessCombatResult(AreAnyPlayerBeesAlive() && !AreAnyEnemiesAlive());
        }
    }

    private IEnumerator ExecuteTurn(List<Combatant3D> attackers, List<Combatant3D> defenders, bool isPlayerAttacking)
    {
        string turnPrefix = isPlayerAttacking ? "Jogador:" : "Inimigo:";
        for (int i = 0; i < attackers.Count; i++)
        {
            Combatant3D attacker = attackers[i];
            if (attacker.IsAlive())
            {
                Combatant3D target = FindTargetFor(attacker, defenders);
                if (target != null)
                {
                    attacker.PerformAttackAction(target);
                    // Sem animação, o dano pode ser "instantâneo" ou com um pequeno delay
                    yield return new WaitForSeconds(actionDelayBetweenAttacks * 0.5f); 
                    target.TakeDamage(attacker.attackPower);
                    yield return new WaitForSeconds(actionDelayBetweenAttacks * 0.5f); 

                    if (!target.IsAlive())
                    {
                        Debug.Log($"{turnPrefix} {target.combatantName} foi derrotado!");
                    }
                    // Verifica se o time defensor foi completamente dizimado
                    if (isPlayerAttacking && !AreAnyEnemiesAlive()) yield break;
                    if (!isPlayerAttacking && !AreAnyPlayerBeesAlive()) yield break;
                }
                else
                {
                     Debug.Log($"{turnPrefix} {attacker.combatantName} não encontrou alvos inimigos.");
                }
            }
        }
    }
    
    private Combatant3D FindTargetFor(Combatant3D attacker, List<Combatant3D> potentialTargets)
    {
        for (int i = 0; i < potentialTargets.Count; i++)
        {
            if (potentialTargets[i] != null && potentialTargets[i].IsAlive())
            {
                return potentialTargets[i];
            }
        }
        return null; 
    }

    private bool AreAnyPlayerBeesAlive()
    {
        for (int i = 0; i < _playerCombatants.Count; i++)
        {
            if (_playerCombatants[i] != null && _playerCombatants[i].IsAlive()) return true;
        }
        return false;
    }

    private bool AreAnyEnemiesAlive()
    {
        for (int i = 0; i < _enemyCombatants.Count; i++)
        {
            if (_enemyCombatants[i] != null && _enemyCombatants[i].IsAlive()) return true;
        }
        return false;
    }
    
    private void ProcessCombatResult(bool playerWon)
    {
        currentCombatState = CombatState.Resolving;
        isCombatActive = false; // O loop de combate para aqui

        Debug.Log(playerWon ? "Jogador VENCEU o combate!" : "Jogador foi DERROTADO.");

        
        
        
        if (playerWon && currentOperatingWave != null)
        {
            if (GerenciadorRecursos.Instancia != null)
            {
                fatorconquista = fatorconquista + 1;
                GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Mel, currentOperatingWave.honeyReward);
                Debug.Log($"Recompensa: {currentOperatingWave.honeyReward} de Mel adicionada.");
            }
        }
        
        // Mostra a UI de resultados e espera a interação do jogador
        if (combatResultsUI != null)
        {
            combatResultsUI.ShowResults(playerWon, currentOperatingWave);
        }
        else
        {
            // Se não houver UI, finaliza o combate diretamente após um delay
            Debug.LogWarning("CombatResultsUI não atribuído no CombatManager. Finalizando combate automaticamente.");
            StartCoroutine(CleanupCombatSequence());
        }
    }
    
    public void ConcludeCombatAfterResults()
    {
        StartCoroutine(CleanupCombatSequence());
    }

    private IEnumerator CleanupCombatSequence()
    {
        // Não precisa de delay aqui, pois o jogador já viu a tela de resultados.
        // Apenas um frame de espera para garantir que tudo ocorra na ordem certa.
        yield return null;

        ClearPreviousCombatants(); 

        if (combatCamera != null) combatCamera.gameObject.SetActive(false);
        if (mainGameCamera != null) mainGameCamera.gameObject.SetActive(true);
        
        currentCombatState = CombatState.Finished; 
        Debug.Log("Sequência de conclusão de combate finalizada. Retornando ao jogo principal.");

        if (InvasionScheduler.Instancia != null)
        {
            InvasionScheduler.Instancia.AlertDecisionMade();
            Debug.Log("InvasionScheduler reativado.");
        }
    }


    private void SpawnPlayerTeam(List<PlayerBeeCombatDataSO> playerTeamData, List<Transform> spawnPointList)
    {
        int spawnIndex = 0;
        
        foreach (var dataSO in playerTeamData)
        {
            if (spawnIndex >= spawnPointList.Count) { Debug.LogWarning("Faltam pontos de spawn para time do jogador."); break; }
            if (dataSO.modelPrefab3D == null) { Debug.LogError($"Prefab 3D não definido para {dataSO.combatantName} do jogador."); continue; }

            Transform spawnPoint = spawnPointList[spawnIndex];
            GameObject instance = Instantiate(dataSO.modelPrefab3D, spawnPoint.position, spawnPoint.rotation);
            Combatant3D combatantScript = instance.GetComponent<Combatant3D>();

            if (combatantScript == null) { Debug.LogError($"Prefab {dataSO.modelPrefab3D.name} sem Combatant3D!"); Destroy(instance); continue; }

            int finalMaxHP = dataSO.baseMaxHP;
            int finalAttack = dataSO.baseAttack;

            if (GerenciadorUpgrades.Instancia != null && !string.IsNullOrEmpty(dataSO.beeTypeNameForUpgrades))
            {

                float hpMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(dataSO.beeTypeNameForUpgrades, TipoUpgrade.VidaCombate);
                float attackMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(dataSO.beeTypeNameForUpgrades, TipoUpgrade.AtaqueCombate);
                finalMaxHP = Mathf.RoundToInt(dataSO.baseMaxHP * hpMultiplier);
                finalAttack = Mathf.RoundToInt(dataSO.baseAttack * attackMultiplier);
                Debug.Log($"Player Bee {dataSO.combatantName} ({dataSO.beeTypeNameForUpgrades}): HP Final={finalMaxHP}, ATK Final={finalAttack}");

            }

            InstantiateAndAssignHealthBar(combatantScript, dataSO.combatantName, finalMaxHP, finalAttack, true);
            _playerCombatants.Add(combatantScript);
            spawnIndex++;
        }
    }
    
    private void SpawnEnemyTeam(EnemyWaveSO waveData, List<Transform> spawnPointList)
    {
        int spawnIndex = 0;
        foreach (var group in waveData.enemyGroups)
        {
            for (int i = 0; i < group.quantity; i++)
            {
                if (spawnIndex >= spawnPointList.Count) { Debug.LogWarning("Faltam pontos de spawn para inimigos."); break; }
                if (group.enemyData.modelPrefab3D == null) { Debug.LogError($"Prefab 3D não definido para inimigo {group.enemyData.combatantName}"); continue; }

                Transform spawnPoint = spawnPointList[spawnIndex];
                GameObject instance = Instantiate(group.enemyData.modelPrefab3D, spawnPoint.position, spawnPoint.rotation);
                Combatant3D combatantScript = instance.GetComponent<Combatant3D>();

                if (combatantScript == null) { Debug.LogError($"Prefab inimigo {group.enemyData.modelPrefab3D.name} sem Combatant3D!"); Destroy(instance); continue; }
                
                InstantiateAndAssignHealthBar(combatantScript, group.enemyData.combatantName, group.enemyData.baseMaxHP, group.enemyData.baseAttack, false);
                _enemyCombatants.Add(combatantScript);
                spawnIndex++;
            }
            if (spawnIndex >= spawnPointList.Count) break; 
        }
    }

    private void InstantiateAndAssignHealthBar(Combatant3D combatantScript, string name, int maxHp, int attack, bool isPlayer)
    {
        Slider healthBarSliderInstance = null; 
        GameObject healthBarGOInstance = null;
        if (healthBarPrefab != null && worldSpaceCanvasTransform != null)
        {
            healthBarGOInstance = Instantiate(healthBarPrefab, worldSpaceCanvasTransform);
            healthBarSliderInstance = healthBarGOInstance.GetComponent<Slider>();
            if(healthBarSliderInstance == null) Debug.LogError("HealthBarPrefab não contém um componente Slider!");
        }
        else
        {
            Debug.LogWarning("HealthBar Prefab ou WorldSpaceCanvasTransform não definidos no CombatManager. Barras de vida não serão criadas.");
        }
        combatantScript.Initialize(name, maxHp, attack, isPlayer, healthBarSliderInstance, healthBarGOInstance, combatCamera);
    }

    private void OrientTeams()
    {
        if (_playerCombatants.Count == 0 || _enemyCombatants.Count == 0) return;
        Vector3 enemyFormationCenter = GetTeamCenter(_enemyCombatants);
        Vector3 playerFormationCenter = GetTeamCenter(_playerCombatants);

        if (enemyFormationCenter == Vector3.zero && playerFormationCenter == Vector3.zero) return; // Evita erro se ambos os times não tiverem posições válidas

        OrientListOfCombatants(_playerCombatants, enemyFormationCenter);
        OrientListOfCombatants(_enemyCombatants, playerFormationCenter);
        Debug.Log("Times orientados para o combate.");
    }

    private Vector3 GetTeamCenter(List<Combatant3D> team)
    {
        Vector3 center = Vector3.zero;
        int aliveCount = 0;
        foreach (Combatant3D member in team)
        {
            if (member != null && member.IsAlive()) 
            {
                center += member.transform.position;
                aliveCount++;
            }
        }
        if (aliveCount > 0) center /= aliveCount;
        return center;
    }

    private void OrientListOfCombatants(List<Combatant3D> teamToOrient, Vector3 targetPoint)
    {
        if (targetPoint == Vector3.zero && teamToOrient.Count > 0 && teamToOrient[0] != null) {
            // Fallback: se o ponto alvo for zero (ex: time oposto todo derrotado ou não spawnado), orienta para frente do spawn point
            // Ou simplesmente não faz nada se não há um ponto claro para olhar.
            // Por simplicidade, não faremos nada aqui, mas você pode adicionar uma lógica de orientação padrão.
            return;
        }

        foreach (Combatant3D member in teamToOrient)
        {
            if (member != null && member.IsAlive())
            {
                Vector3 directionToTarget = targetPoint - member.transform.position;
                directionToTarget.y = 0; 
                if (directionToTarget != Vector3.zero)
                {
                    member.transform.rotation = Quaternion.LookRotation(directionToTarget);
                }
            }
        }
    }

    private void ClearPreviousCombatants()
    {
        foreach (var combatant in _playerCombatants) { if (combatant != null && combatant.gameObject != null) Destroy(combatant.gameObject); }
        _playerCombatants.Clear();
        foreach (var combatant in _enemyCombatants) { if (combatant != null && combatant.gameObject != null) Destroy(combatant.gameObject); }
        _enemyCombatants.Clear();
    }


}