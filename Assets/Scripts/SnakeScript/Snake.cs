using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Snake : MonoBehaviour
{
    public Transform segmentPrefab;
    public Vector2Int direction = Vector2Int.right;
    public float speed = 20f;
    public float speedMultiplier = 1f;
    public int initialSize = 4;
    public bool moveThroughWalls = false;

    private readonly List<Transform> segments = new List<Transform>();
    private Vector2Int input;
    private float nextUpdate;
    private float eatCooldown = 0f;

    private void Start()
    {
        ResetState();
    }

    private void Update()
    {
        // Only allow turning along perpendicular axis
        if (direction.x != 0)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                input = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                input = Vector2Int.down;
        }
        else if (direction.y != 0)
        {
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                input = Vector2Int.right;
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                input = Vector2Int.left;
        }
    }

    private void FixedUpdate()
    {
        if (Time.time < nextUpdate) return;

        if (input != Vector2Int.zero) direction = input;

        for (int i = segments.Count - 1; i > 0; i--)
        {
            segments[i].position = segments[i - 1].position;
        }

        transform.position = new Vector2(
            Mathf.RoundToInt(transform.position.x) + direction.x,
            Mathf.RoundToInt(transform.position.y) + direction.y
        );

        nextUpdate = Time.time + (1f / (speed * speedMultiplier));

        if (eatCooldown > 0) eatCooldown -= Time.fixedDeltaTime;
    }

    public void Grow()
    {
        Transform segment = Instantiate(segmentPrefab);
        segment.position = segments[segments.Count - 1].position;
        segment.tag = "SnakeBody";
        segments.Add(segment);
    }

    public void ResetState()
    {
        direction = Vector2Int.right;
        transform.position = Vector3.zero;

        FindObjectOfType<GameManager>().ResetScore();

        for (int i = 1; i < segments.Count; i++)
            Destroy(segments[i].gameObject);

        segments.Clear();
        segments.Add(transform);

        for (int i = 0; i < initialSize - 1; i++)
            Grow();

        // Enable movement again
        enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (segments[0] != transform) return;

        if (other.CompareTag("Food"))
        {
            Grow();
            FindObjectOfType<GameManager>().AddScore(10);
            eatCooldown = 0.1f;
        }
        else if (other.CompareTag("Obstacle") ||
                 (other.CompareTag("Wall") && !moveThroughWalls) ||
                 (other.CompareTag("SnakeBody") && eatCooldown <= 0f && other.transform != segments[0]))
        {
            GameOverSnake();
        }
        else if (other.CompareTag("Wall") && moveThroughWalls)
        {
            Traverse(other.transform);
        }
    }

    private void Traverse(Transform wall)
    {
        Vector3 position = transform.position;

        if (direction.x != 0) position.x = Mathf.RoundToInt(-wall.position.x + direction.x);
        else if (direction.y != 0) position.y = Mathf.RoundToInt(-wall.position.y + direction.y);

        transform.position = position;
    }

    private void GameOverSnake()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.GameOver();

        // Stop snake movement
        enabled = false;
    }

    public bool Occupies(int x, int y)
{
    foreach (Transform segment in segments)
    {
        if (Mathf.RoundToInt(segment.position.x) == x &&
            Mathf.RoundToInt(segment.position.y) == y)
        {
            return true;
        }
    }

    return false;
}

}
