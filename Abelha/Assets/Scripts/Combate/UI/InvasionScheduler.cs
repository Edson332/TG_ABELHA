// Scripts/GameSystems/InvasionScheduler.cs
using System.Collections.Generic;
using UnityEngine;

public class InvasionScheduler : MonoBehaviour
{
    public static InvasionScheduler Instancia { get; private set; }

    [Header("Configurações de Invasão")]
    public List<EnemyWaveSO> possibleInvasionWaves; // Arraste seus EnemyWaveSO aqui
    public float minTimeBetweenInvasions = 120f; // 2 minutos
    public float maxTimeBetweenInvasions = 300f; // 5 minutos
    public float initialDelay = 60f; // Atraso para a primeira invasão

    [Header("Referências da UI")]
    public CombatAlertUI combatAlertUI; // Arraste o GameObject com o script CombatAlertUI

    private float _invasionTimer;
    private bool _schedulerActive = true;

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
        if (combatAlertUI == null)
        {
            Debug.LogError("CombatAlertUI não atribuído ao InvasionScheduler!");
            _schedulerActive = false;
            return;
        }
        if (possibleInvasionWaves == null || possibleInvasionWaves.Count == 0)
        {
            Debug.LogError("Nenhuma EnemyWaveSO atribuída ao InvasionScheduler!");
            _schedulerActive = false;
            return;
        }
        _invasionTimer = initialDelay;
    }

    void Update()
    {
        if (!_schedulerActive || CombatManager.Instancia == null || CombatManager.Instancia.isCombatActive)
        {
            // Não agenda novas invasões se o scheduler estiver inativo,
            // o CombatManager não existir, ou se um combate já estiver ativo.
            return;
        }

        _invasionTimer -= Time.deltaTime;
        if (_invasionTimer <= 0)
        {
            TriggerInvasion();
            ResetTimer();
        }
    }

    void TriggerInvasion()
    {
        if (possibleInvasionWaves.Count > 0)
        {
            int randomIndex = Random.Range(0, possibleInvasionWaves.Count);
            EnemyWaveSO selectedWave = possibleInvasionWaves[randomIndex];
            Debug.Log($"Agendador: Disparando invasão com a onda '{selectedWave.waveName}'");
            combatAlertUI.ShowAlert(selectedWave);
            _schedulerActive = false; // Para de agendar até que o alerta seja resolvido
        }
    }

    public void ResetTimer()
    {
        _invasionTimer = Random.Range(minTimeBetweenInvasions, maxTimeBetweenInvasions);
        Debug.Log($"Próxima checagem de invasão em {_invasionTimer:F0} segundos.");
    }

    /// <summary>
    /// Chamado pela UI quando o jogador toma uma decisão sobre o alerta (aceita ou ignora).
    /// </summary>
    public void AlertDecisionMade()
    {
        _schedulerActive = true; // Permite que o scheduler continue após a decisão
        ResetTimer(); // Reseta o timer para a próxima invasão
    }
}