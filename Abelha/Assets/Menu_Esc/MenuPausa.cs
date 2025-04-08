using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SpiritsOfTheLostWorld
{
    public class MenuPausa : MonoBehaviour
    {
        public GameObject pauseScreen;
        public GameObject options;
        public GameObject runes;
        
        void Start()
        {
            
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (pauseScreen.activeSelf)
                    Continue();
                else
                    Pause();
            }
        }

        public void Pause()
        {
            options.SetActive(false);
            runes.SetActive(false);
            pauseScreen.SetActive(true);
            Time.timeScale = 0;
        }

        public void Continue()
        {
            options.SetActive(false);
            runes.SetActive(false);
            pauseScreen.SetActive(false);
            Time.timeScale = 1;
        }

           public void button_exit()
        {
        
            Application.Quit();

        }


        public void Runes()
        {
            options.SetActive(false);
            runes.SetActive(true);

        }

        public void Options()
        {
            runes.SetActive(false);
            //controladordeEfeitosonoro.PlayOneShot(somEfeitoSonoro);
            options.SetActive(true);
        }

        public void Back()
        {
            runes.SetActive(false);
            options.SetActive(false);
        }
    }
}
