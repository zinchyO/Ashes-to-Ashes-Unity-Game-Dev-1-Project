using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager GM;
    public HudManager hudManager; // Reference to the HudManager script
    public Canvas gameOverCanvas; // Reference to the Game Over canvas
    public TMPro.TextMeshProUGUI winnerText;

    public enum turnState
    {
        level_start,
        p1_ready,
        p1_fired,
        p2_ready,
        p2_fired,
        level_end
    }

    [Header("Inscribed")]
    public GameObject tankPrefab;

    [Header("Dynamic")]
    public turnState state;
    public Tank player1, player2;

    void Awake()
    {
        if (GM != null)
        {
            Debug.Log("ERROR: GM has already been set.");
            return;
        }
        GM = this;

        hudManager = FindObjectOfType<HudManager>(); // Ensure that HudManager is assigned
        if (hudManager == null)
        {
            Debug.LogError("HudManager not found in the scene. Source GameManager.cs");
            // Handle this situation appropriately (e.g., disable the tank or show an error message)
        }
        
        // Assign WinnerText in the Unity Editor
        winnerText = gameOverCanvas.transform.Find("WinnerText").GetComponent<TMPro.TextMeshProUGUI>();
        if (winnerText == null)
        {
            Debug.LogError("WinnerText not found in the scene. Source GameManager.cs");
            // Handle this situation appropriately (e.g., disable the tank or show an error message)
        }
        
        // Stop either player from performing an action
        state = turnState.level_start;

        // Set up player 1's tank
        player1 = GameObject.Instantiate<GameObject>(tankPrefab).GetComponent<Tank>();
        player1.transform.position = new(-9.0f, 0);
        player1.Barrel.rotation = Quaternion.Euler(0f, 0f, -90f);
        player1.name = "Player 1";
        player1.tankColor = new Color(0.4f, 0.0f, 0.0f);
        player1.gameObject.layer = LayerMask.NameToLayer("Player1");
        Tank.P1 = player1;

        //Set up player 2's tank
        player2 = GameObject.Instantiate<GameObject>(tankPrefab).GetComponent<Tank>();
        player2.transform.position = new(9.0f, 0);
        player2.Barrel.rotation = Quaternion.Euler(0f, 0f, 90f);
        player2.name = "Player 2";
        player2.tankColor = new Color(0.0f, 0.0f, 0.4f);
        player2.gameObject.layer = LayerMask.NameToLayer("Player2");
        Tank.P2 = player2;

        UpdateState();
        
    }

    public static void UPDATE_STATE()
    {
        GM.UpdateState();
    }

    public void UpdateState()
    {
        switch (state)
        {
            case turnState.level_start:
                state = turnState.p1_ready;
                hudManager.UpdateTurn("Player 1"); // Update the turn UI
                player1.ResetFuel(); // Reset Fuel for player 1
                break;

            case turnState.p1_ready:
                hudManager.UpdateTurn("Player 1"); // Update the turn UI
                state = turnState.p1_fired;
                break;
            case turnState.p1_fired:
                state = turnState.p2_ready;
                hudManager.UpdateTurn("Player 2"); // Update the turn UI
                player2.ResetFuel(); // Reset fuel for player 2
                break;

            case turnState.p2_ready:
                hudManager.UpdateTurn("Player 2"); // Update the turn UI
                state = turnState.p2_fired;
                break;
            case turnState.p2_fired:
                state = turnState.p1_ready;
                hudManager.UpdateTurn("Player 1"); // Update the turn UI
                player1.ResetFuel(); // Reset fuel for player 1
                break;

            case turnState.level_end:
                break;
        }
    }

    public static turnState STATE()
    {
        return GM.state;
    }
    
    public void TankDestroyed(Tank destroyedTank)
    {
        // Handle tank destruction logic...
        state = turnState.level_end;

        // Check if the other tank is still alive
        if (destroyedTank == player1)
        {
            ShowGameOverScreen("!!!GAME OVER!!!\nPlayer 2");
        }
        else if (destroyedTank == player2)
        {
            ShowGameOverScreen("!!!GAME OVER!!!\nPlayer 1");
        }

        // Wait before returning to menu
        IEnumerator coroutine = WaitThenReturnToMenu(3.0f);
        StartCoroutine(coroutine);
    }

    // Used to add a timer before returning to main menu
    private IEnumerator WaitThenReturnToMenu(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            SceneManager.LoadScene(0);
        }
    }

    public void ShowGameOverScreen(string winner)
    {
        // Activate the Game Over Canvas
        gameOverCanvas.gameObject.SetActive(true);

        // Display the winning player
        winnerText.text = winner + " Wins";
    }
}
