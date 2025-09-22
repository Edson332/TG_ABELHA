// Scripts/UI/ShopUI.cs
using UnityEngine;
using UnityEngine.UI;
public class ShopUI : MonoBehaviour
{
    [Header("Painéis da Loja")]
    [Tooltip("O GameObject que contém os botões de compra das abelhas primárias.")]
    public GameObject primaryBeesPanel; // Opcional, sempre visível

    [Tooltip("O GameObject que contém os botões de compra das abelhas passivas. Este será ativado/desativado.")]
    public GameObject passiveBeesPanel;

    [Tooltip("Um objeto visual (ex: um painel com um cadeado e texto) para mostrar quando as abelhas passivas estão bloqueadas.")]
    public GameObject passiveBeesLockedOverlay; // Opcional
    [Header("Opções Visuais")]
    public Toggle showAllBeesToggle;
    [Header("Opções Visuais")]
    [Tooltip("Arraste aqui o Toggle para ativar/desativar a aura visual da Rainha.")]
    public Toggle queenAuraToggle;

    private const string AURA_VFX_KEY = "AuraVFXEnabled";

    void Start() // Se você não tiver um Start, crie um.
    {
        if (showAllBeesToggle != null && BeeVisualsManager.Instancia != null)
        {
            // Define o estado inicial do toggle e adiciona o listener
            showAllBeesToggle.isOn = true; // Começa mostrando todas
            showAllBeesToggle.onValueChanged.AddListener(OnBeeVisualsToggleChanged);
        }
        if (queenAuraToggle != null)
        {
            // 1. Define o estado inicial do toggle com base no valor salvo
            // PlayerPrefs.GetInt(chave, padrão): Pega o valor salvo. Se não existir, usa o padrão (1 = true).
            queenAuraToggle.isOn = PlayerPrefs.GetInt(AURA_VFX_KEY, 1) == 1;
            
            // 2. Adiciona um "listener" para que o método seja chamado quando o toggle mudar
            queenAuraToggle.onValueChanged.AddListener(OnAuraVfxToggleChanged);
        }
    }

    void OnEnable()
    {
        // OnEnable é chamado toda vez que o objeto da loja se torna ativo.
        // É um bom lugar para atualizar a visibilidade dos painéis.
        UpdatePanelsVisibility();
    }
    public void OnBeeVisualsToggleChanged(bool showAll)
    {
        if (BeeVisualsManager.Instancia != null)
        {
            BeeVisualsManager.Instancia.SetVisualsMode(showAll);
        }
    }


    public void OnAuraVfxToggleChanged(bool isEnabled)
    {
        // Salva a preferência do jogador (1 para true, 0 para false)
        PlayerPrefs.SetInt(AURA_VFX_KEY, isEnabled ? 1 : 0);
        PlayerPrefs.Save(); // Força o salvamento
        Debug.Log($"Visual da aura da Rainha definido para: {isEnabled}");

        // Se a Rainha já existir na cena, notifica-a para atualizar seu visual imediatamente.
        if (QueenBeeController.Instancia != null)
        {
            QueenBeeController.Instancia.UpdateVisualsBasedOnSetting();
        }
    }

    void Update()
    {
        // O Update garante que a loja se atualize assim que a condição for atendida,
        // mesmo que o jogador já esteja com a loja aberta.
        // Pode ser otimizado com eventos no futuro, se necessário.
        UpdatePanelsVisibility();
    }

    private void UpdatePanelsVisibility()
    {
        if (BeeManager.Instancia == null) return;

        bool arePassivesUnlocked = BeeManager.Instancia.AreAllPrimaryBeesUnlocked();

        if (passiveBeesPanel != null)
        {
            passiveBeesPanel.SetActive(arePassivesUnlocked);
        }

        if (passiveBeesLockedOverlay != null)
        {
            passiveBeesLockedOverlay.SetActive(!arePassivesUnlocked);
        }
    }
}