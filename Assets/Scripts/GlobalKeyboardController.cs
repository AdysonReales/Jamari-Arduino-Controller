using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GlobalKeyboardController : MonoBehaviour
{
    public static GlobalKeyboardController Instance;

    [Header("UI Buttons in Current Scene")]
    public Button[] buttons;    // Assign dynamically or in inspector
    private int currentIndex = 0;

    [Header("Selection Triangle Sprite")]
    public RectTransform triangle;  // Assign your triangle sprite here
    public float pulseAmount = 1.2f; // How big the triangle grows
    public float pulseSpeed = 3f;    // Pulsing speed

    [Header("Triangle Offset (adjust in Inspector or runtime)")]
    public Vector3 triangleOffset = new Vector3(-50f, 0f, 0f); // default offset

    private Vector3 triangleOriginalScale;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Controller persists
            DontDestroyOnLoad(triangle.gameObject); // Triangle persists too
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        if (triangle != null)
            triangleOriginalScale = triangle.localScale;

        HighlightButton(currentIndex);
    }

    private void Update()
    {
        if (buttons == null || buttons.Length == 0) return;

        // Make sure triangle exists before using it
        if (triangle != null)
        {
            // Pulsate triangle
            float scale = Mathf.Sin(Time.time * pulseSpeed) * (pulseAmount - 1f) + 1f;
            triangle.localScale = triangleOriginalScale * scale;
        }

        // Navigate buttons using WASD
        if (Input.GetKeyDown(KeyCode.W))
        {
            currentIndex = (currentIndex - 1 + buttons.Length) % buttons.Length;
            HighlightButton(currentIndex);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            currentIndex = (currentIndex + 1) % buttons.Length;
            HighlightButton(currentIndex);
        }

        // Press button
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            buttons[currentIndex].onClick.Invoke();
        }
    }


    private void HighlightButton(int index)
    {
        if (triangle == null || buttons.Length == 0) return;

        // Move in local space (relative to parent UI)
        triangle.SetParent(buttons[index].transform.parent, false);
        triangle.localPosition = buttons[index].transform.localPosition + triangleOffset;
    }

    // Call this each time you load a new scene to update buttons
    public void SetButtons(Button[] newButtons)
    {
        buttons = newButtons;
        currentIndex = 0;
        if (buttons.Length > 0)
            HighlightButton(currentIndex);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int buildIndex = scene.buildIndex;

        // ✅ If this is a GAME scene, destroy the menu controller
        if (buildIndex >= 2) // (2 = snake, 3 = racing, 4 = tycoon)
        {
            Debug.Log($"[GlobalKeyboardController] Destroying self for scene: {scene.name}");
            if (triangle != null)
                Destroy(triangle.gameObject); // also remove triangle
            Destroy(gameObject); // destroy menu controller
            return;
        }

        // ✅ Otherwise, we're in a menu (0 = main menu, 1 = game picker)
        buttons = FindObjectsOfType<Button>();
        currentIndex = 0;

        // Find triangle
        if (triangle == null)
        {
            GameObject triObj = GameObject.FindGameObjectWithTag("SelectorTriangle");
            if (triObj != null)
            {
                triangle = triObj.GetComponent<RectTransform>();
                triangleOriginalScale = triangle.localScale;
            }
        }

        // Offset depending on scene
        switch (buildIndex)
        {
            case 0:
                triangleOffset = new Vector2(320f, 0f);
                break;
            case 1:
                triangleOffset = new Vector2(220f, 0f);
                break;
            default:
                triangleOffset = new Vector2(-50f, 0f);
                break;
        }

        if (buttons.Length > 0 && triangle != null)
            HighlightButton(currentIndex);
    }
}
