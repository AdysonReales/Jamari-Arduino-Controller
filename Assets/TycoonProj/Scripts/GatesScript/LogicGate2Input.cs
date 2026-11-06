using UnityEngine;

public enum GateType { NOT, BUFFER, OR, AND, NAND, XOR, XNOR, NOR }

[RequireComponent(typeof(Collider2D))]
public class LogicGate2Input : MonoBehaviour
{
    [Header("Gate Settings")]
    public GateType gateType;
    public Color gateColor;

    [Header("Output Settings")]
    public Transform outputPoint;         // Where output bit spawns
    public GameObject outputBit0Prefab;   // Prefab for result 0
    public GameObject outputBit1Prefab;   // Prefab for result 1

    private Bit input1Bit;
    private Bit input2Bit;

    // Returns the direction bits should come from (input side)
    public Vector2 GetInputDirection()
    {
        // Assume default input is LEFT, gate default facing right
        float rotZ = (transform.eulerAngles.z + 360) % 360;
        return rotZ switch
        {
            0f => Vector2.left,    // default, facing right, input on left
            90f => Vector2.down,
            180f => Vector2.right,
            270f => Vector2.up,
            _ => Vector2.left
        };
    }

    // Returns the output direction (for reference)
    public Vector2 GetOutputDirection()
    {
        float rotZ = (transform.eulerAngles.z + 360) % 360;
        return rotZ switch
        {
            0f => Vector2.right,
            90f => Vector2.up,
            180f => Vector2.left,
            270f => Vector2.down,
            _ => Vector2.right
        };
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        Bit bit = col.GetComponent<Bit>();
        if (bit == null) return;

        // Assign input bits
        if (input1Bit == null)
        {
            input1Bit = bit;
        }
        else if (input2Bit == null)
        {
            input2Bit = bit;
        }
        else
        {
            // Both inputs already occupied for gates that need 2
            return;
        }

        // Determine if we have enough input to evaluate
        bool readyToEvaluate = false;

        if (gateType == GateType.NOT || gateType == GateType.BUFFER)
        {
            // Only input1 is needed
            readyToEvaluate = (input1Bit != null);
        }
        else
        {
            // Require two inputs
            readyToEvaluate = (input1Bit != null && input2Bit != null);
        }

        if (!readyToEvaluate) return;

        // Hide input bits
        input1Bit.gameObject.SetActive(false);
        if (input2Bit != null)
            input2Bit.gameObject.SetActive(false);

        // Evaluate
        int a = input1Bit != null ? input1Bit.value : 0;
        int b = input2Bit != null ? input2Bit.value : 0;
        int result = EvaluateGate(a, b);

        // Spawn output bit
        GameObject prefabToSpawn = (result == 0) ? outputBit0Prefab : outputBit1Prefab;
        if (prefabToSpawn != null && outputPoint != null)
        {
            Vector3 spawnPos = outputPoint.position;
            spawnPos.z = 0f;
            GameObject newBit = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            Bit newBitComp = newBit.GetComponent<Bit>();
            if (newBitComp != null)
                newBitComp.SetValue(result);

            SpriteRenderer sr = newBit.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                sr.color = new Color(1f, 1f, 1f, 1f);
            }
        }

        // Destroy input bits
        Destroy(input1Bit.gameObject);
        if (input2Bit != null)
            Destroy(input2Bit.gameObject);

        // Reset
        input1Bit = null;
        input2Bit = null;
    }


    private int EvaluateGate(int a, int b)
    {
        switch (gateType)
        {
            case GateType.NOT: return a == 0 ? 1 : 0;
            case GateType.BUFFER: return a;
            case GateType.OR: return a | b;
            case GateType.AND: return a & b;
            case GateType.NAND: return (a & b) == 0 ? 1 : 0;
            case GateType.XOR: return a ^ b;
            case GateType.XNOR: return (a ^ b) == 0 ? 1 : 0;
            case GateType.NOR: return (a | b) == 0 ? 1 : 0;
            default: return 0;
        }
    }
}
