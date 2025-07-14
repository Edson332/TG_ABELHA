
using UnityEngine;
using System.IO; // Para lidar com arquivos

public static class SaveSystem
{
    private static readonly string SAVE_FILENAME = "beegame_save.json";

    // Retorna o caminho completo para o arquivo de save
    private static string GetSavePath()
    {
        // Application.persistentDataPath é uma pasta segura em qualquer plataforma
        // (Windows, Mac, Android, iOS) para salvar dados de jogo.
        return Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
    }


    /// <summary>
    /// Salva o objeto GameData em um arquivo JSON.
    /// </summary>
    public static void SaveGame(GameData data)
    {
        string savePath = GetSavePath();
        // Converte o objeto GameData para uma string JSON formatada (prettyPrint = true)
        string json = JsonUtility.ToJson(data, true);

        try
        {
            // Escreve a string JSON no arquivo, sobrescrevendo se já existir
            File.WriteAllText(savePath, json);
            Debug.Log($"Jogo salvo com sucesso em: {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Falha ao salvar o jogo: {e.Message}");
        }
    }

    
    /// <summary>
    /// Carrega o objeto GameData a partir de um arquivo JSON.
    /// </summary>
    public static GameData LoadGame()
    {
        string savePath = GetSavePath();

        if (File.Exists(savePath))
        {
            try
            {
                // Lê todo o texto do arquivo
                string json = File.ReadAllText(savePath);
                // Converte a string JSON de volta para um objeto GameData
                GameData loadedData = JsonUtility.FromJson<GameData>(json);
                Debug.Log($"Jogo carregado com sucesso de: {savePath}");
                return loadedData;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Falha ao carregar o jogo: {e.Message}");
                return null; // Retorna nulo se houver um erro na leitura/conversão
            }
        }
        else
        {
            Debug.Log("Nenhum arquivo de save encontrado. Iniciando um novo jogo.");
            return null; // Retorna nulo se o arquivo não existe
        }
    }

    /// <summary>
    /// Deleta o arquivo de save existente. Útil para um botão de "Novo Jogo".
    /// </summary>
    public static void DeleteSaveFile()
    {
        string savePath = GetSavePath();
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Arquivo de save deletado.");
        }
    }
}