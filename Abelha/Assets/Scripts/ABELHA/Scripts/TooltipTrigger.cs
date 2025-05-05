using UnityEngine;
using UnityEngine.EventSystems; // Required for IPointer handlers
using System.Collections; // Required for Coroutines

// Implementa as interfaces para detectar entrada e saída do ponteiro
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Content")]
    [Tooltip("The main body text for the tooltip.")]
    [SerializeField]
    [TextArea(3, 10)] // Melhora a visualização de textos longos no Inspector
    string mBodyText;

    [Tooltip("Optional header text for the tooltip.")]
    [SerializeField]
    string mHeaderText; // <<<--- NOVO: Texto do Header

    [Header("Behavior")]
    [Tooltip("Delay in seconds before the tooltip appears after hovering.")]
    [SerializeField]
    float showDelay = 1.0f; // <<<--- NOVO: Delay para mostrar

    private Coroutine showCoroutine; // Referência para a coroutine de exibição

    // Chamado quando o ponteiro do mouse entra na área do objeto com este script
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Cancela qualquer coroutine anterior que possa estar rodando (segurança)
        StopTooltipCoroutine();

        // Inicia a nova coroutine para mostrar o tooltip após o delay
        showCoroutine = StartCoroutine(ShowTooltipAfterDelay());
    }

    // Chamado quando o ponteiro do mouse sai da área do objeto
    public void OnPointerExit(PointerEventData eventData)
    {
        // Cancela a coroutine de exibição (se o mouse sair antes do delay)
        StopTooltipCoroutine();

        // Esconde o tooltip imediatamente
        // Verifica se Instance não é null antes de chamar (caso o tooltip seja destruído)
        if (Tooltip.Instance != null)
        {
            Tooltip.Instance.HideTooltip();
        }
    }

    // Coroutine que espera o delay e então mostra o tooltip
    private IEnumerator ShowTooltipAfterDelay()
    {
        // Espera pelo tempo definido em showDelay
        yield return new WaitForSeconds(showDelay);

        // Após o delay, mostra o tooltip com o header e o body
        // Verifica se Instance não é null antes de chamar
        if (Tooltip.Instance != null)
        {
            // Chama o SetTooltip atualizado, passando ambos os textos
            Tooltip.Instance.SetTooltip(mBodyText, mHeaderText);
        }

        // Reseta a referência da coroutine, pois ela terminou
        showCoroutine = null;
    }

    // Método auxiliar para parar a coroutine de forma segura
    private void StopTooltipCoroutine()
    {
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }
    }

    // Método para permitir a atualização do texto do tooltip via script (opcional)
    public void SetTooltipContent(string newBodyText, string newHeaderText = null)
    {
        mBodyText = newBodyText;
        mHeaderText = newHeaderText; // Atualiza o header também
    }

    // Garante que o tooltip seja escondido se o objeto for desativado/destruído
    void OnDisable()
    {
        StopTooltipCoroutine();
        if (Tooltip.Instance != null && Tooltip.Instance.gameObject.activeSelf) // Verifica se o tooltip está ativo
        {
             // Poderia verificar se este trigger é o que está mostrando o tooltip atualmente
             // para evitar esconder um tooltip de outro trigger, mas para simplificar:
             Tooltip.Instance.HideTooltip();
        }
    }
}
