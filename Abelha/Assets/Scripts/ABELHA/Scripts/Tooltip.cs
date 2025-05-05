using UnityEngine;
using UnityEngine.UI; // Required for Text, LayoutRebuilder
using System.Collections; // Required for Coroutines (though delay is handled in Trigger)
using System;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance;

    // --- Inspector Configurables ---
    [Header("Components")]
    [Tooltip("Assign the child Text component for the main body text.")]
    [SerializeField]
    Text mBodyText; // Renomeado de mText para clareza

    [Tooltip("Assign the child Text component for the header text (Optional). Leave empty if no header is used.")]
    [SerializeField]
    Text mHeaderText; // <<<--- NOVO: Referência para o Texto do Header

    [Tooltip("Assign the child Corner Image RectTransform here (Optional).")]
    [SerializeField]
    RectTransform mCornerImage;

    [Header("Layout Settings")]
    [Tooltip("Default maximum width for text wrapping (in pixels). Set to 0 or less for no default wrapping.")]
    [SerializeField]
    float defaultMaxWidth = 300f;

    [Tooltip("Horizontal padding between text and background edges (applied to both sides).")]
    [SerializeField]
    float horizontalPadding = 30f;

    [Tooltip("Vertical padding between text and background edges (applied to top and bottom).")]
    [SerializeField]
    float verticalPadding = 15f;

    [Tooltip("Additional vertical spacing between the header and body text when a header is present.")]
    [SerializeField]
    float headerSpacing = 5f; // <<<--- NOVO: Espaçamento entre header e body

    [Header("Positioning")]
     [Tooltip("Small gap between the mouse cursor and the tooltip edge.")]
    [SerializeField] float cursorGap = 10f;
    // --- End Inspector Configurables ---


    // --- Internal State ---
    RectTransform mRectTransform;
    RectTransform mBodyTextRectTransform; // Cache do RectTransform do body
    RectTransform mHeaderTextRectTransform; // Cache do RectTransform do header (se existir)

    bool mActive;
    float mWidth;
    float mHeight;
    RenderMode mGUIMode;
    Canvas mParentCanvas;
    CanvasScaler mScaler;
    // --- End Internal State ---

    void Awake()
    {
        // --- Singleton Setup ---
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning($"Duplicate Tooltip instance found on {gameObject.name}, destroying this one.", gameObject);
            Destroy(gameObject);
            return;
        }
        // --- End Singleton Setup ---

        mRectTransform = transform.GetComponent<RectTransform>();
        if (mRectTransform == null) Debug.LogError("Tooltip GameObject requires a RectTransform component!", this);

        // Get Body Text component
        if (mBodyText == null) mBodyText = GetComponentInChildren<Text>(true); // Procura se não atribuído
        if (mBodyText == null) Debug.LogError("Tooltip requires a child Text component for the body. Please assign 'M Body Text'.", this);
        else mBodyTextRectTransform = mBodyText.GetComponent<RectTransform>();

        // Get Header Text component (Optional)
        if (mHeaderText != null)
        {
            mHeaderTextRectTransform = mHeaderText.GetComponent<RectTransform>();
            if (mHeaderTextRectTransform == null) Debug.LogError("Tooltip's Header Text component requires a RectTransform!", mHeaderText);
            // Garante que o header comece desativado se o componente existir mas não for usado inicialmente
             if (mHeaderText.gameObject.activeSelf) mHeaderText.gameObject.SetActive(false);
        }
        // else: No header component assigned, which is fine.

        // Corner image is optional
        if (mCornerImage == null)
        {
            Transform cornerTransform = transform.Find("Corner");
            if (cornerTransform != null) mCornerImage = cornerTransform.GetComponent<RectTransform>();
        }
    }

    void Start()
    {
        mParentCanvas = GetComponentInParent<Canvas>();
        if (mParentCanvas != null)
        {
            mGUIMode = mParentCanvas.renderMode;
            mScaler = mParentCanvas.GetComponent<CanvasScaler>();
        }
        else
        {
            Debug.LogError("Tooltip must be placed within a Canvas hierarchy!", this);
            this.enabled = false;
            if(mBodyText != null) mBodyText.gameObject.SetActive(false);
            if(mHeaderText != null) mHeaderText.gameObject.SetActive(false);
            gameObject.SetActive(false);
            return;
        }
        HideTooltip(); // Ensure it starts hidden
    }

    /// <summary>
    /// Shows the tooltip with body text and an optional header.
    /// </summary>
    /// <param name="aBodyText">The main text content.</param>
    /// <param name="aHeaderText">Optional header text. Pass null or empty string for no header.</param>
    /// <param name="aMaxWidthOverride">Optional max width override.</param>
    public void SetTooltip(string aBodyText, string aHeaderText = null, float aMaxWidthOverride = 0)
    {
        // --- Pre-conditions ---
        if (mGUIMode != RenderMode.ScreenSpaceOverlay || string.IsNullOrEmpty(aBodyText) ||
            mBodyText == null || mRectTransform == null || mBodyTextRectTransform == null)
        {
            if (mActive) HideTooltip();
            return;
        }
        // --- End Pre-conditions ---

        // --- Header Handling ---
        bool useHeader = mHeaderText != null && mHeaderTextRectTransform != null && !string.IsNullOrEmpty(aHeaderText);
        float headerPreferredHeight = 0f;

        if (useHeader)
        {
            mHeaderText.gameObject.SetActive(true);
            mHeaderText.text = aHeaderText;
            // Configura o header para usar a mesma largura máxima (ou sem limite se body não tiver)
            float actualMaxWidthForHeader = (aMaxWidthOverride > 0) ? aMaxWidthOverride : defaultMaxWidth;
             if(actualMaxWidthForHeader > 0)
             {
                mHeaderText.horizontalOverflow = HorizontalWrapMode.Wrap;
                mHeaderTextRectTransform.sizeDelta = new Vector2(actualMaxWidthForHeader, mHeaderTextRectTransform.sizeDelta.y);
             }
             else
             {
                mHeaderText.horizontalOverflow = HorizontalWrapMode.Overflow;
                // Poderia resetar a largura aqui se necessário
             }
            LayoutRebuilder.ForceRebuildLayoutImmediate(mHeaderTextRectTransform);
            headerPreferredHeight = mHeaderText.preferredHeight;
        }
        else if (mHeaderText != null) // Garante que o header esteja desativado se não for usado
        {
            mHeaderText.gameObject.SetActive(false);
        }
        // --- End Header Handling ---

        // --- Body Text Handling & Sizing ---
        mBodyText.text = aBodyText;
        float actualMaxWidth = (aMaxWidthOverride > 0) ? aMaxWidthOverride : defaultMaxWidth;
        float bodyPreferredHeight = 0f;
        float contentWidth = 0f; // Largura do conteúdo (texto)

        if (actualMaxWidth > 0)
        {
            // Constrained Width
            mBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
            mBodyTextRectTransform.sizeDelta = new Vector2(actualMaxWidth, mBodyTextRectTransform.sizeDelta.y);
            LayoutRebuilder.ForceRebuildLayoutImmediate(mBodyTextRectTransform);
            bodyPreferredHeight = mBodyText.preferredHeight;
            contentWidth = actualMaxWidth; // Largura é a máxima definida
        }
        else
        {
            // Unconstrained Width
            mBodyText.horizontalOverflow = HorizontalWrapMode.Overflow;
            LayoutRebuilder.ForceRebuildLayoutImmediate(mBodyTextRectTransform); // Garante cálculo correto
            bodyPreferredHeight = mBodyText.preferredHeight;
            // A largura do conteúdo é a maior entre o header (se houver) e o body
            contentWidth = mBodyText.preferredWidth;
            if(useHeader) contentWidth = Mathf.Max(contentWidth, mHeaderText.preferredWidth);
        }
        // --- End Body Text Handling & Sizing ---


        // --- Calculate Final Tooltip Size ---
        float totalTextHeight = bodyPreferredHeight;
        if (useHeader)
        {
            totalTextHeight += headerPreferredHeight + headerSpacing; // Adiciona altura do header e espaçamento
        }

        // Largura final = largura do conteúdo + padding horizontal * 2
        // Altura final = altura total do texto + padding vertical * 2
        Vector2 newSize = new Vector2(contentWidth + horizontalPadding * 2f, totalTextHeight + verticalPadding * 2f);

        mRectTransform.sizeDelta = newSize;
        mWidth = newSize.x;
        mHeight = newSize.y;
        // --- End Final Size Calculation ---


        // --- Activation & Positioning ---
        if (!mActive || !this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(true);
            mActive = true;
        }
        UpdatePosition(); // Posiciona imediatamente
        // --- End Activation & Positioning ---
    }


    public void HideTooltip()
    {
        if (mActive || (this.gameObject != null && this.gameObject.activeSelf))
        {
            if (mBodyText != null) mBodyText.text = "";
            if (mHeaderText != null && mHeaderText.gameObject.activeSelf) // Desativa o header também
            {
                 mHeaderText.gameObject.SetActive(false);
                 mHeaderText.text = "";
            }
            if (this.gameObject != null) gameObject.SetActive(false);
            mActive = false;
        }
    }

    void Update()
    {
        if (mActive && mGUIMode == RenderMode.ScreenSpaceOverlay)
        {
            UpdatePosition();
        }
    }

    void UpdatePosition()
    {
        if (!mActive || mRectTransform == null || mParentCanvas == null) return;

        float mouseX = Input.mousePosition.x;
        float mouseY = Input.mousePosition.y;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        bool isRightHalf = mouseX > screenWidth / 2f;
        bool isTopHalf = mouseY > screenHeight / 2f;

        float mXShift, mYShift;
        float cornerRotationZ = 0f;
        Vector2 cornerAnchorMin = Vector2.zero;
        Vector2 cornerAnchorMax = Vector2.zero;

        // Calcula o deslocamento baseado no tamanho atual e no gap
        float baseShiftX = mWidth * 0.5f + cursorGap;
        float baseShiftY = mHeight * 0.5f + cursorGap;

        // Determina a direção do deslocamento e a rotação/ancoragem do corner
        if (isRightHalf) { // Mouse na direita -> Tooltip à esquerda
            mXShift = baseShiftX;
            if (isTopHalf) { // Canto Sup Direito -> Tooltip Abaixo Esquerda
                mYShift = baseShiftY;
                cornerAnchorMin = new Vector2(1, 1); cornerAnchorMax = new Vector2(1, 1); cornerRotationZ = 270f;
            } else { // Canto Inf Direito -> Tooltip Acima Esquerda
                mYShift = -baseShiftY;
                cornerAnchorMin = new Vector2(1, 0); cornerAnchorMax = new Vector2(1, 0); cornerRotationZ = 180f;
            }
        } else { // Mouse na esquerda -> Tooltip à direita
            mXShift = -baseShiftX;
            if (isTopHalf) { // Canto Sup Esquerdo -> Tooltip Abaixo Direita
                mYShift = baseShiftY;
                cornerAnchorMin = new Vector2(0, 1); cornerAnchorMax = new Vector2(0, 1); cornerRotationZ = 0f;
            } else { // Canto Inf Esquerdo -> Tooltip Acima Direita
                mYShift = -baseShiftY;
                cornerAnchorMin = new Vector2(0, 0); cornerAnchorMax = new Vector2(0, 0); cornerRotationZ = 90f;
            }
        }

        // Aplica transformação ao Corner Image (se existir)
        if (mCornerImage != null)
        {
            mCornerImage.anchorMin = cornerAnchorMin;
            mCornerImage.anchorMax = cornerAnchorMax;
            mCornerImage.localRotation = Quaternion.Euler(0, 0, cornerRotationZ);
            mCornerImage.anchoredPosition = Vector2.zero;
        }

        // --- Aplica Canvas Scaling Adjustment (Simplificado) ---
        // A precisão depende do modo do CanvasScaler. Esta é uma aproximação.
        if (mScaler != null && mScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
             float ratio = screenWidth / mScaler.referenceResolution.x; // Baseado na largura
             mXShift *= ratio;
             mYShift *= ratio;
        }

        // --- Calcula e Clampa a Posição Final ---
        Vector3 targetPos = new Vector3(mouseX - mXShift, mouseY - mYShift, 0f);

        Vector2 pivot = mRectTransform.pivot;
        float minX = mWidth * pivot.x;
        float maxX = screenWidth - (mWidth * (1f - pivot.x));
        float minY = mHeight * pivot.y;
        float maxY = screenHeight - (mHeight * (1f - pivot.y));

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        // --- Define a Posição ---
        transform.position = targetPos;
    }
}
