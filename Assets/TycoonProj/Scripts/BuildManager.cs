using UnityEngine;
using System.Collections.Generic;

public class BuildManager : MonoBehaviour
{
    [Header("Grid & Prefabs")]
    [SerializeField] private Grid buildGrid;
    //Double tap space
    private float lastSpaceTime = 0f;
    private float doublePressThreshold = 0.3f; // 0.3s for double-tap   

    [Header("Machines")]
    public GameObject conveyorPrefab;
    public GameObject cornerConveyorPrefab;   // LTD
    public GameObject corner2ConveyorPrefab;  // DTL
    public GameObject dropperPrefab;
    public GameObject receiverPrefab;
    public GameObject splitterPrefab;

    [Header("Logic Gates")]
    public GameObject bufferGatePrefab;
    public GameObject andGatePrefab;
    public GameObject nandGatePrefab;
    public GameObject norGatePrefab;
    public GameObject notGatePrefab;
    public GameObject orGatePrefab;
    public GameObject xnorGatePrefab;
    public GameObject xorGatePrefab;
    public GameObject twowayPrefab;
    public GameObject intersectionPrefab;

    [Header("Ghosts")]
    public GameObject smallGhostPrefab;
    public GameObject largeGhostPrefab;
    public GameObject cornerGhostPrefab;
    public GameObject corner2GhostPrefab;
    public GameObject dropperGhostPrefab;

    private GameObject selectedPrefab;
    private GameObject ghostInstance;
    private Vector3Int gridPos;
    private int rotation;

    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

    // ---------------- Selection ----------------
    public void SelectToBuild(string type)
    {
        CancelPlacement();

        switch (type)
        {
            case "Conveyor":
                selectedPrefab = conveyorPrefab;
                ghostInstance = Instantiate(smallGhostPrefab);
                break;

            case "CornerConveyor":
                selectedPrefab = cornerConveyorPrefab;
                ghostInstance = Instantiate(cornerGhostPrefab);
                break;

            case "Corner2Conveyor":
                selectedPrefab = corner2ConveyorPrefab;
                ghostInstance = Instantiate(corner2GhostPrefab);
                break;

            case "Dropper":
                selectedPrefab = dropperPrefab;
                ghostInstance = Instantiate(dropperGhostPrefab);
                break;

            case "Receiver":
                selectedPrefab = receiverPrefab;
                ghostInstance = Instantiate(smallGhostPrefab);
                break;

            case "Splitter":
                selectedPrefab = splitterPrefab;
                ghostInstance = Instantiate(smallGhostPrefab);
                break;

            case "Twoway":
                selectedPrefab = twowayPrefab;
                ghostInstance = Instantiate(twowayPrefab);
                break;

            case "Intersection":
                selectedPrefab = intersectionPrefab;
                ghostInstance = Instantiate(intersectionPrefab);
                break;

            // Logic Gates (2x2)
            case "BUFFERGate": selectedPrefab = bufferGatePrefab; ghostInstance = Instantiate(largeGhostPrefab); break;
            case "ANDGate": selectedPrefab = andGatePrefab; ghostInstance = Instantiate(largeGhostPrefab); break;
            case "NANDGate": selectedPrefab = nandGatePrefab; ghostInstance = Instantiate(largeGhostPrefab); break;
            case "NORGate": selectedPrefab = norGatePrefab; ghostInstance = Instantiate(largeGhostPrefab); break;
            case "NOTGate": selectedPrefab = notGatePrefab; ghostInstance = Instantiate(largeGhostPrefab); break;
            case "ORGate": selectedPrefab = orGatePrefab; ghostInstance = Instantiate(largeGhostPrefab); break;
            case "XNORGate": selectedPrefab = xnorGatePrefab; ghostInstance = Instantiate(largeGhostPrefab); break;
            case "XORGate": selectedPrefab = xorGatePrefab; ghostInstance = Instantiate(largeGhostPrefab); break;

            default:
                Debug.LogWarning("Unknown build type: " + type);
                return;
        }

        gridPos = Vector3Int.zero;
        rotation = 0;

        DisableGhostCollider();
        UpdateGhost();
    }

