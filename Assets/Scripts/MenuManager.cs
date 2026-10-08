using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuManager : MonoBehaviour
{
    public void PlayGame()
    {
        // Load the main game scene when the "Play" button is clicked
       SceneManager.LoadScene("LevelSel");
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
    public void QuitGame()
    {
        // Quit the application when the "Quit" button is clicked
        Application.Quit();
    }
    public void gotoselChar()
    {
        SceneManager.LoadScene("CharacterMenu");
    }
}
