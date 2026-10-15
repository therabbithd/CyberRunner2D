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
        if (characterIndex < 1)
        {
            Debug.LogWarning($"CharacterMenuManager: invalid character index {characterIndex}.");
            return;
        }
        // Store the selected character index in PlayerPrefs (1-based: 1=Dino, 2=Frog)
        PlayerPrefs.SetInt("SelectedCharacter", characterIndex);
        //save player preferences
        PlayerPrefs.Save();
        Debug.Log($"CharacterMenuManager: character {characterIndex} selected and saved.");
    }
}
