using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ParkedCarTrigger : MonoBehaviour
{
    [HideInInspector] public ParkingLotManager manager;

    private void Awake()
    {
        // Ensure collider is a trigger
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (manager == null) return;

        // Only care if it's the player car
        if (other.CompareTag("Player"))
        {
            manager.OnPlayerHitParkedCar();
        }
    }
}
