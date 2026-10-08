using UnityEngine;

public class LevelSelManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void gotolevel(int levelIndex)
    {
        // Load the specified level scene based on the provided level index
        string levelName = "level" + levelIndex; // Construct the level name (e.g., "level1", "level2", etc.)
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName); // Load the corresponding level scene
    }
    public void BackToMenu()
    {
        // Load the main menu scene when the "Back" button is clicked
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}
