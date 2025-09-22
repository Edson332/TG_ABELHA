// Scripts/Managers/PassiveIncomeManager.cs
using System.Collections.Generic;
using UnityEngine;

public class PassiveIncomeManager : MonoBehaviour
{
    public static PassiveIncomeManager Instancia { get; private set; }

    // Lista de todas as abelhas passivas ativas na cena
    private List<PassiveBee> _activePassiveBees = new List<PassiveBee>();

    private float _totalBaseIncomePerSecond = 0f;

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }

     void Update()
    {
        if (_activePassiveBees.Count == 0) return;

        float totalIncomeThisFrame = 0f;
        float queenMultiplier = 1f; // Padrão é 1x (sem bônus)

        // Pega o multiplicador da rainha, se ela existir
        if (QueenBeeController.Instancia != null)
        {
            queenMultiplier = QueenBeeController.Instancia.nectarAmountMultiplier;
        }

        // Itera por todas as abelhas passivas ativas
        foreach (var bee in _activePassiveBees)
        {
            float currentBeeIncome = bee.baseIncomePerSecond;

            // Verifica se a abelha está perto da rainha para aplicar o bônus
            if (QueenBeeController.Instancia != null)
            {
                float distanceToQueen = Vector3.Distance(bee.transform.position, QueenBeeController.Instancia.transform.position);
                if (distanceToQueen <= QueenBeeController.Instancia.auraRadius)
                {
                    // Está dentro da aura, aplica o multiplicador
                    currentBeeIncome *= queenMultiplier;
                }
            }
            totalIncomeThisFrame += currentBeeIncome * Time.deltaTime;
        }
        
        if (totalIncomeThisFrame > 0)
        {
            GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Mel, totalIncomeThisFrame);
        }
    }

    public void RegisterPassiveBee(PassiveBee bee)
    {
        if (!_activePassiveBees.Contains(bee))
        {
            _activePassiveBees.Add(bee);
            RecalculateTotalBaseIncome();
        }
    }

    public void UnregisterPassiveBee(PassiveBee bee)
    {
        if (_activePassiveBees.Contains(bee))
        {
            _activePassiveBees.Remove(bee);
            RecalculateTotalBaseIncome();
        }
    }

    private void RecalculateTotalBaseIncome()
    {
        _totalBaseIncomePerSecond = 0f;
        foreach (var bee in _activePassiveBees)
        {
            _totalBaseIncomePerSecond += bee.baseIncomePerSecond;
        }
        Debug.Log($"Renda Passiva Base recalculada: {_totalBaseIncomePerSecond}/s");
    }
}