// Scripts/GameSystems/GameBootstrapper.cs
using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Prefabs dos Gerenciadores Essenciais")]
    [Tooltip("Arraste aqui o prefab do seu AudioManager.")]
    public GameObject audioManagerPrefab;
    
    [Tooltip("Arraste aqui o prefab do seu SaveLoadManager.")]
    public GameObject saveLoadManagerPrefab;
    
    // Adicione aqui outros prefabs de gerenciadores se necessário
    // public GameObject passiveIncomeManagerPrefab;

    void Awake()
    {
        // Este método garante que os sistemas essenciais existam.
        // Ele verifica se a 'Instancia' de cada gerenciador já foi definida.
        // Se não foi, significa que o gerenciador não existe, então nós o criamos.

        if (AudioManager.Instancia == null)
        {
            Debug.Log("AudioManager não encontrado. Criando a partir do prefab...");
            Instantiate(audioManagerPrefab);
        }

        if (SaveLoadManager.Instancia == null)
        {
            Debug.Log("SaveLoadManager não encontrado. Criando a partir do prefab...");
            Instantiate(saveLoadManagerPrefab);
        }
        
        // Exemplo para outros gerenciadores:
        // if (PassiveIncomeManager.Instancia == null)
        // {
        //     Instantiate(passiveIncomeManagerPrefab);
        // }

        // O Bootstrapper já fez seu trabalho, então pode se destruir para não poluir a cena.
        Destroy(gameObject);
    }
}