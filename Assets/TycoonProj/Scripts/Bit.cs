using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bit : MonoBehaviour
{
    [Header("Bit Settings")]
    public int value = 0;
    public float lifeTime = 30f;
    public float moveSpeed = 2f;

    [Header("Clogging Detection")]
    public GameObject currentBitAhead;

    //MAKES FROZEN
    [HideInInspector] public bool frozen = false;


    public enum CornerType { None, LTD, DTL }
    [HideInInspector] public CornerType cornerType = CornerType.None;
    private bool onCorner = false;
    private Transform cornerTransform;

    [HideInInspector] public Vector2 direction = Vector2.zero;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        // Lifetime countdown
        lifeTime -= Time.fixedDeltaTime;
        if (lifeTime <= 0f) { Destroy(gameObject); return; }

        // Corner handling
        if (onCorner && cornerTransform != null)
        {
            rb.position = cornerTransform.position;

            if (cornerType == CornerType.LTD)
            {
                float zRot = cornerTransform.eulerAngles.z;
                int rot90 = Mathf.RoundToInt(zRot / 90f) % 4;

                switch (rot90)
                {
                    case 0: direction = Vector2.down; break;  // 0°: left → down
                    case 1: direction = Vector2.right; break; // 90°: down → right
                    case 2: direction = Vector2.up; break;    // 180°: right → up
                    case 3: direction = Vector2.left; break;  // 270°: up → left
                }
            }
            else if (cornerType == CornerType.DTL)
            {
                float zRot = cornerTransform.eulerAngles.z;
                int rot90 = Mathf.RoundToInt(zRot / 90f) % 4;

                switch (rot90)
                {
                    case 0: direction = Vector2.left; break;  // 0°
                    case 1: direction = Vector2.down; break;  // 90°
                    case 2: direction = Vector2.right; break; // 180°
                    case 3: direction = Vector2.up; break;    // 270°
                }
            }

            onCorner = false;
        }

        // Stop if another bit is in front
        if (currentBitAhead != null)
        {
            float distance = Vector2.Distance(rb.position, currentBitAhead.transform.position);
            if (distance <= moveSpeed * Time.fixedDeltaTime + 0.05f)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        // Move along current direction
        rb.linearVelocity = direction.normalized * moveSpeed;
        /*
        if (frozen)
        {
            rb.linearVelocity = Vector2.zero;
            return; // Stop all movement
        }
        */

    }

    public void EnterCorner(CornerType type, Transform cornerTrans)
    {
        cornerType = type;
        cornerTransform = cornerTrans;
        onCorner = true;
        rb.linearVelocity = Vector2.zero;
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    public void SetValue(int v)
    {
        value = v;
    }


}
