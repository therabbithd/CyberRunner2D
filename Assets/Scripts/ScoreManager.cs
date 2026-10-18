using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText; // Reference to the TextMeshProUGUI component for displaying the score
    int currentScore; // Puntuacion acumulada (volteretas + monedas)

    public int CurrentScore => currentScore;

    /// <summary>
    /// Suma puntos al marcador: 100 por voltereta y 50 por moneda (ver Coin.cs).
    /// </summary>
    public void AddScore(int points)
    {
        currentScore += points;
        if (scoreText != null)
            scoreText.text = currentScore.ToString("0000");
    }
}
