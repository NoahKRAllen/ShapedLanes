using UnityEngine;
using UnityEngine.SceneManagement;
public class GameOver : MonoBehaviour
{
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject gameWonUI;
    [SerializeField] private GameObject[] endGameUI;
    [SerializeField] private GameObject[] resourceUI;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private Transform enemyContainer;
    public bool gameOver = false;

    public void GameLost()
    {
        gameOverUI.SetActive(true);
        EndGame();
    }

    public void GameWon()
    {
        gameWonUI.SetActive(true);
        EndGame();
    }

    private void EndGame()
    {
        gameOver = true;
        foreach (GameObject g in resourceUI)
        {
            g.SetActive(false);
        }
        foreach (GameObject g in endGameUI)
        {
            g.SetActive(true);
        }
        
        foreach (Transform child in playerContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemyContainer)
        {
            Destroy(child.gameObject);
        }

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
