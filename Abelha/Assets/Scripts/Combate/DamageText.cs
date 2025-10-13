// Scripts/CombatSystem/DamageText.cs
using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float fadeTime = 1f;
    
    private TextMeshProUGUI _textMesh;
    private Camera _combatCamera; // Referência para a câmera de combate

    void Awake()
    {
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        Destroy(gameObject, fadeTime);
    }

    void Start()
    {
        // Pega a referência da câmera de combate ativa no momento da sua criação
        if (CombatManager.Instancia != null)
        {
            _combatCamera = CombatManager.Instancia.combatCamera;
        }
    }
    
    // Usamos LateUpdate para garantir que a orientação aconteça após a câmera se mover
    void LateUpdate()
    {
        // Garante que o texto sempre encare a câmera de combate
        if (_combatCamera != null)
        {
            transform.LookAt(transform.position + _combatCamera.transform.rotation * Vector3.forward,
                             _combatCamera.transform.rotation * Vector3.up);
        }
    }

    public void SetText(string text)
    {
        if(_textMesh != null) _textMesh.text = text;
    }

    void Update()
    {
        // Move o texto para cima no seu próprio eixo local
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
    }
}