// Scripts/UI/TooltipTrigger.cs
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Content (Default)")]
    [TextArea(3, 10)]
    [SerializeField] string mBodyText;
    [SerializeField] string mHeaderText;
    
    [Header("Behavior")]
    [SerializeField] float showDelay = 0.5f;
    
    private Coroutine _showCoroutine;
    private bool _isHovering = false; // --- ADICIONADO --- Flag para saber se o mouse está em cima

    // --- NOVO MÉTODO UPDATE ---
    void Update()
    {
        // Se o mouse estiver sobre este objeto e o tooltip já estiver visível,
        // força a atualização.
        if (_isHovering && Tooltip.Instance != null && Tooltip.Instance.gameObject.activeSelf)
        {
            Tooltip.Instance.SetTooltip(mBodyText, mHeaderText);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovering = true; // --- ADICIONADO ---
        StopTooltipCoroutine();
        _showCoroutine = StartCoroutine(ShowTooltipAfterDelay());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false; // --- ADICIONADO ---
        StopTooltipCoroutine();
        if (Tooltip.Instance != null)
        {
            Tooltip.Instance.HideTooltip();
        }
    }

    private IEnumerator ShowTooltipAfterDelay()
    {
        yield return new WaitForSecondsRealtime(showDelay);

        // A verificação _isHovering garante que o tooltip não apareça se o mouse
        // já tiver saído durante o delay.
        if (_isHovering && Tooltip.Instance != null)
        {
            Tooltip.Instance.SetTooltip(mBodyText, mHeaderText);
        }
        _showCoroutine = null;
    }

    private void StopTooltipCoroutine()
    {
        if (_showCoroutine != null)
        {
            StopCoroutine(_showCoroutine);
            _showCoroutine = null;
        }
    }
    
    public void SetTooltipContent(string newBodyText, string newHeaderText = "")
    {
        mBodyText = newBodyText;
        mHeaderText = newHeaderText;
    }

    void OnDisable()
    {
        // Garante que tudo seja resetado se o objeto for desativado
        _isHovering = false; 
        StopTooltipCoroutine();
        if (Tooltip.Instance != null && Tooltip.Instance.gameObject.activeSelf)
        {
            Tooltip.Instance.HideTooltip();
        }
    }
}