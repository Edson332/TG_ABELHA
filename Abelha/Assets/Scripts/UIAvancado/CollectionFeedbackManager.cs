// Scripts/Managers/CollectionFeedbackManager.cs
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CollectionFeedbackManager : MonoBehaviour
{
    [Header("Configuração")]
    [Tooltip("O prefab do texto flutuante para recursos (com o script FloatingResourceText).")]
    public GameObject floatingResourceTextPrefab; // Nome atualizado para clareza
    
    [Tooltip("O Canvas em Screen Space - Overlay onde os textos serão criados.")]
    public RectTransform screenSpaceCanvas;

    // The pool should be of the correct type
    private List<FloatingResourceText> _objectPool = new List<FloatingResourceText>(); // MODIFICADO: Tipo da lista
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        GameEvents.OnResourceCollectedByBee += ShowCollectionText;
    }

    private void OnDisable()
    {
        GameEvents.OnResourceCollectedByBee -= ShowCollectionText;
    }

    private void ShowCollectionText(Transform location, float amount, TipoRecurso type)
    {

        if (CombatManager.Instancia != null && CombatManager.Instancia.isCombatActive)
        {
            return;
        }
        if (floatingResourceTextPrefab == null || screenSpaceCanvas == null || _mainCamera == null) return;
        
        Vector2 screenPosition = _mainCamera.WorldToScreenPoint(location.position);
        
        FloatingResourceText textInstance = GetPooledText(); // MODIFICADO: O retorno agora é do tipo correto
        
        textInstance.transform.SetParent(screenSpaceCanvas, false);
        (textInstance.transform as RectTransform).position = screenPosition;
        
        textInstance.SetText($"+{amount:F1}");

        TextMeshProUGUI textMesh = textInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh != null)
        {
            switch (type)
            {
                case TipoRecurso.Nectar:
                    textMesh.color = Color.blue;
                    break;
                case TipoRecurso.Mel:
                    textMesh.color = new Color(1.0f, 0.75f, 0.0f); // Ouro
                    break;
            }
        }
        
        textInstance.Init();
    }

    // Lógica do pool de objetos para reutilizar textos
    private FloatingResourceText GetPooledText() // MODIFICADO: Tipo de retorno da função
    {
        foreach (var text in _objectPool)
        {
            if (!text.gameObject.activeInHierarchy)
            {
                return text;
            }
        }
        
        GameObject newTextGO = Instantiate(floatingResourceTextPrefab);
        FloatingResourceText newFloatingText = newTextGO.GetComponent<FloatingResourceText>(); // MODIFICADO: Pega o componente correto
        
        if (newFloatingText == null)
        {
            Debug.LogError("O prefab 'floatingResourceTextPrefab' não contém o script 'FloatingResourceText'!", newTextGO);
            Destroy(newTextGO);
            return null;
        }

        _objectPool.Add(newFloatingText);
        newTextGO.SetActive(false); 
        return newFloatingText;
    }
}