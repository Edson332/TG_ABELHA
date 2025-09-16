// Scripts/CombatSystem/CombatCameraController.cs
using UnityEngine;
using System.Collections;

public class CombatCameraController : MonoBehaviour
{
    public static CombatCameraController Instancia { get; private set; }

    private Vector3 _originalPosition;
    private Coroutine _shakeCoroutine;

    void Awake() 
    { 
        if (Instancia != null && Instancia != this) { Destroy(this); return; }
        Instancia = this;
    }

    void Start()
    {
        _originalPosition = transform.localPosition; // Usamos localPosition para tremer em relação à câmera pai
    }

    /// <summary>
    /// Inicia o efeito de tremor na câmera.
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        // Impede que múltiplos shakes aconteçam ao mesmo tempo, resetando o anterior
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
            transform.localPosition = _originalPosition; // Garante que a posição seja resetada
        }
        _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while(elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            transform.localPosition = new Vector3(_originalPosition.x + x, _originalPosition.y + y, _originalPosition.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = _originalPosition; // Retorna à posição original
        _shakeCoroutine = null;
    }
}