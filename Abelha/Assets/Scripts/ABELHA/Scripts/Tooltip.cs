// Scripts/UI/Tooltip.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance;

    [Header("Components")]
    [SerializeField]
    private TextMeshProUGUI mBodyText;
    [SerializeField]
    private TextMeshProUGUI mHeaderText;
    
    [Header("Positioning")]
    [SerializeField] 
    private float cursorGap = 15f;

    private RectTransform mRectTransform;
    private bool mActive;
    private Canvas mParentCanvas;
    
    // --- ADICIONADO PARA OTIMIZAÇÃO ---
    // Guarda o texto atual para evitar recálculos desnecessários
    private string _currentBodyText;
    private string _currentHeaderText;
    // --- FIM DA ADIÇÃO ---

    void Awake()
    {
        // ... (código do Awake existente) ...
        if (Instance == null) { Instance = this; } else if (Instance != this) { Destroy(gameObject); return; }
        mRectTransform = GetComponent<RectTransform>();
        if (mHeaderText != null && mHeaderText.gameObject.activeSelf) { mHeaderText.gameObject.SetActive(false); }
        gameObject.SetActive(false);
    }

    void Start()
    {
        // ... (código do Start existente) ...
        mParentCanvas = GetComponentInParent<Canvas>();
        if (mParentCanvas == null || mParentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogError("Tooltip requires a parent Canvas in ScreenSpaceOverlay mode!", this);
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Define e mostra o tooltip. Agora é otimizado para não redesenhar se o texto for o mesmo.
    /// </summary>
    public void SetTooltip(string aBodyText, string aHeaderText = null)
    {
        if (string.IsNullOrEmpty(aBodyText))
        {
            HideTooltip();
            return;
        }

        // --- LÓGICA DE OTIMIZAÇÃO ADICIONADA ---
        // Se o tooltip já estiver ativo e o texto for o mesmo, não faz nada.
        if (mActive && aBodyText == _currentBodyText && aHeaderText == _currentHeaderText)
        {
            return;
        }
        // --- FIM DA LÓGICA DE OTIMIZAÇÃO ---

        // Armazena o novo texto
        _currentBodyText = aBodyText;
        _currentHeaderText = aHeaderText;

        if (!mActive)
        {
            gameObject.SetActive(true);
            mActive = true;
        }

        bool useHeader = mHeaderText != null && !string.IsNullOrEmpty(aHeaderText);
        
        if (mHeaderText != null)
        {
            mHeaderText.gameObject.SetActive(useHeader);
            if (useHeader) mHeaderText.text = aHeaderText;
        }

        mBodyText.text = aBodyText;
        
        UpdatePosition();
    }

    public void HideTooltip()
    {
        if (mActive)
        {
            _currentBodyText = null; // Limpa o texto cacheado
            _currentHeaderText = null;
            gameObject.SetActive(false);
            mActive = false;
        }
    }

    void LateUpdate()
    {
        if (mActive)
        {
            UpdatePosition();
        }
    }

    void UpdatePosition()
    {
        // ... (código do UpdatePosition existente, sem alterações) ...
        if (mParentCanvas == null) return;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle( mParentCanvas.transform as RectTransform, Input.mousePosition, mParentCanvas.worldCamera, out localPoint);
        float tooltipWidth = mRectTransform.rect.width;
        float tooltipHeight = mRectTransform.rect.height;
        float screenWidth = mParentCanvas.GetComponent<RectTransform>().rect.width;
        float screenHeight = mParentCanvas.GetComponent<RectTransform>().rect.height;
        float pivotX = (Input.mousePosition.x / screenWidth) < 0.5f ? 0 : 1;
        float pivotY = (Input.mousePosition.y / screenHeight) < 0.5f ? 0 : 1;
        mRectTransform.pivot = new Vector2(pivotX, pivotY);
        float gapX = pivotX == 0 ? cursorGap : -cursorGap;
        float gapY = pivotY == 0 ? cursorGap : -cursorGap;
        Vector3 newPosition = localPoint + new Vector2(gapX, gapY);
        float minX = (-screenWidth / 2f) + (pivotX * tooltipWidth);
        float maxX = (screenWidth / 2f) - ((1 - pivotX) * tooltipWidth);
        float minY = (-screenHeight / 2f) + (pivotY * tooltipHeight);
        float maxY = (screenHeight / 2f) - ((1 - pivotY) * tooltipHeight);
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        transform.localPosition = newPosition;
    }
}