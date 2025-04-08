using UnityEngine;

namespace SpiritsOfTheLostWorld
{
    public class GlobalGameState : MonoBehaviour
    {
        public static GlobalGameState Instance;

        // Aqui vai sua variável persistente
        public int runesCollected = 0;
        public int runesOpen = 0;
        public bool hasSpecialKey = false;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Persiste entre cenas
            }
            else
            {
                Destroy(gameObject); // Garante que só haja uma instância
            }
        }
    }
}
