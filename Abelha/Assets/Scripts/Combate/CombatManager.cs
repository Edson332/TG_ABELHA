using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum CombatState
{
    NotActive, Preparing, PlayerTurn, EnemyTurn, Resolving, Finished
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instancia { get; private set; }

    [Header("Referências da Cena")]
    public Camera mainGameCamera;
    public Camera combatCamera;
    public CombatResultsUI combatResultsUI;
    
    [Header("Referências de Prefabs e UI")]
    public Transform worldSpaceCanvasTransform;
    public GameObject healthBarPrefab;
    public GameObject projectilePrefab;

    [Header("Dados dos Combatentes")]
    public List<PlayerBeeCombatDataSO> allPlayerBeeCombatData;

    [Header("Pontos de Spawn na Arena")]
    public List<Transform> playerSpawnPoints = new List<Transform>();
    public List<Transform> enemySpawnPoints = new List<Transform>();

    [Header("Configurações de Combate")]
    public EnemyWaveSO currentOperatingWave;
    public float actionDelayBetweenAttacks = 0.5f; 
    public float turnTransitionDelay = 1.0f;
    public float postCombatUIDelay = 2.0f;

    [Header("Dados de Teste (Opcional)")]
    public List<PlayerBeeCombatDataSO> testPlayerSquadData = new List<PlayerBeeCombatDataSO>();
    public EnemyWaveSO testWaveToUse;

    // Variável para a conquista
    public int fatorconquista = 0;

    private List<Combatant3D> _playerCombatants = new List<Combatant3D>();
    private List<Combatant3D> _enemyCombatants = new List<Combatant3D>();
    public CombatState currentCombatState { get; private set; } = CombatState.NotActive;
    public bool isCombatActive { get; private set; } = false;

    [Header("Efeitos Visuais de Fim de Combate")]
    [Tooltip("VFX a ser criado quando o jogador vence.")]
    public GameObject victoryVFXPrefab;
    [Tooltip("VFX a ser criado quando o jogador perde.")]
    public GameObject defeatVFXPrefab;
    [Tooltip("Ponto na arena onde o VFX de resultado irá aparecer.")]
    public Transform resultVFXSpawnPoint; // Opcional, pode ser o centro da arena

    private GameObject _currentResultVFXInstance;

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    void Start()
    {
        if (combatCamera != null) combatCamera.gameObject.SetActive(false);
        if (mainGameCamera != null) mainGameCamera.gameObject.SetActive(true);
    }
    
    public void Test_StartCombatWithInspectorData()
    {
        if (testWaveToUse != null && testPlayerSquadData.Count > 0)
        {
           StartNewCombat(testWaveToUse, testPlayerSquadData);
        }
        else
        {
            Debug.LogError("Dados de teste não configurados no CombatManager Inspector.");
        }
    }

    public void StartNewCombat(EnemyWaveSO enemyWave, List<PlayerBeeCombatDataSO> playerSelectedBees)
    {
        if (isCombatActive) return;

        currentOperatingWave = enemyWave;
        currentCombatState = CombatState.Preparing;
        isCombatActive = true;
        //fatorconquista = 0;
         AudioManager.Instancia.PlayMusic("CombatBGM");

        if (mainGameCamera != null) mainGameCamera.gameObject.SetActive(false);
        if (combatCamera != null) combatCamera.gameObject.SetActive(true);

        ClearPreviousCombatants();
        SpawnPlayerTeam(playerSelectedBees, playerSpawnPoints);
        SpawnEnemyTeam(enemyWave, enemySpawnPoints);

        if (_playerCombatants.Count == 0) { ProcessCombatResult(false); return; }
        if (_enemyCombatants.Count == 0) { ProcessCombatResult(true); return; }

        OrientTeams();
        StartCoroutine(CombatLoopCoroutine());
    }

    private IEnumerator CombatLoopCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        currentCombatState = CombatState.PlayerTurn; 

        while (isCombatActive && AreAnyPlayerBeesAlive() && AreAnyEnemiesAlive())
        {
            switch (currentCombatState)
            {
                case CombatState.PlayerTurn:
                    yield return StartCoroutine(ExecuteTurn(_playerCombatants, _enemyCombatants));
                    if (!AreAnyEnemiesAlive()) break; 
                    currentCombatState = CombatState.EnemyTurn;
                    yield return new WaitForSeconds(turnTransitionDelay);
                    break;
                case CombatState.EnemyTurn:
                    yield return StartCoroutine(ExecuteTurn(_enemyCombatants, _playerCombatants));
                    if (!AreAnyPlayerBeesAlive()) break; 
                    currentCombatState = CombatState.PlayerTurn;
                    yield return new WaitForSeconds(turnTransitionDelay);
                    break;
            }
        }
        if (isCombatActive) { ProcessCombatResult(AreAnyPlayerBeesAlive()); }
    }

    private IEnumerator ExecuteTurn(List<Combatant3D> attackers, List<Combatant3D> defenders)
    {
        foreach (var attacker in attackers)
        {
            if (attacker.IsAlive())
            {
                Combatant3D target = FindTargetFor(attacker, defenders);
                if (target != null)
                {
                    attacker.PerformAttackAction(target);
                    
                    if (projectilePrefab != null)
                    {
                        GameObject projGO = Instantiate(projectilePrefab, attacker.transform.position, Quaternion.identity);
                        Projectile proj = projGO.GetComponent<Projectile>();
                        if (proj != null)
                        {
                            proj.target = target;
                            proj.damage = attacker.attackPower;
                            proj.attacker = attacker.transform;
                        }
                    }
                    else
                    {
                        target.TakeDamage(attacker.attackPower, attacker.transform);
                    }
                    
                    yield return new WaitForSeconds(actionDelayBetweenAttacks); 

                    if (!AreAnyEnemiesAlive() || !AreAnyPlayerBeesAlive()) yield break;
                }
            }
        }
    }
    
    private void ProcessCombatResult(bool playerWon)
    {
        currentCombatState = CombatState.Resolving;
        //isCombatActive = false;

        Debug.Log(playerWon ? "Jogador VENCEU o combate!" : "Jogador foi DERROTADO.");

        if (_currentResultVFXInstance != null)
        {
            Destroy(_currentResultVFXInstance);
        }

        Vector3 spawnPos = resultVFXSpawnPoint != null ? resultVFXSpawnPoint.position : Vector3.zero;

        if (playerWon && victoryVFXPrefab != null)
        {
            _currentResultVFXInstance = Instantiate(victoryVFXPrefab, spawnPos, Quaternion.identity);
        }
        else if (!playerWon && defeatVFXPrefab != null)
        {
            _currentResultVFXInstance = Instantiate(defeatVFXPrefab, spawnPos, Quaternion.identity);
        }
        
        if (playerWon)
        {
            fatorconquista += 1; // Incrementa a variável
            if (currentOperatingWave != null && GerenciadorRecursos.Instancia != null)
            {
                GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Mel, currentOperatingWave.honeyReward);
                GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.GeleiaReal, currentOperatingWave.royalJellyReward);
            }
        }
        
        if (combatResultsUI != null)
        {
            combatResultsUI.ShowResults(playerWon, currentOperatingWave);
        }
        else
        {
            StartCoroutine(CleanupCombatSequence());
        }
    }
    
    public void ConcludeCombatAfterResults()
    {
        StartCoroutine(CleanupCombatSequence());
    }

    private IEnumerator CleanupCombatSequence()
    {

        if (_currentResultVFXInstance != null)
        {
            Destroy(_currentResultVFXInstance);
            _currentResultVFXInstance = null; // Limpa a referência
        }
        yield return new WaitForSeconds(postCombatUIDelay);

        ClearPreviousCombatants(); 

        if (combatCamera != null) combatCamera.gameObject.SetActive(false);
        if (mainGameCamera != null) mainGameCamera.gameObject.SetActive(true);

        AudioManager.Instancia.PlayMusic("MainBGM");
        isCombatActive = false;
        currentCombatState = CombatState.Finished; 

        if (InvasionScheduler.Instancia != null)
        {
            InvasionScheduler.Instancia.AlertDecisionMade();
        }
    }

    // --- MÉTODOS DE SPAWN E AUXILIARES COMPLETOS ---

    private void SpawnPlayerTeam(List<PlayerBeeCombatDataSO> playerTeamData, List<Transform> spawnPointList)
    {
        int spawnIndex = 0;
        foreach (var dataSO in playerTeamData)
        {
            if (spawnIndex >= spawnPointList.Count) break;
            if (dataSO.modelPrefab3D == null) continue;

            Transform spawnPoint = spawnPointList[spawnIndex];
            GameObject instance = Instantiate(dataSO.modelPrefab3D, spawnPoint.position, spawnPoint.rotation);
            Combatant3D combatantScript = instance.GetComponent<Combatant3D>();
            if (combatantScript == null) { Destroy(instance); continue; }
            
            int finalMaxHP = dataSO.baseMaxHP;
            int finalAttack = dataSO.baseAttack;
            if (GerenciadorUpgrades.Instancia != null)
            {
                float hpMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(dataSO.beeTypeNameForUpgrades, TipoUpgrade.VidaCombate);
                float attackMultiplier = GerenciadorUpgrades.Instancia.GetMultiplier(dataSO.beeTypeNameForUpgrades, TipoUpgrade.AtaqueCombate);
                finalMaxHP = Mathf.RoundToInt(dataSO.baseMaxHP * hpMultiplier);
                finalAttack = Mathf.RoundToInt(dataSO.baseAttack * attackMultiplier);
            }
            if (RoyalJellyShopManager.Instancia != null)
            {
                float globalHPBonus = RoyalJellyShopManager.Instancia.GetGlobalCombatHPBonus();
                float globalAttackBonus = RoyalJellyShopManager.Instancia.GetGlobalCombatAttackBonus();

                finalMaxHP = Mathf.RoundToInt(finalMaxHP * (1f + globalHPBonus));
                finalAttack = Mathf.RoundToInt(finalAttack * (1f + globalAttackBonus));
            }
            InitializeCombatant(combatantScript, dataSO, finalMaxHP, finalAttack, true);
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
                if (spawnIndex >= spawnPointList.Count) break;
                if (group.enemyData.modelPrefab3D == null) continue;

                Transform spawnPoint = spawnPointList[spawnIndex];
                GameObject instance = Instantiate(group.enemyData.modelPrefab3D, spawnPoint.position, spawnPoint.rotation);
                Combatant3D combatantScript = instance.GetComponent<Combatant3D>();
                if (combatantScript == null) { Destroy(instance); continue; }
                
                InitializeCombatant(combatantScript, group.enemyData, group.enemyData.baseMaxHP, group.enemyData.baseAttack, false);
                _enemyCombatants.Add(combatantScript);
                spawnIndex++;
            }
            if (spawnIndex >= spawnPointList.Count) break; 
        }
    }

    private void InitializeCombatant(Combatant3D script, CombatantBaseDataSO data, int maxHp, int attack, bool isPlayer)
    {
        Slider slider = null; 
        GameObject healthBarGO = null;
        if (healthBarPrefab != null && worldSpaceCanvasTransform != null)
        {
            healthBarGO = Instantiate(healthBarPrefab, worldSpaceCanvasTransform);
            slider = healthBarGO.GetComponent<Slider>();
        }
        script.Initialize(data.combatantName, maxHp, attack, isPlayer, slider, healthBarGO, combatCamera, worldSpaceCanvasTransform, data.impactVFXPrefab, data.deathVFXPrefab);
    }

    private void OrientTeams()
    {
        if (_playerCombatants.Count == 0 || _enemyCombatants.Count == 0) return;
        Vector3 enemyCenter = GetTeamCenter(_enemyCombatants);
        Vector3 playerCenter = GetTeamCenter(_playerCombatants);
        OrientListOfCombatants(_playerCombatants, enemyCenter);
        OrientListOfCombatants(_enemyCombatants, playerCenter);
    }

    private Vector3 GetTeamCenter(List<Combatant3D> team)
    {
        Vector3 center = Vector3.zero;
        int aliveCount = 0;
        foreach (var member in team)
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
        foreach (var member in teamToOrient)
        {
            if (member != null && member.IsAlive())
            {
                Vector3 direction = targetPoint - member.transform.position;
                direction.y = 0; 
                if (direction != Vector3.zero)
                {
                    member.transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
    }
    
    private Combatant3D FindTargetFor(Combatant3D attacker, List<Combatant3D> potentialTargets)
    {
        return potentialTargets.FirstOrDefault(t => t != null && t.IsAlive());
    }

    private bool AreAnyPlayerBeesAlive()
    {
        return _playerCombatants.Any(b => b != null && b.IsAlive());
    }

    private bool AreAnyEnemiesAlive()
    {
        return _enemyCombatants.Any(e => e != null && e.IsAlive());
    }

 private void ClearPreviousCombatants()
    {
        // Limpa o time do jogador
        foreach (var combatant in _playerCombatants)
        {
            if (combatant != null)
            {
                // <<<--- CORREÇÃO ADICIONADA AQUI ---
                // Primeiro, destrói a barra de vida da UI associada a este combatente
                if (combatant.healthBarCanvasElement != null)
                {
                    Destroy(combatant.healthBarCanvasElement);
                }
                // --- FIM DA CORREÇÃO ---

                // Então, destrói o GameObject da abelha 3D
                Destroy(combatant.gameObject);
            }
        }
        _playerCombatants.Clear();

        // Limpa o time inimigo (mesma lógica)
        foreach (var combatant in _enemyCombatants)
        {
            if (combatant != null)
            {
                // <<<--- CORREÇÃO ADICIONADA AQUI ---
                if (combatant.healthBarCanvasElement != null)
                {
                    Destroy(combatant.healthBarCanvasElement);
                }
                // --- FIM DA CORREÇÃO ---

                Destroy(combatant.gameObject);
            }
        }
        _enemyCombatants.Clear();
    }
}