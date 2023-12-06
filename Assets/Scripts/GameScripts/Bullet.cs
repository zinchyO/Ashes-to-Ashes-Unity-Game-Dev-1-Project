using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class Bullet : MonoBehaviour
{
    public float life = 10;             // Time in seconds before the bullet is destroyed on its own
    public GameObject explosionPrefab;  // Reference to the explosion prefab
    public float explosionRadius = 1f;  // Radius of the explosion

    private bool hasExploded = false;   // Flag to prevent multiple explosions
    public bool awake { get; set; } = true; // Flag to prevent explosion on spawn
    private BoundsCheck bChk;

    void Awake() 
    {
        bChk = GetComponent<BoundsCheck>();
        awake = true; 
        Destroy(gameObject, life);
    }

    // Call this method to reset the explosion state for the next shot
    public void ResetExplosionState()
    {
        hasExploded = false;
    }

    // Destroy the projectile if it is off screen to the left, right, or bottom
    private void Update()
    {
        if (!(bChk.isOnScreen || bChk.LocIs(BoundsCheck.eScreenLocations.offUp))) Destroy(gameObject);
    }

    // Method to handle collision with other objects
    void OnCollisionEnter2D(Collision2D collision)
    {
        awake = false;

        if (!hasExploded)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            hasExploded = true;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            foreach (Collider2D hitCollider in colliders)
            {
                Rigidbody2D rb = hitCollider.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direction = rb.transform.position - transform.position;
                    rb.AddForce(direction.normalized * 10f, ForceMode2D.Impulse);
                }

                if (hitCollider.CompareTag("Tanks"))
                {
                    // Handle damage to the tank or destroy it
                    Tank hitTank = hitCollider.GetComponent<Tank>();
                    if (hitTank != null)
                    {
                        GameManager.GM.TankDestroyed(hitTank);
                        Destroy(hitTank.gameObject);
                    }
                }
            }

            TerrainManager.REMOVE_FROM_TERRAIN((int)explosionRadius, transform.position);
        }

        Destroy(gameObject);
        // Notify GameManager about tank destruction GameManager.TankDestroyed(this);
    }

    private void OnDestroy()
    {
        GameManager.UPDATE_STATE();
    }
}
