// Scripts/Managers/BeeVisualsManager.cs
using System.Collections.Generic;
using UnityEngine;

public class BeeVisualsManager : MonoBehaviour
{
    public static BeeVisualsManager Instancia { get; private set; }

    // Dicionário para agrupar as abelhas por tipo
    private Dictionary<string, List<PassiveBee>> _passiveBeesByType = new Dictionary<string, List<PassiveBee>>();
    
    // O estado atual da configuração de visibilidade
    private bool _showAllBees = true;

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
    }
    
    /// <summary>
    /// Chamado pela UI (um botão Toggle) para mudar o modo de visualização.
    /// </summary>
    public void SetVisualsMode(bool showAll)
    {
        _showAllBees = showAll;
        Debug.Log($"Modo de visualização de abelhas alterado para: {(showAll ? "Mostrar Todas" : "Otimizado")}");
        ApplyCurrentVisualsMode();
    }

    /// <summary>
    /// Aplica a regra de visibilidade atual a todas as abelhas registradas.
    /// </summary>
    private void ApplyCurrentVisualsMode()
    {
        // Itera sobre cada tipo de abelha que temos
        foreach (var beeList in _passiveBeesByType.Values)
        {
            if (_showAllBees)
            {
                // Se for para mostrar todas, ativa os visuais de todas as abelhas da lista
                foreach (var bee in beeList)
                {
                    bee.SetVisualsActive(true);
                }
            }
            else
            {
                // Se for o modo otimizado, mostra apenas a primeira da lista e esconde as outras
                for (int i = 0; i < beeList.Count; i++)
                {
                    // Mostra a primeira (índice 0), esconde as demais
                    beeList[i].SetVisualsActive(i == 0);
                }
            }
        }
    }

    /// <summary>
    /// Chamado por cada PassiveBee quando ela é criada/ativada.
    /// </summary>
    public void RegisterPassiveBee(PassiveBee bee)
    {
        string beeType = bee.beeType;

        // Se este é o primeiro registro para este tipo de abelha, cria a lista
        if (!_passiveBeesByType.ContainsKey(beeType))
        {
            _passiveBeesByType[beeType] = new List<PassiveBee>();
        }

        // Adiciona a abelha à lista de seu tipo
        if (!_passiveBeesByType[beeType].Contains(bee))
        {
            _passiveBeesByType[beeType].Add(bee);
            // Aplica a regra de visibilidade a esta abelha recém-adicionada
            bee.SetVisualsActive(_showAllBees || _passiveBeesByType[beeType].Count == 1);
        }
    }

    /// <summary>
    /// Chamado por cada PassiveBee quando ela é desativada/destruída.
    /// </summary>
    public void UnregisterPassiveBee(PassiveBee bee)
    {
        string beeType = bee.beeType;

        if (_passiveBeesByType.ContainsKey(beeType))
        {
            _passiveBeesByType[beeType].Remove(bee);
            // Se a lista ficar vazia, remove a chave
            if (_passiveBeesByType[beeType].Count == 0)
            {
                _passiveBeesByType.Remove(beeType);
            }
            else
            {
                // Após remover uma abelha, é importante re-aplicar a regra
                // para garantir que uma nova "primeira" abelha se torne visível
                // se estivermos no modo otimizado.
                ApplyCurrentVisualsMode();
            }
        }
    }
}