using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Conveyor : MonoBehaviour
{
    public enum ConveyorType { Straight, Corner_LTD, Corner_DTL }
    public ConveyorType type = ConveyorType.Straight;

    [Header("Conveyor Settings")]
    public float speed = 2f;

    // For straight conveyors
    public Vector2 direction = Vector2.right;

    private Bit currentBitOnCorner = null; // only allow 1 bit on corner

    private void Awake()
    {
        direction.Normalize();
        UpdateRotation();
    }

    public void InitConveyor(Vector2 dir, ConveyorType t)
    {
        type = t;
        if (type == ConveyorType.Straight)
            direction = dir.normalized;

        UpdateRotation();
    }

    public void UpdateRotation()
    {
        if (type == ConveyorType.Straight)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else if (type == ConveyorType.Corner_LTD)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Left→Down visual
        }
        else if (type == ConveyorType.Corner_DTL)
        {
            transform.rotation = Quaternion.Euler(0, 0, -90); // Fixed: Down→Left visual
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Bit bit = col.GetComponent<Bit>();
        if (bit == null) return;

        // Handle corners
        if (type == ConveyorType.Corner_LTD || type == ConveyorType.Corner_DTL)
        {
            if (currentBitOnCorner == null)
            {
                currentBitOnCorner = bit;

                // Determine corner type for bit
                Bit.CornerType cornerType = (type == ConveyorType.Corner_LTD)
                                            ? Bit.CornerType.LTD
                                            : Bit.CornerType.DTL;

                bit.EnterCorner(cornerType, transform);
                bit.moveSpeed = speed;
            }
        }
        else
        {
            // Straight conveyor
            bit.SetDirection(direction);
            bit.moveSpeed = speed;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        Bit bit = col.GetComponent<Bit>();
        if (bit == null) return;

        // Clear the corner when the bit exits
        if (bit == currentBitOnCorner)
            currentBitOnCorner = null;
    }
}
