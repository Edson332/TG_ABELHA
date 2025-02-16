using UnityEngine;

public class HoneycombClick : MonoBehaviour
{
    public int nectarRequiredPerClick = 5; // Néctar necessário por clique
    public int honeyProducedPerClick = 1;  // Mel produzido por clique

    void OnMouseDown()
    {
        // Verifica se há néctar suficiente para a conversão
        if (ResourceManager.Instance.nectar >= nectarRequiredPerClick)
        {
            // Subtrai o néctar utilizado
            ResourceManager.Instance.nectar -= nectarRequiredPerClick;
            // Adiciona o mel carregado produzido
            ResourceManager.Instance.carriedHoney += honeyProducedPerClick;
            Debug.Log("Néctar convertido em mel carregado!");
            // Aqui você pode adicionar efeitos visuais ou sonoros
        }
        else
        {
            Debug.Log("Néctar insuficiente para conversão.");
            // Opcional: adicionar feedback visual ou sonoro para indicar falha
        }
    }
}
