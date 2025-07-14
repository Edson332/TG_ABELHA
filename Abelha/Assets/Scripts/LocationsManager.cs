
using UnityEngine;

public class LocationsManager : MonoBehaviour
{
    // Padrão Singleton para acesso global fácil
    public static LocationsManager Instancia { get; private set; }

    [Header("Destinos das Abelhas")]
    public Transform flowerTarget;    // Ponto de coleta de néctar
    public Transform honeycombTarget; // Ponto de processamento
    public Transform hiveTarget;      // Ponto de depósito final

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;

        // Validação para garantir que os alvos foram configurados no Inspector
        if (flowerTarget == null || honeycombTarget == null || hiveTarget == null)
        {
            Debug.LogError("Um ou mais alvos de destino não foram atribuídos no LocationsManager! As abelhas não saberão para onde ir.");
        }
    }
}
