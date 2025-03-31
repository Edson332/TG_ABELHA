using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Upgrade")]
public abstract class Upgrade : ScriptableObject
{
    public Sprite icon { get; private set; }
    public string upgradeName;
    public string description { get; private set; }
    public ResourceAmount cost = new ResourceAmount(ResourceType.Research, 500);

    public abstract void DoUpgrade();
}

// Cria um botão manualmente no Inspector
#if UNITY_EDITOR
[CustomEditor(typeof(Upgrade), true)]
public class UpgradeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Upgrade upgrade = (Upgrade)target;
        if (GUILayout.Button("Executar Upgrade"))
        {
            upgrade.DoUpgrade();
        }
    }
}
#endif

