using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    // Assuming GameManager maintains references to the player tanks
    private Tank player1;
    private Tank player2;
    public GameObject indicatorPrefab;

    private GameObject currentIndicator;
    private Tank currentTarget;
    private bool player1Destroyed = false;
    private bool player2Destroyed = false;

    void Start()
    {
        // You can find the tanks using their names or tags
        player1 = GameObject.Find("Player 1").GetComponent<Tank>();
        player2 = GameObject.Find("Player 2").GetComponent<Tank>();

        if (player1 == null || player2 == null)
        {
            Debug.LogError("Player tanks not found!");
        }
        else currentIndicator = Instantiate(indicatorPrefab, transform);
    }

    void Update()
    {
        // Check whose turn it is and set the target accordingly
        if (GameManager.STATE() == GameManager.turnState.p1_ready)
        {
            if (player1 == null)
            {
                if (!player1Destroyed)
                {
                    Debug.LogWarning("Player 1 tank has been destroyed!");
                    player1Destroyed = true;
                }
                return;
            }
            currentTarget = player1;
        }
        else if (GameManager.STATE() == GameManager.turnState.p2_ready)
        {
            if (player2 == null)
            {
                if (!player2Destroyed)
                {
                    Debug.LogWarning("Player 2 tank has been destroyed!");
                    player2Destroyed = true;
                }
                return;
            }
            currentTarget = player2;
        }
        else if (currentIndicator != null && GameManager.STATE() == GameManager.turnState.level_end)
        {
            // No player's turn, hide the indicator
            currentIndicator.SetActive(false);
            return;
        }

        // Update the position of the indicator to follow the target
        currentIndicator.transform.position = currentTarget.transform.position + Vector3.up;
    }
}