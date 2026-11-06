using UnityEngine;

public class ANDGate : MonoBehaviour
{
    // Example logic: double the bit's value
    public void ApplyGate(Bit bit)
    {
        if (bit != null)
        {
            bit.SetValue(bit.value * 2);
        }
    }

    // Trigger when a Bit enters the gate
    private void OnTriggerEnter2D(Collider2D other)
    {
        Bit bit = other.GetComponent<Bit>();
        if (bit != null)
        {
            ApplyGate(bit);
        }
    }
}
