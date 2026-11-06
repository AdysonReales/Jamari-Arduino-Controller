using UnityEngine;

public class LevelObstacle : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Make sure your car prefab is tagged "Player"
        {
            FindObjectOfType<ParkingLotManager>().StartLevel();
        }
    }
}
