// Scripts/Managers/LocationsManager.cs
using UnityEngine;
using System.Collections.Generic;

public class LocationsManager : MonoBehaviour
{
    public static LocationsManager Instancia { get; private set; }

    [Header("Destinos das Abelhas")]
    [Tooltip("Lista de todos os possíveis alvos de flores na cena.")]
    public List<Transform> flowerTargets;
    
    [Tooltip("Ponto de processamento.")]
    public Transform honeycombTarget;
    
    [Tooltip("Ponto de depósito final.")]
    public Transform hiveTarget;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;

        if (flowerTargets == null || flowerTargets.Count == 0 || honeycombTarget == null || hiveTarget == null)
        {
            Debug.LogError("Um ou mais alvos de destino não foram atribuídos no LocationsManager! Verifique a lista de flores.", this);
        }
    }

    /// <summary>
    /// Retorna uma flor aleatória da lista de alvos.
    /// </summary>
    public Transform GetRandomFlowerTarget()
    {
        if (flowerTargets == null || flowerTargets.Count == 0)
        {
            Debug.LogError("Nenhuma flor alvo definida no LocationsManager!");
            return null;
        }
        int randomIndex = Random.Range(0, flowerTargets.Count);
        return flowerTargets[randomIndex];
    }
}