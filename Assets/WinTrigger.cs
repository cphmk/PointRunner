using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Import TextMeshPro for UI

public class WinTrigger : MonoBehaviour
{
    public GameObject winMessage; // ✅ This should now allow you to drag the WinMessage UI

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Check if the player reaches the goal
        {
            Debug.Log("🎉 Player Reached the End of the Maze! 🎉");

            // Check if ScoreManager exists and player has 100+ points
            if (ScoreManager.Instance != null && ScoreManager.Instance.GetScore() >= 100)
            {
                Debug.Log("✅ Player has 100 points! WINNING!");

                // ✅ Show the win message
                if (winMessage != null)
                {
                    winMessage.SetActive(true); // ✅ Now it will be enabled when the player wins
                }

                // Freeze the game for 3 seconds before loading the next scene
                Invoke("LoadNextScene", 3f);
            }
            else
            {
                Debug.Log("❌ Player needs 100 points to win!");
            }
        }
    }

    private void LoadNextScene()
    {
        Debug.Log("➡️ Loading next scene...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); // Load next scene
    }
}
