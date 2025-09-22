// Scripts/Effects/AutoDestroyVFX.cs
using UnityEngine;
public class AutoDestroyVFX : MonoBehaviour
{
    [Tooltip("Tempo em segundos para destruir este GameObject.")]
    public float destroyDelay = 1.5f; 
    void Start() { Destroy(gameObject, destroyDelay); }
}
