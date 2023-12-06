using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankShooting : MonoBehaviour
{
    public HudManager hudManager; // Reference to the HudManager script
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed;
    public float maxBulletSpeed = 20;  //  variable for maximum bullet speed
    public float minBulletSpeed = 10;  //  variable for maximum bullet speed
    public float shotPower = 1f;  //  variable for shot power
    public float powerChangeRate = 0.1f;  // Rate at which shot power changes
    public GameObject projLinePrefab; // Reference to the ProjectileLine prefab
    public bool drawProjectileLines = true; // Whether or not to draw projectile lines

    // Variables for trajectory prediction
    public int resolution = 10; // Number of points on the trajectory line
    public float simulationTime = 1f; // Time duration for the trajectory simulation


 
    // Reference to the Tank script
    private Tank tankScript;

    public Transform barrelTrans;  // Reference to the Barrel object

    void Start()
    {
        hudManager = FindObjectOfType<HudManager>();
        if (hudManager == null)
        {
            Debug.LogError("HudManager not found in the scene. Source TankShooting.cs");
            // Handle this situation appropriately (e.g., disable the tank or show an error message)
        }

        // Assuming the Tank script is on the same GameObject
        tankScript = GetComponent<Tank>();

        // Debug statement to check if Barrel is assigned
    if (barrelTrans == null)
    {
        Debug.LogError("Barrel is not assigned in TankShooting script.");
    }
    }
    void Update()
    {
        // Increase shot power if E key is pressed
        if (Input.GetKey(KeyCode.E))
        {
            shotPower += powerChangeRate * Time.deltaTime;
        }

        // Decrease shot power if Q key is pressed
        if (Input.GetKey(KeyCode.Q))
        {
            shotPower -= powerChangeRate * Time.deltaTime;
        }

        // Clamp shot power between 0 and 1
        shotPower = Mathf.Clamp(shotPower, 0f, 1f);
        
        // Update the shot power UI
        hudManager.UpdateShotPower(shotPower);

        // Adjust bullet speed based on shot power
        bulletSpeed = minBulletSpeed + shotPower * (maxBulletSpeed - minBulletSpeed);

        //Skip input check for firing if the state isn't correct
        if (GameManager.STATE() == (tankScript.Equals(Tank.P1) ? GameManager.turnState.p1_ready : GameManager.turnState.p2_ready) && Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.UPDATE_STATE();
            var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            
            // Set the bullet's layer to correspond to the tank that shot it
            bullet.layer = tankScript == Tank.P1 ? LayerMask.NameToLayer("ProjectileP1") : LayerMask.NameToLayer("ProjectileP2");
            
            // Multiply velocity by shot power
            bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * bulletSpeed;
            
            // Reset the explosion state for the next shot
            bullet.GetComponent<Bullet>().ResetExplosionState();
            
            // Only draw the projectile line if drawProjectileLines is true
            if (drawProjectileLines)
            {
                UpdateTrajectory(bulletSpawnPoint.position, bulletSpawnPoint.up * bulletSpeed, Physics2D.gravity);
        
                // Instantiate the ProjectileLine prefab and set it as a child of the bullet
                var projLine = Instantiate(projLinePrefab, bullet.transform.position, bullet.transform.rotation);
                projLine.transform.SetParent(bullet.transform);
            }


        }

        // Adjust bullet spawn point with Barrel tilt
        AdjustBulletSpawnPoint();
    }

    void AdjustBulletSpawnPoint()
    {
        // Get the current rotation of the Barrel
        float BarrelRotation = barrelTrans.eulerAngles.z;

        // Reset the bullet spawn point's rotation
        bulletSpawnPoint.rotation = Quaternion.identity;

        // Offset the bullet spawn point based on Barrel rotation
        bulletSpawnPoint.localPosition = new Vector3(0f, 1.5f, 0f); // Adjust this value based on your desired offset
        bulletSpawnPoint.Rotate(0f, 0f, BarrelRotation);
    }
    // function to update the trajectory line
    void UpdateTrajectory(Vector3 initialPosition, Vector3 initialVelocity, Vector3 gravity)
    {
        LineRenderer lineRenderer = projLinePrefab.GetComponent<LineRenderer>();
        lineRenderer.positionCount = resolution;

        float timestep = simulationTime / resolution;
        Vector3 position = initialPosition;
        Vector3 velocity = initialVelocity;

        for (int i = 0; i < resolution; i++)
        {
            // Calculate position and velocity at the next timestep
            position += velocity * timestep;
            velocity += gravity * timestep;

            // Update the position of the trajectory line point
            lineRenderer.SetPosition(i, position);
        }
    }


}
