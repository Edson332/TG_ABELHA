using UnityEngine;

public class FlowerClick : MonoBehaviour
{
    public int nectarPerClick = 1; // Quantidade de néctar coletado por clique

    void OnMouseDown()
    {
        if (GerenciadorRecursos.Instancia != null)
        {
            GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Nectar, nectarPerClick);
            Debug.Log("Néctar coletado!");
            // Aqui você pode adicionar efeitos visuais ou sonoros
        }
    }
}

