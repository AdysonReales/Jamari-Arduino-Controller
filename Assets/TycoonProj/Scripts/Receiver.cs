using UnityEngine;

public class Receiver : MonoBehaviour
{
    public int moneyPerBit = 10; // how much each Bit gives
    private MoneyManager moneyManager;

    void Start()
    {
        // Find the MoneyManager in the scene
        moneyManager = FindObjectOfType<MoneyManager>();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Bit bit = col.GetComponent<Bit>();
        if (bit != null)
        {
            // Add money when Bit reaches Receiver
            if (moneyManager != null)
            {
                moneyManager.AddMoney(moneyPerBit);
            }

            Destroy(bit.gameObject);
        }
    }
}
