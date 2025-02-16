using UnityEngine;

public class FlowerClick : MonoBehaviour
{
    public int nectarPerClick = 1; // Quantidade de néctar coletado por clique

    void OnMouseDown()
    {
        ResourceManager.Instance.AddNectar(nectarPerClick);
        Debug.Log("Néctar coletado!");
        // Aqui podemos adicionar efeitos visuais ou sonoros
    }
}
