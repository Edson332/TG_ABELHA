using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpiritsOfTheLostWorld
{
    public class RuneCheck : MonoBehaviour
    {
        
        [Header("Estado do botão")]
        public bool isLocked = true;

        [Header("Sprites")]
        public Sprite lockedSprite;
        public Sprite unlockedSprite;

        [Header("Referências")]
        public Image buttonImage;
        public TextMeshProUGUI targetText; // Troque GameObject por TextMeshProUGUI

        [Header("Textos que serão exibidos")]
        public string lockedMessage = "Locked";
        public string unlockedMessage = "Unlocked";

        void Start()
        {
            UpdateButtonVisual();

            if (targetText != null)
                targetText.text = ""; // Começa vazio
        }

        void Update()
        {
            UpdateButtonVisual();
        }

        public void TryActivate()
        {
            if (!isLocked && targetText != null)
            {
                // Alterna entre o texto desbloqueado e vazio
                if (targetText.text == unlockedMessage)
                {
                    targetText.text = "";
                }
                else
                {
                    targetText.text = unlockedMessage;
                }
            }
            else if (isLocked && targetText != null)
            {
                targetText.text = lockedMessage;
            }
        }

        public void SetLocked(bool locked)
        {
            isLocked = locked;
            UpdateButtonVisual();
        }

        void UpdateButtonVisual()
        {
            if (buttonImage != null)
            {
                buttonImage.sprite = isLocked ? lockedSprite : unlockedSprite;
            }
        }
    }
}
