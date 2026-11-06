using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class ParkedCar : MonoBehaviour
{
    [HideInInspector] public ParkingLotManager manager;

    void Awake()
    {
        // Ensure the Rigidbody2D is kinematic
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (manager == null) return;

        if (collision.collider.CompareTag("Player"))
        {
            // Call the new method
            manager.OnPlayerHitParkedCar();
        }
    }

}
