﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public TileManager tileManager;
    public GameManager gameManager;
    public GameObject batteryDead;
    public GameObject exitReached;
    public GameObject exitReachedButtons;
    public GameObject batteryLevel;
    public Button retryButton;
    public Button nextLevelButton;
    public Button giveUpButton;
    public Button backToMenuButton;
    public GameObject fuelBar;
    public GameObject miniMapPanel;
    public GameObject mapPanel;
    public GameObject mapFragmentsPanel;
    public GameObject buttonPanel;
    public GameObject player;
    public GameObject fragmentPanelPrefab;
    readonly Color32 floorColor = new Color32(255, 255, 255, 255);
    readonly Color32 wallColor = new Color32(25, 25, 25, 255);
    readonly Color32 obstacleColor = new Color32(50, 50, 50, 255);
    readonly Color32 playerColor = new Color32(0, 0, 255, 255);
    readonly Color32 exitColor = new Color32(255, 0, 0, 255);
    readonly Color32 spawnTextColor = new Color32(0, 0, 0, 255);
    double fuelTank;
    double fuelCount;
    private bool batteryLevelBlinking;
    private Quaternion initialRotation;

    private void Awake() {
        retryButton.onClick.AddListener(gameManager.Retry);
        giveUpButton.onClick.AddListener(gameManager.GiveUp);
    }

    private void Start() {
        player = gameManager.player;
        fuelTank = player.GetComponent<Player>().fuelTank;
        DrawStartingMiniMap();
    }

    private void Update() {
        RotateMiniMap();
        fuelCount = player.GetComponent<Player>().fuelCount;
        if (fuelCount >= 0) {
            UpdateBatteryLevel();
            if (fuelCount == 0 && !batteryDead.gameObject.activeSelf) {
                batteryDead.SetActive(true);
                StopCoroutine(nameof(BlinkBatteryLevel));
                buttonPanel.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
            }
        }
        if (fuelCount <= fuelTank * 0.5 && !batteryLevelBlinking) StartCoroutine(BlinkBatteryLevel());
        RotateMiniMap();
    }

    private void RotateMiniMap() {
        float angle = player.transform.eulerAngles.y + 180;
        Image[] tiles=  miniMapPanel.GetComponentsInChildren<Image>();
        foreach (Image tile in tiles) tile.transform.rotation = Quaternion.Euler(0f,0f,0f);
        miniMapPanel.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void UpdateMiniMap() {
        foreach (GameObject tile in gameManager.revealedTiles) AddTileToMiniMap(tile);
    }

    private void UpdateBatteryLevel() {
        // Update scale
        Vector3 localScale = fuelBar.transform.localScale;
        localScale = new Vector3(
            (float) (fuelCount / fuelTank),
            localScale.y,
            localScale.z
        );
        fuelBar.transform.localScale = localScale;
        // Update color
        fuelBar.GetComponent<Image>().color = fuelCount > fuelTank / 2
            ? new Color32((byte) (fuelTank - fuelCount), 255, 0, 255)
            : new Color32(255, (byte) fuelCount, 0, 255);
    }

    private void DrawStartingMiniMap() {
        AddTileToMiniMap(gameManager.startingTile);
    }

    private void AddTileToMiniMap(GameObject tile) {
        // Regenerate previously drawn tiles
        Vector3 position = tile.transform.position;
        if (tileManager.HasBeenRevealed(tile, gameManager.revealedTiles)) {
            Destroy(GameObject.Find($"MiniMap_{position.x}_{position.z}_{tile.tag}"));
        }

        if (!tileManager.HasBeenRevealed(tile, gameManager.revealedTiles))
            tileManager.AddToRevealedTiles(tile, gameManager.revealedTiles);

        // Instantiate new tile and anchor it in the middle of the panel
        GameObject newTile =
            new GameObject($"MiniMap_{position.x}_{position.z}_{tile.tag}");
        Image newImage = newTile.AddComponent<Image>();
        Color32 tileColor = GetTileColor(tile.tag);

        newTile.GetComponent<RectTransform>().SetParent(miniMapPanel.transform);
        newTile.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        newTile.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        newTile.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f,0f,0f);

        // Set the position of the new tile
        if (tile == tileManager.GetTileUnderPlayer()) {
            newTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
            tileColor = playerColor;
        }
        else {
            newTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(
                tileManager.GetRelativePosition(tileManager.GetTileUnderPlayer(), tile)[0] * 10,
                tileManager.GetRelativePosition(tileManager.GetTileUnderPlayer(), tile)[1] * 10,
                0);
        }

        // Set the size and scale of the tile
        newTile.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 10);
        newTile.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
        newImage.color = tileColor;

        if (tile == gameManager.startingTile) DrawSpawnTileInFragment(newTile);
        newTile.SetActive(true);
    }

    private void DrawSpawnTileInFragment(GameObject tile, int fragmentNumber = 0) {
        GameObject spawnTile = new GameObject("SpawnTile");
        spawnTile.transform.parent = tile.transform;
        spawnTile.AddComponent<TextMeshProUGUI>();
        TextMeshProUGUI spawnText = spawnTile.GetComponent<TextMeshProUGUI>();
        spawnText.text = "S";
        if (fragmentNumber > 0) spawnText.text += fragmentNumber;
        spawnText.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        spawnText.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        spawnText.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        spawnText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        spawnText.GetComponent<RectTransform>().sizeDelta = tile.GetComponent<RectTransform>().sizeDelta;
        spawnText.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        spawnText.fontSize = 8 * tile.GetComponent<RectTransform>().sizeDelta.x / 10;
        spawnText.color = spawnTextColor;
        spawnText.alignment = TextAlignmentOptions.MidlineJustified;
    }

    private void AddTileToMap(GameObject tile) {
        /*
        Was used when drawing the whole map at once,
        Basically only the anchor is different
         */
        Color32 tileColor = GetTileColor(tile.tag);
        Vector3 position = tile.transform.position;
        GameObject newTile =
            new GameObject($"Map_{position.x}_{position.z}_{tile.tag}");
        Image newImage = newTile.AddComponent<Image>();
        newTile.GetComponent<RectTransform>().SetParent(mapPanel.transform);

        newTile.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        newTile.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        newTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(
            position.x * 10 + 5,
            position.z * 10 + 5,
            0
        );
        newTile.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 10);
        newTile.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
        newImage.color = tileColor;
        newTile.SetActive(true);
    }

    private void DrawWholeMap(GameObject[,] map2D)
    {
        RectTransform rect = mapPanel.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2((map2D.GetLength(0) + 1) * 10, (map2D.GetLength(1) + 1) * 10);
        for (int i = 0; i < map2D.GetLength(0); i++) {
            for (int j = 0; j < map2D.GetLength(1); j++) {
                AddTileToMap(map2D[i, j]);
            }
        }
    }

    public void DrawMap(List<string> tiles) {
        foreach (string tileName in tiles) {
            GameObject tile = GameObject.Find(tileName);
            AddTileToMap(tile);
        }
    }
    
    

    private Color32 GetTileColor(string tileTag) {
        switch (tileTag) {
            case "Wall":
                return wallColor;
            case "Floor":
                return floorColor;
            case "Obstacle":
                return obstacleColor;
            case "Player":
                return playerColor;
            case "Exit":
                return exitColor;
            default:
                Debug.Log("TAG_NOT_FOUND_FOR_TILE: " + tileTag);
                return floorColor;
        }
    }

    private IEnumerator BlinkBatteryLevel() {
        batteryLevelBlinking = true;
        TextMeshProUGUI batteryText = batteryLevel.GetComponent<TextMeshProUGUI>();
        while (true) {
            if (fuelCount > fuelTank * 0.2) {
                batteryText.text = "BATTERY LEVEL LOW";
                batteryLevel.SetActive(true);
                yield return new WaitForSeconds(0.4f);
                batteryLevel.SetActive(false);
                yield return new WaitForSeconds(0.4f);
            } else {
                batteryText.text = "BATTERY LEVEL CRITICAL";
                batteryLevel.SetActive(true);
                yield return new WaitForSeconds(0.2f);
                batteryLevel.SetActive(false);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    public void DrawMapFragments(List<Fragment> mapFragments) {
        if (mapFragmentsPanel.transform.childCount > 0) {
            foreach (Transform panel in mapFragmentsPanel.transform) Destroy(panel.gameObject);
        }
        int fragmentNumber = 1;
        foreach (Fragment fragment in mapFragments.Where(fragment => fragment.discovered)) {
            DrawMapFragment(fragment.tiles, fragmentNumber);
            fragmentNumber++;
        }
    }

    private void DrawMapFragment(List<string> tiles, int fragmentNumber) {
        // Instantiate fragment panel
        GameObject fragmentPanel = Instantiate(fragmentPanelPrefab, new Vector3(0, 0, 0),
            mapFragmentsPanel.transform.rotation, mapFragmentsPanel.transform);

        fragmentPanel.GetComponent<RectTransform>().SetParent(mapFragmentsPanel.transform);
        switch (fragmentNumber) {
            case 1:
                fragmentPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                fragmentPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                fragmentPanel.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                break;
            case 2:
                fragmentPanel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
                fragmentPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
                fragmentPanel.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
                break;
            case 3:
                fragmentPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0.5f);
                fragmentPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0.5f);
                fragmentPanel.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                break;
            case 4:
                fragmentPanel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0.5f);
                fragmentPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.5f);
                fragmentPanel.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);
                break;
            case 5:
                fragmentPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                fragmentPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
                fragmentPanel.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
                break;
            case 6:
                fragmentPanel.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
                fragmentPanel.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
                fragmentPanel.GetComponent<RectTransform>().pivot = new Vector2(1, 0);
                break;
            default:
                Debug.LogError("Fragment number case not found for " + fragmentNumber);
                break;
        }

        fragmentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        fragmentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
        fragmentPanel.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
        foreach (string tileName in tiles) {
            AddTileToFragment(GameObject.Find(tileName), fragmentPanel, fragmentNumber);
        }
    }

    private void AddTileToFragment(GameObject tile, GameObject panel, int fragmentNumber) {
        // Instantiate new tile and anchor it in the middle of the panel
        Vector3 position = tile.transform.position;
        GameObject newTile =
            new GameObject($"Fragment_{position.x}_{position.z}_{tile.tag}");

        Image newImage = newTile.AddComponent<Image>();
        Color32 tileColor = GetTileColor(tile.tag);

        newTile.GetComponent<RectTransform>().SetParent(panel.transform);
        newTile.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        newTile.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);

        // Get spawn tile associated to fragment number
        // GameObject spawnTileOfFragment = GameObject.Find(gameManager.spawnTilesString.ElementAt(fragmentNumber - 1));
        // newTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(
            // tileManager.GetRelativePosition(spawnTileOfFragment, tile)[0] * 5,
            // tileManager.GetRelativePosition(spawnTileOfFragment, tile)[1] * 5,
            // 0);

        // Set the size and scale of the tile
        newTile.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 5);
        newTile.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
        newImage.color = tileColor;

        if (gameManager.IsPreviousSpawnTile(tile)) {
            DrawSpawnTileInFragment(newTile, gameManager.GetSpawnTileTryNumber(tile));
        }

        newTile.SetActive(true);
    }
    
    public void ShowExitUi() {
        exitReached.SetActive(true);
        exitReachedButtons.SetActive(true);
        nextLevelButton.onClick.AddListener(gameManager.NextLevel);
        backToMenuButton.onClick.AddListener(gameManager.BackToMenu);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        player.GetComponent<Player>().lockPlayer = true;
    }
}
