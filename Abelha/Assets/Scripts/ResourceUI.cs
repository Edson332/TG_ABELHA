using UnityEngine;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    public TextMeshProUGUI resourceText;

    void Update()
    {
        if (resourceText != null && ResourceManager.Instance != null)
        {
            resourceText.text = $"Néctar: {ResourceManager.Instance.nectar}\n" +
                                $"Mel Carregado: {ResourceManager.Instance.carriedHoney}\n" +
                                $"Mel Armazenado: {ResourceManager.Instance.storedHoney}";
        }
    }
}
