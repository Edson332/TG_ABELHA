// Scripts/GameSystems/InvasionScheduler.cs
using System.Collections.Generic;
using UnityEngine;

public class InvasionScheduler : MonoBehaviour
{
    public static InvasionScheduler Instancia { get; private set; }

    [Header("Configurações de Invasão")]
    public List<EnemyWaveSO> possibleInvasionWaves;
    public float minTimeBetweenInvasions = 120f;
    public float maxTimeBetweenInvasions = 300f;
    public float initialDelay = 60f;

    [Header("Referências da UI")]
    public CombatAlertUI combatAlertUI;

    // O campo de referência do tutorial foi REMOVIDO daqui

    private float _invasionTimer;
    private bool _schedulerActive = true;

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

    void Start()
    {
        if (combatAlertUI == null || possibleInvasionWaves == null || possibleInvasionWaves.Count == 0)
        {
            Debug.LogError("Configuração incompleta no InvasionScheduler! Desativando...");
            _schedulerActive = false;
            return;
        }
        _invasionTimer = initialDelay;
    }

    void Update()
    {
        if (!_schedulerActive || CombatManager.Instancia == null || CombatManager.Instancia.isCombatActive)
        {
            return;
        }

        _invasionTimer -= Time.deltaTime;
        if (_invasionTimer <= 0)
        {
            TriggerInvasion();
        }
    }

    void TriggerInvasion()
    {
        _schedulerActive = false; // Pausa o agendador para lidar com este evento

        int randomIndex = Random.Range(0, possibleInvasionWaves.Count);
        EnemyWaveSO selectedWave = possibleInvasionWaves[randomIndex];
        Debug.Log($"Agendador: Mostrando alerta de combate para a onda '{selectedWave.waveName}'");
        combatAlertUI.ShowAlert(selectedWave);
    }

    public void ResetTimer()
    {
        _invasionTimer = Random.Range(minTimeBetweenInvasions, maxTimeBetweenInvasions);
        Debug.Log($"Próxima checagem de invasão em {_invasionTimer:F0} segundos.");
    }

    public void AlertDecisionMade()
    {
        _schedulerActive = true;
        ResetTimer();
    }
}