using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText; // Reference to the TextMeshProUGUI component for displaying the score
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void AddScore(int score)
    {
        scoreText.text = score.ToString("0000"); // Update the score text with the new score
    }
}
