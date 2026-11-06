using UnityEngine;

public class NOTGate : MonoBehaviour
{
    // This method flips the Bit's value (0 -> 1, 1 -> 0)
    public void ApplyGate(Bit bit)
    {
        if (bit != null)
        {
            int newValue = (bit.value == 0) ? 1 : 0;
            bit.SetValue(newValue);
        }
    }

    // Example: trigger when a Bit enters the gate
    private void OnTriggerEnter2D(Collider2D other)
    {
        Bit bit = other.GetComponent<Bit>();
        if (bit != null)
        {
            ApplyGate(bit);
        }
    }
}
