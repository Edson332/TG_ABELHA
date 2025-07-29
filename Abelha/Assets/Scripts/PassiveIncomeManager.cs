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

        float finalIncomeThisFrame = 0f;

        // --- Cálculo da Renda Base ---
        // _totalBaseIncomePerSecond já está calculado, então usamos ele.
        float baseIncomeThisFrame = _totalBaseIncomePerSecond * Time.deltaTime;

        // --- Cálculo do Bônus da Rainha ---
        float queenBonusThisFrame = 0f;
        if (QueenBeeController.Instancia != null)
        {
            List<PassiveBee> boostedBees = QueenBeeController.Instancia.GetBoostedPassiveBees();
            float queenMultiplier = QueenBeeController.Instancia.nectarAmountMultiplier; // Usando o multiplicador de néctar

            if (boostedBees != null && boostedBees.Count > 0)
            {
                foreach (var bee in boostedBees)
                {
                    // O bônus é a renda extra (acima do normal)
                    float bonusAmount = bee.baseIncomePerSecond * (queenMultiplier - 1);
                    queenBonusThisFrame += bonusAmount * Time.deltaTime;
                }
            }
        }

        finalIncomeThisFrame = baseIncomeThisFrame + queenBonusThisFrame;

        // Adiciona a renda final ao total
        if (finalIncomeThisFrame > 0)
        {
            GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Mel, finalIncomeThisFrame);
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