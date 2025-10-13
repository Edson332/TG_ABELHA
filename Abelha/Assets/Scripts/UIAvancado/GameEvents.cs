// Scripts/GameEvents.cs
using UnityEngine;
using System; // Essencial para Action

public static class GameEvents
{
    // Evento que será disparado quando um recurso for coletado/produzido por uma abelha
    // Ele enviará: a posição da abelha, a quantidade, e o tipo de recurso.
    public static event Action<Transform, float, TipoRecurso> OnResourceCollectedByBee;

    // Método para disparar o evento de forma segura
    public static void ReportResourceCollected(Transform beeTransform, float amount, TipoRecurso type)
    {
        OnResourceCollectedByBee?.Invoke(beeTransform, amount, type);
    }
}