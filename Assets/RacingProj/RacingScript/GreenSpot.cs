using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GreenSpot : MonoBehaviour
{
    [HideInInspector] public ParkingLotManager manager;
    [HideInInspector] public int spotIndex;

    private void Update()
    {
        if (manager == null || manager.currentCar == null) return;

        Collider2D carCol = manager.currentCar.GetComponent<Collider2D>();
        Bounds carBounds = carCol.bounds;
        Bounds spotBounds = GetComponent<Collider2D>().bounds;

        // Only advance level when the player's full collider is inside the green spot
        if (spotBounds.Contains(carBounds.min) && spotBounds.Contains(carBounds.max))
        {
            manager.PlayerParked(spotIndex);
        }
    }
}
