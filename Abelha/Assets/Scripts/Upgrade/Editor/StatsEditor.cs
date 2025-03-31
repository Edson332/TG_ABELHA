using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Stats))]
public class StatsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Desenha o Inspector padrão
        DrawDefaultInspector();

        // Obtém a referência ao script alvo
        Stats statsScript = (Stats)target;

        // Adiciona um botão ao Inspector
        if (GUILayout.Button("Resetar Upgrades Aplicados"))
        {
            statsScript.ResetAppliedUpgrades();
            Debug.Log("Upgrades aplicados foram resetados.");
        }
    }
}
