using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour
{
    [SerializeField] private string socialMediaURL; // URL for the social media link
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void BackToMenu()
    {
        // Load the main menu scene when the "Back" button is clicked
        SceneManager.LoadScene("Menu");
    }
    public void SocialMediaLink()
    {
        // Open the specified social media link in the default web browser
        Application.OpenURL(socialMediaURL);
    }
}