    private void DisableGhostCollider()
    {
        if (ghostInstance != null && ghostInstance.TryGetComponent<Collider2D>(out Collider2D col))
            col.enabled = false;
    }

    private void CancelPlacement()
    {
        selectedPrefab = null;
        if (ghostInstance != null) Destroy(ghostInstance);
        gridPos = Vector3Int.zero;
        rotation = 0;
    }

    // ---------------- Movement & Rotation ----------------
    private void HandleMovement()
    {
        if (Input.GetKeyDown(KeyCode.W)) gridPos += Vector3Int.up;
        if (Input.GetKeyDown(KeyCode.S)) gridPos += Vector3Int.down;
        if (Input.GetKeyDown(KeyCode.A)) gridPos += Vector3Int.left;
        if (Input.GetKeyDown(KeyCode.D)) gridPos += Vector3Int.right;
    }

    private void HandleRotation()
    {
        if (Input.GetKeyDown(KeyCode.E)) rotation = (rotation + 90) % 360;
    }

    // ---------------- Placement ----------------
    private void HandlePlacement()
    {
        if (selectedPrefab == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector2Int size = GetPrefabSize(selectedPrefab);

            // First, check if the cell has a dropper under the ghost
            Vector3 worldPos = buildGrid.CellToWorld(gridPos);
            Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
            bool dropperFound = false;
            foreach (var hit in hits)
            {
                DropperUII ui = hit.GetComponentInChildren<DropperUII>();
                if (ui != null)
                {
                    Bounds dropperBounds = hit.GetComponentInParent<Dropper>().GetComponent<Collider2D>().bounds;
                    Vector3 ghostCenter = ghostInstance.transform.position;
                    if (dropperBounds.Contains(ghostCenter))
                    {
                        ui.TogglePanel();
                        dropperFound = true;
                        break; // only toggle the correct dropper
                    }
                }
            }

            if (dropperFound)
                return; // skip placement, only toggled the panel

            // Normal occupancy check for all other prefabs
            if (IsAreaOccupied(gridPos, size))
            {
                Debug.Log("Cell already occupied!");
                return;
            }

            // Place prefab normally
            PlacePrefabAtGrid(selectedPrefab, gridPos, rotation);
            KeepGhostAtLastPlacement(gridPos);
        }
    }

    private void UpdateGhost()
    {
        if (ghostInstance == null) return;

        Vector2Int size = GetPrefabSize(selectedPrefab);
        Vector3 worldPos = buildGrid.CellToWorld(gridPos) +
                           new Vector3(size.x, size.y, 0) * 0.5f * buildGrid.cellSize.x;
        ghostInstance.transform.position = worldPos;
        ghostInstance.transform.rotation = Quaternion.Euler(0, 0, rotation);
    }

    private Vector2Int GetPrefabSize(GameObject prefab)
    {
        if (prefab == null) return Vector2Int.one;

        // Gates are 2x2
        if (prefab == bufferGatePrefab || prefab == andGatePrefab || prefab == nandGatePrefab ||
            prefab == norGatePrefab || prefab == notGatePrefab || prefab == orGatePrefab ||
            prefab == xnorGatePrefab || prefab == xorGatePrefab)
            return new Vector2Int(2, 2);

        return Vector2Int.one;
    }

