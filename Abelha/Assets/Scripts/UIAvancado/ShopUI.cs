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

    void Start() // Se você não tiver um Start, crie um.
    {
        if (showAllBeesToggle != null && BeeVisualsManager.Instancia != null)
        {
            // Define o estado inicial do toggle e adiciona o listener
            showAllBeesToggle.isOn = true; // Começa mostrando todas
            showAllBeesToggle.onValueChanged.AddListener(OnBeeVisualsToggleChanged);
        }
    }

    public void OnBeeVisualsToggleChanged(bool showAll)
    {
        if (BeeVisualsManager.Instancia != null)
        {
            BeeVisualsManager.Instancia.SetVisualsMode(showAll);
        }
    }
    void OnEnable()
    {
        // OnEnable é chamado toda vez que o objeto da loja se torna ativo.
        // É um bom lugar para atualizar a visibilidade dos painéis.
        UpdatePanelsVisibility();
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