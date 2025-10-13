// Scripts/UI/FloatingResourceText.cs
using UnityEngine;
using TMPro;

public class FloatingResourceText : MonoBehaviour
{
    public float moveSpeed = 70f; // Velocidade em pixels por segundo
    public float fadeOutTime = 1f;
    
    private TextMeshProUGUI _textMesh;
    private RectTransform _rectTransform;
    private Color _startColor;
    private float _timeToFade;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Init()
    {
        if (_textMesh != null) _startColor = _textMesh.color;
        _timeToFade = Time.time + fadeOutTime;
        gameObject.SetActive(true); 
    }

    public void SetText(string text)
    {
        if(_textMesh != null) _textMesh.text = text;
    }

    void Update()
    {
        if (Time.time >= _timeToFade)
        {
            gameObject.SetActive(false); // Desativa para o pool
            return;
        }
        
        _rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;

        float alpha = Mathf.Clamp01((_timeToFade - Time.time) / fadeOutTime);
        if (_textMesh != null)
        {
            _textMesh.color = new Color(_startColor.r, _startColor.g, _startColor.b, alpha);
        }
    }
}