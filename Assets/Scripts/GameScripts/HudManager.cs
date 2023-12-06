using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// import text mesh pro
using TMPro;


public class HudManager : MonoBehaviour
{
    public TextMeshProUGUI fuelText;
    public Slider ShotPowerSlider;
    public TextMeshProUGUI turnText;

    // Add methods to update UI elements based on game events
    public void UpdateFuel(float fuelLevel)
    {
        int roundedFuelLevel = Mathf.RoundToInt(fuelLevel);
        fuelText.text = $"Fuel: {roundedFuelLevel}%";
    }

    public void UpdateShotPower(float power)
    {
        ShotPowerSlider.value = power;
    }

    public void UpdateTurn(string player)
    {
        turnText.text = $"Turn: {player}";
    }
}
