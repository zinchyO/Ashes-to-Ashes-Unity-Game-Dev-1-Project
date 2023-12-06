using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public float speed = 1.0f; // Speed of tank. You can adjust this value as needed

    private float tankWidth; // Radius of tank base collider in world coordinates
    private float tankHeight; // height of tank base

    public HudManager hudManager; // Reference to the HudManager script
   


    [Header("Dynamic")]
    [Tooltip("Public representations of Player 1 and Player 2")]
    public static Tank P1, P2;
    [Tooltip("Color of each tank")]
    public Color tankColor = Color.black;

    void Start()
    {
        hudManager = FindObjectOfType<HudManager>();
        if (hudManager == null)
        {
            Debug.LogError("HudManager not found in the scene. Source Tank.cs");
            // Handle this situation appropriately (e.g., disable the tank or show an error message)
        }
        // Get the radius of the tank base collider
        tankWidth = transform.localScale.x;
        tankHeight = transform.localScale.y/2;
        GetComponentInChildren<MeshRenderer>().material.color = tankColor;
        transform.position = new(transform.position.x, 
            Mathf.Max(  TerrainManager.HEIGHT_AT(transform.position.x), 
                        TerrainManager.HEIGHT_AT(transform.position.x+tankWidth),
                        TerrainManager.HEIGHT_AT(transform.position.x-tankWidth)
            ));
        // initialize fuel to maxfuel
        fuel = maxfuel;
        
    }

    [Header("Barrel")]
    public Transform Barrel;  // Reference to the Barrel object
    public float BarrelTiltSpeed = 30f;  // Tilt speed of the Barrel
    

    [Header("Fuel")]
    public float maxfuel = 100f; // Maximum fuel level
    public float fuel; // current fuel level
    public float fuelConsumptionRate = 1f; // Rate at which fuel is consumed
    
    void Update()
    {
        // skip update if state isn't correct
        if (!(GameManager.STATE() == (Equals(P1) ? GameManager.turnState.p1_ready : GameManager.turnState.p2_ready))) return;

            float x = Input.GetAxis("Horizontal");
        
        // Only allow movement if there is fuel left
        if (fuel > 0)
        {
            Vector3 newPosition = transform.position;
            newPosition.x += x * speed * Time.deltaTime;
            float nextToTank = TerrainManager.HEIGHT_AT(newPosition.x + tankWidth * 1.1f * x);

            if (nextToTank - newPosition.y <= tankHeight)
                newPosition.y = Mathf.Max(nextToTank, newPosition.y);

            transform.position = newPosition;

            // Decrease fuel based on movement
            fuel -= Mathf.Abs(x) * fuelConsumptionRate * Time.deltaTime;
            
            // Update the fuel UI
            hudManager.UpdateFuel(fuel);
        }

        // Barrel tilt code should be outside the fuel check
        float tiltInput = Input.GetAxis("Vertical");
        TiltBarrel(tiltInput);
    }

    void TiltBarrel(float tiltInput)
    {
        // Calculate the new rotation angle based on input
        float tiltAngle = Barrel.eulerAngles.z;
        tiltAngle = (tiltAngle > 180) ? tiltAngle - 360 : tiltAngle; // Convert to -180 to 180 scale
        float newTiltAngle = Mathf.Clamp(tiltAngle + tiltInput * BarrelTiltSpeed * Time.deltaTime, -120f, 120f);

        // Apply the new rotation to the Barrel
        Barrel.rotation = Quaternion.Euler(0f, 0f, newTiltAngle);
    }

    // Method to reset fuel
    public void ResetFuel()
    {
        fuel = maxfuel;
    }
}