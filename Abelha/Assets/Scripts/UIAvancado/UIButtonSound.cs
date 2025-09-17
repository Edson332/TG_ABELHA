// Scripts/UI/UIButtonSound.cs
using UnityEngine;
using UnityEngine.EventSystems; // Para detectar o clique

public class UIButtonSound : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("O nome do efeito sonoro a ser tocado, como definido no AudioManager.")]
    public string clickSoundName = "ButtonClick"; // Nome padrão

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.Instancia != null)
        {
            AudioManager.Instancia.PlaySFX(clickSoundName);
        }
    }
}