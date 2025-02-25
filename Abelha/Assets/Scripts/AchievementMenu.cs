using UnityEngine;

public class AchievementMenu : MonoBehaviour
{
    public GameObject achievementPanel;

    public void ToggleAchievementPanel()
    {
        achievementPanel.SetActive(!achievementPanel.activeSelf);
    }
}
