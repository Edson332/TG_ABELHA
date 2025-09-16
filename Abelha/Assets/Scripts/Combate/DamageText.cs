// Scripts/CombatSystem/DamageText.cs
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float fadeTime = 1f;
    private TextMeshProUGUI _textMesh;

    void Awake()
    {
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
        Destroy(gameObject, fadeTime); // Garante que o objeto seja destruído
    }

    public void SetText(string text)
    {
        if(_textMesh != null) _textMesh.text = text;
    }

    void Update()
    {
        // Move o texto para cima
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
    }
}