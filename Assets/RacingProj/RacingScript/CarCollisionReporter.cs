using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class CarCollisionReporter : MonoBehaviour
{
    private ParkingLotManager manager;

    // Called by manager when instantiating player
    public void Init(ParkingLotManager mgr)
    {
        manager = mgr;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            manager.OnPlayerHitParkedCar();
        }
    }

}
