// Scripts/CombatSystem/CombatantBaseDataSO.cs
using UnityEngine;

public abstract class CombatantBaseDataSO : ScriptableObject
{
    [Header("Informações Básicas do Combatente")]
    public string combatantName = "Novo Combatente";
    public Sprite icon; // Para usar em UIs futuras (seleção, etc.)
    public GameObject modelPrefab3D; // O prefab 3D a ser instanciado

    [Header("Atributos de Combate Base")]
    public int baseMaxHP = 100;
    public int baseAttack = 10;

    [Header("Efeitos Visuais")]
    [Tooltip("O prefab do efeito de partícula a ser criado quando este combatente é atingido.")]
    public GameObject impactVFXPrefab; 

    [Tooltip("O prefab do efeito de partícula a ser criado quando este combatente morre.")]
    public GameObject deathVFXPrefab;
    // Adicione outros stats base comuns se necessário (ex: defesa, velocidade)
}