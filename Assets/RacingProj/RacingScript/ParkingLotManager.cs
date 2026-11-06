using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ParkingLotManager : MonoBehaviour
{
    [Header("Triangle Navigation")]
    public RectTransform triangle;
    public Vector2 triangleOffset = new Vector2(-50f, 0f);

    private Button[] menuButtons;
    private int currentButtonIndex = 0;

    [Header("Gameplay")]
    private float levelStartTime;
    private int highestLevel = 0;

    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI bestTimeText;
    [SerializeField] private TextMeshProUGUI highestLevelText;

    [Header("Setup")]
    public Collider2D[] parkingSpots;
    public Transform[] spawnPoints;
    public GameObject playerCarPrefab;
    public CameraFollow2D mainCamera;

    [Header("Parked Car Prefabs")]
    public GameObject[] parkedCarPrefabs;
    public GameObject greenSpotPrefab;

    [Header("UI")]
    public GameObject hitPanel;
    public Button restartButton;
    public Button exitButton;

    [Header("Gameplay")]
    public int level = 0;

    private int targetIndex;
    public GameObject currentCar;
    private GameObject[] spawnedCars;

    void Start()
    {
        spawnedCars = new GameObject[parkingSpots.Length];

        // Load highest level from previous sessions
        highestLevel = PlayerPrefs.GetInt("HighestLevel", 0);

        // Hook buttons
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
        if (exitButton != null) exitButton.onClick.AddListener(ExitGame);

        StartLevel();
        UpdateUI();
    }

    void Update()
    {
        // 🚗 Gameplay logic: parking detection
        if (currentCar != null)
        {
            Collider2D carCol = currentCar.GetComponent<Collider2D>();
            if (carCol != null)
            {
                Bounds carBounds = carCol.bounds;
                Bounds targetBounds = parkingSpots[targetIndex].bounds;

                // ✅ Car fully inside target spot
                if (targetBounds.Contains(carBounds.min) && targetBounds.Contains(carBounds.max))
                {
                    PlayerParked(targetIndex);
                }
            }

            // ⏱ Update elapsed time
            if (timeText != null)
            {
                float elapsedTime = Time.time - levelStartTime;
                timeText.text = $"Time: {elapsedTime:F2}s";
            }
        }

        // 🎮 Menu navigation when hitPanel is active
        if (hitPanel != null && hitPanel.activeSelf && menuButtons != null && menuButtons.Length > 0)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentButtonIndex = (currentButtonIndex - 1 + menuButtons.Length) % menuButtons.Length;
                HighlightCurrentButton();
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentButtonIndex = (currentButtonIndex + 1) % menuButtons.Length;
                HighlightCurrentButton();
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                menuButtons[currentButtonIndex].onClick.Invoke();
            }
        }
    }
    #region Collision / Parking

    public void OnPlayerHitParkedCar()
    {
        menuButtons = new Button[] { restartButton, exitButton };
        currentButtonIndex = 0;
        HighlightCurrentButton();

        if (hitPanel != null)
            hitPanel.SetActive(true);

        if (currentCar != null)
        {
            Rigidbody2D rb = currentCar.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        // Pause the game
        Time.timeScale = 0f;
    }

    public void PlayerParked(int index)
    {
        if (index != targetIndex) return;

        float elapsedTime = Time.time - levelStartTime;

        // Save best time for this level
        float previousBest = PlayerPrefs.GetFloat("BestTime_Level_" + level, Mathf.Infinity);
        if (elapsedTime < previousBest)
        {
            PlayerPrefs.SetFloat("BestTime_Level_" + level, elapsedTime);
            PlayerPrefs.Save();
        }

        level++;

        // Save highest level globally
        if (level > highestLevel)
        {
            highestLevel = level;
            PlayerPrefs.SetInt("HighestLevel", highestLevel);
            PlayerPrefs.Save();
        }

        UpdateUI();
        StartLevel();
    }

    #endregion

    #region Level Setup

    public void StartLevel()
    {
        levelStartTime = Time.time;

        // Destroy old player
        if (currentCar != null) Destroy(currentCar);

        // Destroy old parked cars
        if (spawnedCars != null)
        {
            foreach (var c in spawnedCars)
                if (c != null) Destroy(c);
        }

        // Spawn player
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        currentCar = Instantiate(playerCarPrefab, spawn.position, spawn.rotation);
        currentCar.transform.Rotate(0f, 0f, 90f);

        if (mainCamera != null) mainCamera.SetTarget(currentCar.transform);

        // Pick target spot
        targetIndex = Random.Range(0, parkingSpots.Length);

        // Spawn cars
        spawnedCars = new GameObject[parkingSpots.Length];
        for (int i = 0; i < parkingSpots.Length; i++)
        {
            Vector3 pos = parkingSpots[i].bounds.center;
            Quaternion rot = parkingSpots[i].transform.rotation;
            GameObject obj;

            if (i == targetIndex)
            {
                obj = Instantiate(greenSpotPrefab, pos, rot, transform);
                obj.tag = "GreenTarget";

                GreenSpot gs = obj.GetComponent<GreenSpot>();
                if (gs != null)
                {
                    gs.manager = this;
                    gs.spotIndex = targetIndex;
                }
            }
            else
            {
                if (parkedCarPrefabs.Length == 0) continue;
                GameObject prefab = parkedCarPrefabs[Random.Range(0, parkedCarPrefabs.Length)];
                obj = Instantiate(prefab, pos, rot, transform);
                obj.tag = "ParkedCar";

                ParkedCarTrigger trigger = obj.GetComponent<ParkedCarTrigger>();
                if (trigger == null) trigger = obj.AddComponent<ParkedCarTrigger>();
                trigger.manager = this;
            }

            FitToSpot(obj, parkingSpots[i], i);
            spawnedCars[i] = obj;
        }

        if (hitPanel != null)
            hitPanel.SetActive(false);

        Debug.Log("🎯 Level " + level + " | Target spot: " + targetIndex);
    }

    #endregion

    #region Helper

    private void UpdateUI()
    {
        if (levelText != null) levelText.text = "Level: " + level;

        float bestTimeForLevel = PlayerPrefs.GetFloat("BestTime_Level_" + level, Mathf.Infinity);
        if (bestTimeText != null)
            bestTimeText.text = "Best Time: " + (bestTimeForLevel == Mathf.Infinity ? "--" : bestTimeForLevel.ToString("F2") + "s");

        if (highestLevelText != null)
            highestLevelText.text = "Highest Lvl: " + highestLevel;
    }

    private void FitToSpot(GameObject obj, Collider2D spot, int spotIndex)
    {
        Vector2 defaultSize = new Vector2(2.2f, 4.2f);

        int[] sideSpots = { 34, 35, 36, 39, 40, 41 };
        bool isSideParking = System.Array.Exists(sideSpots, index => index == spotIndex);

        if (isSideParking)
        {
            obj.transform.localScale = new Vector3(0.66f, 1.95f, obj.transform.localScale.z);//0.66,1.95
            obj.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }
        else
        {
            Vector2 spotSize = spot.bounds.size;
            obj.transform.localScale = new Vector3(
                spotSize.x / defaultSize.x,
                spotSize.y / defaultSize.y,
                obj.transform.localScale.z
            );
            obj.transform.rotation = spot.transform.rotation;
        }

        obj.transform.position = spot.bounds.center;
    }

    private void HighlightCurrentButton()
    {
        if (triangle != null && menuButtons != null && currentButtonIndex >= 0)
        {
            triangle.position = menuButtons[currentButtonIndex].transform.position + new Vector3(-50f, 0, 0);
        }
    }




    public void RestartLevel()
    {
        if (hitPanel != null)
            hitPanel.SetActive(false);

        menuButtons = null;
        Time.timeScale = 1f;
        StartLevel();
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    #endregion
}
