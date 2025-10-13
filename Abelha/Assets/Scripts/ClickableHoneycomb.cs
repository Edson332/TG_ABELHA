// Scripts/ClickableHoneycomb.cs
using UnityEngine;

// O objeto precisa de um Collider para detectar cliques
[RequireComponent(typeof(Collider))]
public class ClickableHoneycomb : MonoBehaviour
{
    [Header("Configuração do Clique")]
    [Tooltip("A quantidade de Mel gerada por cada clique no favo.")]
    [Range(0.01f, 0.5f)] // Limita o valor no Inspector para evitar valores muito altos
    public float honeyPerClick = 0.1f; // Valor padrão, pode ser alterado no Inspector

    private void OnMouseDown()
    {
        // OnMouseDown é chamado quando o botão do mouse é pressionado sobre este Collider
        // Certifica-se de que não estamos clicando na UI ao mesmo tempo (para jogos 3D)
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return; // Clique na UI, ignora
        }

        // Garante que o jogo não esteja pausado
        if (Time.timeScale == 0f) return;

        if (GerenciadorRecursos.Instancia != null)
        {
            GerenciadorRecursos.Instancia.AdicionarRecurso(TipoRecurso.Mel, honeyPerClick);
            Debug.Log($"Mel adicionado via clique no favo: {honeyPerClick} Mel.");
            GameEvents.ReportResourceCollected(transform, honeyPerClick, TipoRecurso.Mel);

            // Opcional: Adicionar feedback visual (texto flutuante como o do dano)
            // ou sonoro (AudioManager.Instancia.PlaySFX("HoneycombClick")) aqui.
        }
    }
}