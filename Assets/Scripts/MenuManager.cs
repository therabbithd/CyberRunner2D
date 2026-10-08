using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        // Load the main game scene when the "Play" button is clicked
       SceneManager.LoadScene("level1");
    }
    public void Credits()
    {
        // Load the credits scene when the "Credits" button is clicked
        SceneManager.LoadScene("Credits");
    }
    public void BackToMenu()
    {
        // Load the main menu scene when the "Back" button is clicked
        SceneManager.LoadScene("Menu");
    }
}
