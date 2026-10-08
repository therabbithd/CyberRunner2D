using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterMenuManager : MonoBehaviour
{
    public void BackToMenu()
    {
        // Load the main menu scene when the "Back" button is clicked
        SceneManager.LoadScene("Menu");
    }
    public void selectCharacter(int characterIndex)
    {
        // Store the selected character index in PlayerPrefs
        PlayerPrefs.SetInt("SelectedCharacter", characterIndex);
        //save player preferences
        PlayerPrefs.Save();
    }
}
