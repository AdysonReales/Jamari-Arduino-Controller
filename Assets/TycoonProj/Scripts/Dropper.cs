using UnityEngine;

public class Dropper : MonoBehaviour
{
    //UI PANEL

    public enum BitSpawnMode { Bit0, Bit1, Both }
    public BitSpawnMode spawnMode = BitSpawnMode.Both; // default Both

    

    [Range(0.1f, 5f)]
    public float dropInterval = 1f;

    [Header("Drop Settings")]
    public GameObject bitPrefab0;
    public GameObject bitPrefab1;
    public Transform spawnPoint;

    private float timer = 0f;

    private Vector2[] directions = { Vector2.right, Vector2.down, Vector2.left, Vector2.up };
    private int currentIndex = 1; // default Down
    private Vector2 direction;

    void Awake()
    {
        if (transform.name.Contains("gDropper"))
        {
            enabled = false;
            return;
        }

        if (spawnPoint == null)
        {
            Transform sp = transform.Find("SpawnPoint");
            if (sp != null) spawnPoint = sp;
            else Debug.LogError("[Dropper] No 'SpawnPoint' child found on " + gameObject.name);
        }
    }

    void Start()
    {
        SetDirectionFromRotation(transform.eulerAngles.z);
        UpdateRotation();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= dropInterval)
        {
            timer = 0f;
            TryDropBit();
        }
    }

    private void TryDropBit()
    {
        // Only spawn if a conveyor is in front
        RaycastHit2D hit = Physics2D.Raycast(spawnPoint.position, direction, 1f, LayerMask.GetMask("Conveyor"));
        if (hit.collider != null)
        {
            DropBit();
        }
    }

    public void DropBit()
    {
        if (bitPrefab0 == null || bitPrefab1 == null || spawnPoint == null) return;

        // Only drop if there's a conveyor in front
        RaycastHit2D hit = Physics2D.Raycast(spawnPoint.position, direction, 1f, LayerMask.GetMask("Conveyor"));
        if (hit.collider == null) return;

        GameObject prefabToSpawn;

        switch (spawnMode)
        {
            case BitSpawnMode.Bit0: prefabToSpawn = bitPrefab0; break;
            case BitSpawnMode.Bit1: prefabToSpawn = bitPrefab1; break;
            case BitSpawnMode.Both:
            default:
                prefabToSpawn = Random.value < 0.5f ? bitPrefab0 : bitPrefab1;
                break;
        }

        Vector3 spawnPos = spawnPoint.position + (Vector3)direction * 0.3f;
        spawnPos.z = 0f;

        GameObject newBit = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        Rigidbody2D rb = newBit.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }



    public void CycleDirection()
    {
        currentIndex = (currentIndex + 1) % directions.Length;
        direction = directions[currentIndex];
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        float angle = currentIndex switch
        {
            0 => 0f,   // right
            1 => 270f, // down
            2 => 180f, // left
            3 => 90f,  // up
            _ => 0f
        };
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Call this from BuildManager after placement
    public void SetDirectionFromRotation(float rotZ)
    {
        rotZ = ((rotZ % 360) + 360) % 360;

        int index = 0;
        if (rotZ == 0) index = 0;       // right
        else if (rotZ == 90) index = 3; // up
        else if (rotZ == 180) index = 2; // left
        else if (rotZ == 270) index = 1; // down

        currentIndex = index;
        direction = directions[currentIndex];
        UpdateRotation();
    }

    public void TryDropBitGlobal()
    {
        if (bitPrefab0 == null || bitPrefab1 == null || spawnPoint == null) return;

        // Only drop if there's a conveyor in front
        RaycastHit2D hit = Physics2D.Raycast(spawnPoint.position, direction, 1f, LayerMask.GetMask("Conveyor"));
        if (hit.collider == null) return;

        GameObject prefabToSpawn = Random.value < 0.5f ? bitPrefab0 : bitPrefab1;

        // Slight random offset to prevent overlap visually
        Vector2 offset = Random.insideUnitCircle * 0.1f;

        Vector3 spawnPos = spawnPoint.position + (Vector3)direction * 0.3f + (Vector3)offset;
        spawnPos.z = 0f;

        GameObject newBit = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        Rigidbody2D rb = newBit.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }


}
