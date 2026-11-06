using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BuildModeManager : MonoBehaviour {
    [Header("UI References")]
    public GameObject buildPanel;
    public Button closeButton;
    public List<Button> buildButtons; // assign Conveyor, Dropper, etc. in Inspector
    public RectTransform triangle;    // 🔺 assign triangle UI element
    public Vector2 triangleOffset = new Vector2(-50f, 0f);

    [Header("References")]
    public BuildManager buildManager;

    private int selectedIndex = 0;
    private bool inBuildMenu = false;
    private bool inBuildMode = false;

    public static BuildModeManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start() {
        buildPanel.SetActive(false);

        if (closeButton != null) {
            closeButton.onClick.AddListener(ExitBuildMenu);
        }

        if (triangle != null)
            triangle.gameObject.SetActive(false);
    }

    void Update()
    {
        // 🔑 Press Enter to toggle Build Menu
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ToggleBuildMenu();
        }

        if (inBuildMenu)
        {
            HandleNavigation();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ExitToMenu();
        }

    }

    public void ExitToMenu() {
        Time.timeScale = 1f; // make sure game isn’t paused
        SceneManager.LoadScene(1); // go back to your game picker
    }

    public bool IsInBuildMenu()
    {
        return inBuildMenu;
    }

    void ToggleBuildMenu() {
        if (inBuildMenu) ExitBuildMenu();
        else EnterBuildMenu();
    }

    void EnterBuildMenu() {
        inBuildMenu = true;
        inBuildMode = false;
        buildPanel.SetActive(true);
        selectedIndex = 0;
        HighlightCurrent();

        if (triangle != null)
            triangle.gameObject.SetActive(true);
    }

    void ExitBuildMenu() {
        inBuildMenu = false;
        inBuildMode = true;
        buildPanel.SetActive(false);

        if (triangle != null)
            triangle.gameObject.SetActive(false);
    }

    void HandleNavigation() {
        if (Input.GetKeyDown(KeyCode.W)) {
            selectedIndex = (selectedIndex - 1 + buildButtons.Count) % buildButtons.Count;
            HighlightCurrent();
        }
        if (Input.GetKeyDown(KeyCode.S)) {
            selectedIndex = (selectedIndex + 1) % buildButtons.Count;
            HighlightCurrent();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (buildButtons.Count > 0 && buildButtons[selectedIndex] != null && buildManager != null) {
                string type = buildButtons[selectedIndex].gameObject.name.Replace("Button", "");
                buildManager.SelectToBuild(type);
                ExitBuildMenu();
            } else {
                Debug.LogWarning("BuildManager or BuildButtons not set up correctly!");
            }
        }


        if (Input.GetKeyDown(KeyCode.Escape)) {
            ExitBuildMenu();
        }
    }

    void HighlightCurrent() {
        for (int i = 0; i < buildButtons.Count; i++) {
            var colors = buildButtons[i].colors;
            colors.normalColor = (i == selectedIndex) ? Color.yellow : Color.white;
            buildButtons[i].colors = colors;
        }

        if (triangle != null && buildButtons.Count > 0) {
            RectTransform btnRect = buildButtons[selectedIndex].GetComponent<RectTransform>();
            RectTransform triRect = triangle.GetComponent<RectTransform>();

            // Parent the highlight to the same parent as buttons (keeps panel layout)
            triRect.SetParent(btnRect.parent, false);

            // Match position & size
            triRect.anchoredPosition = btnRect.anchoredPosition;
            triRect.sizeDelta = btnRect.sizeDelta;

            // Reset scale so it’s not stretched
            triRect.localScale = Vector3.one;
        }

        if (EventSystem.current != null && buildButtons.Count > 0) {
            EventSystem.current.SetSelectedGameObject(buildButtons[selectedIndex].gameObject);
        }
    }
}