    // ---------------- Generic Placement ----------------
    public GameObject PlacePrefabAtGrid(GameObject prefab, Vector3Int cell, int rotation = 0)
    {
        if (IsAreaOccupied(cell, GetPrefabSize(prefab))) return null;

        Vector2Int size = GetPrefabSize(prefab);
        Vector3 worldPos = buildGrid.CellToWorld(cell) +
                           new Vector3(size.x, size.y, 0) * 0.5f * buildGrid.cellSize.x;

        GameObject placed = Instantiate(prefab, worldPos, Quaternion.Euler(0, 0, rotation));

        // Ensure all child colliders are triggers for corner conveyors
        foreach (var col in placed.GetComponentsInChildren<Collider2D>())
        {
            if (prefab == cornerConveyorPrefab || prefab == corner2ConveyorPrefab)
                col.isTrigger = true;
        }

        // Conveyor setup
        if (placed.TryGetComponent<Conveyor>(out Conveyor conv))
        {
            int rotMod = ((rotation % 360) + 360) % 360;

            if (prefab == conveyorPrefab)
            {
                conv.type = Conveyor.ConveyorType.Straight;
                switch (rotMod)
                {
                    case 0: conv.direction = Vector2.right; break;
                    case 90: conv.direction = Vector2.up; break;
                    case 180: conv.direction = Vector2.left; break;
                    case 270: conv.direction = Vector2.down; break;
                }
                conv.UpdateRotation();
            }
            else if (prefab == cornerConveyorPrefab)
            {
                conv.type = Conveyor.ConveyorType.Corner_LTD;
                placed.transform.rotation = Quaternion.Euler(0, 0, rotMod);
            }
            else if (prefab == corner2ConveyorPrefab)
            {
                conv.type = Conveyor.ConveyorType.Corner_DTL;
                placed.transform.rotation = Quaternion.Euler(0, 0, rotMod);
            }
        }

        // Dropper setup
        if (placed.TryGetComponent<Dropper>(out Dropper dropper))
        {
            Transform sp = placed.transform.Find("SpawnPoint");
            if (sp != null) dropper.spawnPoint = sp;
            dropper.SetDirectionFromRotation(rotation);

            // Force sprite visible
            if (placed.TryGetComponent<SpriteRenderer>(out var sr))
                sr.enabled = true;

            // Setup DropperUII
            DropperUII dropperUI = placed.GetComponentInChildren<DropperUII>();
            if (dropperUI != null)
            {
                dropperUI.Setup(dropper);   // <- this is crucial
                dropperUI.panel.SetActive(false);
            }
        }




        // Mark occupied cells
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                occupiedCells.Add(cell + new Vector3Int(x, y, 0));

        return placed;
    }


    private bool IsAreaOccupied(Vector3Int startCell, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                if (occupiedCells.Contains(startCell + new Vector3Int(x, y, 0)))
                    return true;

        return false;
    }

    public void KeepGhostAtLastPlacement(Vector3Int lastCell)
    {
        gridPos = lastCell;
        UpdateGhost();
    }

    // ---------------- Delete ----------------
    private void HandleDeletion()
    {
        if (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.Backspace))
        {
            GameObject obj = GetPlacedObjectAtGrid(gridPos);
            if (obj != null)
                RemovePlacedObject(obj);
        }
    }

    private GameObject GetPlacedObjectAtGrid(Vector3Int cell)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(buildGrid.CellToWorld(cell));
        foreach (var hit in hits)
        {
            if (hit.gameObject != ghostInstance)
                return hit.gameObject;
        }
        return null;
    }

    private void RemovePlacedObject(GameObject obj)
    {
        Vector2Int size = GetPrefabSize(obj);
        Vector3Int cell = buildGrid.WorldToCell(obj.transform.position) - new Vector3Int(size.x / 2, size.y / 2, 0);

        for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
                occupiedCells.Remove(cell + new Vector3Int(x, y, 0));

        Destroy(obj);
    }

    // ---------------- Corner Placement Check ----------------
    private bool CanPlaceCorner(Vector3Int cell)
    {
        // Allow if the cell is not occupied (removed extra restrictions)
        return !IsAreaOccupied(cell, Vector2Int.one);
    }

    void Update()
    {
        if (FindObjectOfType<BuildModeManager>().IsInBuildMenu())
            return;
        HandleMovement();
        HandleRotation();
        HandlePlacement();
        UpdateGhost();
        HandleDeletion();
    }
    
}
