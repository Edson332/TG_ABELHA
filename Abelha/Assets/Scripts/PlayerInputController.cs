using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic; // <--- ADICIONE ESTA LINHA

public class PlayerInputController : MonoBehaviour
{
    public LayerMask groundLayerMask; // Configurar no Inspector para a layer do chão/NavMesh clicável
    public Camera mainCamera;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Nenhuma câmera principal encontrada!");
                this.enabled = false;
            }
        }
    }

    void Update()
    {
        // Verifica clique esquerdo do mouse E se não está clicando sobre a UI
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
        {
            // Verifica se a Rainha existe na cena
            if (QueenBeeController.Instancia != null)
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Faz o Raycast apenas contra a layer do chão/NavMesh
                if (Physics.Raycast(ray, out hit, 1000f, groundLayerMask))
                {
                    // Envia o comando de mover para a Rainha
                    QueenBeeController.Instancia.MoveTo(hit.point);
                     // Opcional: Mostrar algum feedback visual no ponto clicado
                }
            }
             else
             {
                 // Opcional: Feedback para o jogador que a Rainha não está ativa/comprada
                 // Debug.Log("Clique detectado, mas a Abelha Rainha não está ativa.");
             }
        }
    }

    /// <summary>
    /// Verifica se o ponteiro do mouse está sobre um elemento de UI.
    /// Útil para evitar que cliques nos botões de UI movam a rainha.
    /// </summary>
    private bool IsPointerOverUIObject()
    {
        // Cria um objeto PointerEventData para o sistema de eventos atual
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        // Define a posição do evento como a posição atual do mouse
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        // Lista para armazenar os resultados do raycast da UI
        List<RaycastResult> results = new List<RaycastResult>(); // Agora o List<> será encontrado
        // Faz o raycast usando o sistema de eventos da UI
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        // Retorna true se houver algum resultado (ou seja, o clique foi em um elemento da UI)
        return results.Count > 0;
    }
}