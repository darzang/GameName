﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    private TileManager tileManager;
    private GameManager gameManager;
    [HideInInspector] public TextMeshProUGUI onboardingText;
    private GameObject batteryLevelText;
    private GameObject batteryDeadText;
    private GameObject exitReachedText;
    private GameObject exitReachedButtons;
    private Button retryButton;
    private Button nextLevelButton;
    private Button giveUpButton;
    private Button backToMenuButton;
    private Button pauseRetryButton;
    private Button pauseResumeButton;
    private Button pauseBackButton;
    private GameObject batteryBar;
    private Image batteryBarImage;
    private GameObject miniMapPanel;
    private Player player;
    private GameObject infoPanel;
    private TextMeshProUGUI discoveryText;
    private TextMeshProUGUI tryCountText;
    private TextMeshProUGUI batteryText;


    private Quaternion initialRotation;
    private int totalInfoText = 0;
    public Sprite wallSprite;
    public Sprite obstacleSprite;
    public Sprite exitSprite;
    public Sprite floorSprite;
    public Sprite playerSprite;
    public GameObject infoTextPrefab;

    public float minDistanceMiniMap = 5;

    private GameObject mainCanvas;
    private GameObject pauseCanvas;
    private GameObject mapCanvas;
    private GameObject miniMapCanvas;
    private GameObject batteryBarCanvas;
    private GameObject infoCanvas;
    private GameObject exitReachedCanvas;
    private GameObject batteryDeadCanvas;
    private GameObject onboardingCanvas;

    private bool batteryLevelBlinking = false;

    private void Awake() {
        Cursor.visible = false;

        // Variables instantiation
        mainCanvas = GameObject.Find("MainCanvas").gameObject;
        pauseCanvas = mainCanvas.transform.Find("PauseCanvas").gameObject;
        mapCanvas = mainCanvas.transform.Find("MapCanvas").gameObject;
        discoveryText = mapCanvas.transform.Find("DiscoveryText").GetComponent<TextMeshProUGUI>();
        tryCountText = mapCanvas.transform.Find("TryCountText").GetComponent<TextMeshProUGUI>();
        miniMapCanvas = mainCanvas.transform.Find("MiniMapCanvas").gameObject;
        miniMapPanel = miniMapCanvas.transform.Find("MiniMapPanel").gameObject;
        batteryBarCanvas = mainCanvas.transform.Find("BatteryBarCanvas").gameObject;
        infoCanvas = mainCanvas.transform.Find("InfoCanvas").gameObject;
        infoPanel = infoCanvas.transform.Find("InfoPanel").gameObject;
        exitReachedCanvas = mainCanvas.transform.Find("ExitReachedCanvas").gameObject;
        batteryDeadCanvas = mainCanvas.transform.Find("BatteryDeadCanvas").gameObject;
        exitReachedText = exitReachedCanvas.transform.Find("ExitReachedText").gameObject;
        exitReachedButtons = exitReachedCanvas.transform.Find("ExitReachedButtons").gameObject;
        nextLevelButton = exitReachedButtons.transform.Find("NextLevelButton").GetComponent<Button>();
        backToMenuButton = exitReachedButtons.transform.Find("BackToMenuButton").GetComponent<Button>();
        giveUpButton = batteryDeadCanvas.transform.Find("BatteryDeadButtons").transform.Find("GiveUpButton")
            .GetComponent<Button>();
        retryButton = batteryDeadCanvas.transform.Find("BatteryDeadButtons").transform.Find("RetryButton")
            .GetComponent<Button>();
        pauseBackButton = pauseCanvas.transform.Find("PauseBackButton").GetComponent<Button>();
        pauseResumeButton = pauseCanvas.transform.Find("PauseResumeButton").GetComponent<Button>();
        pauseRetryButton = pauseCanvas.transform.Find("PauseRetryButton").GetComponent<Button>();
        onboardingCanvas = mainCanvas.transform.Find("OnboardingCanvas").gameObject;
        onboardingText = onboardingCanvas.transform.Find("OnboardingText").GetComponent<TextMeshProUGUI>();

        batteryDeadText = batteryDeadCanvas.transform.Find("BatteryDeadText").gameObject;
        batteryLevelText = batteryBarCanvas.transform.Find("BatteryLevelText").gameObject;
        batteryText = batteryLevelText.GetComponent<TextMeshProUGUI>();
        batteryBar = batteryBarCanvas.transform.Find("BatteryBar").gameObject;
        batteryBarImage = batteryBar.GetComponent<Image>();
    }

    public void Instantiation() {
        // Managers
        tileManager = GameObject.Find("TileManager").GetComponent<TileManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        // Button listeners
        player = gameManager.player.GetComponent<Player>();
        retryButton.onClick.AddListener(gameManager.Retry);
        giveUpButton.onClick.AddListener(gameManager.GiveUp);
        pauseResumeButton.onClick.AddListener(ResumeGame);
        pauseRetryButton.onClick.AddListener(gameManager.Retry);
        pauseBackButton.onClick.AddListener(gameManager.GiveUp);
        if (gameManager.totalDiscoveredTiles.Count > 0) DrawMap(gameManager.totalDiscoveredTiles);
        tryCountText.text = $"Try number {gameManager.tryCount} / {gameManager.tryMax}";
        UpdateDiscoveryText(gameManager.totalDiscoveredTiles.Count, tileManager.GetMapSize());
        if (gameManager.totalDiscoveredTiles.Count > 0) AddInfoMessage("Previous data loaded");
    }

    private void Update() {
        RotateMiniMap();
        player.fuelCount = player.GetComponent<Player>().fuelCount;
        if (player.fuelCount > 0) {
            UpdateBatteryLevel();
            if (player.fuelCount > gameManager.playerData.batteryMax * 0.5) {
                batteryText.text = "";
            }
            else if (player.fuelCount > gameManager.playerData.batteryMax * 0.2) {
                batteryText.text = "BATTERY LEVEL LOW";
                if (!batteryLevelBlinking) StartCoroutine(BlinkBatteryLevel(0.5f));
            }
            else {
                batteryText.text = "BATTERY LEVEL CRITICAL";
                if (!batteryLevelBlinking) StartCoroutine(BlinkBatteryLevel(0.25f));
            }
        }
        else if (!batteryDeadCanvas.activeSelf) {
            Cursor.visible = true;
            batteryLevelText.SetActive(false);
            StopCoroutine(nameof(BlinkBatteryLevel));
            batteryDeadCanvas.SetActive(true);
            if (gameManager.tryCount >= gameManager.tryMax) {
                batteryDeadText.GetComponent<TextMeshProUGUI>().fontSize = 18;
                batteryDeadText.GetComponent<TextMeshProUGUI>().text =
                    "Sorry, You die too much, you should be punished...\nHow about erasing your current level progress ?\nYeah that sounds nice, Let's do that !";
                GameObject.Find("GiveUpText").GetComponent<TextMeshProUGUI>().text = "Fuck off and die";
            }

            Cursor.lockState = CursorLockMode.None;
        }
    }


    public void HideCanvas() {
        mainCanvas.SetActive(false);
    }

    private void RotateMiniMap() {
        float angle = player.transform.eulerAngles.y + 180;
        Image[] tiles = miniMapPanel.GetComponentsInChildren<Image>();
        foreach (Image tile in tiles) tile.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        miniMapPanel.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void UpdateMiniMap() {
        foreach (GameObject tile in gameManager.revealedTilesInRun) {
            if (tile) AddTileToMiniMap(tile);
        }
    }

    private void UpdateBatteryLevel() {
        // Update scale
        Vector3 localScale = batteryBar.transform.localScale;
        localScale = new Vector3(
            player.fuelCount / gameManager.playerData.batteryMax,
            localScale.y,
            localScale.z
        );
        batteryBar.transform.localScale = localScale;
        // Update color
        batteryBarImage.color = player.fuelCount > gameManager.playerData.batteryMax / 2
            ? new Color32((byte) (gameManager.playerData.batteryMax - player.fuelCount), 255, 0, 255)
            : new Color32(255, (byte) player.fuelCount, 0, 255);
    }

    public IEnumerator OnboardingBlinkBattery() {
        float targetTime = 3.0f;
        while (targetTime >= 0.0f) {
            batteryBarImage.color = batteryBarImage.color == Color.red ? Color.yellow : Color.red;
            targetTime -= Time.deltaTime;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void AddTileToMiniMap(GameObject tile) {
        GameObject existingMiniMapTile = GameObject.Find($"MiniMap_{tile.gameObject.name}");

        float distance = Vector3.Distance(tile.transform.position, player.gameObject.transform.position);
        if (distance > minDistanceMiniMap) {
            // TODO: Maybe just disabled then later replace + reenable ?
            if (existingMiniMapTile) {
                existingMiniMapTile.GetComponent<Image>().enabled = false;
            }
        }
        else {
            if (!tileManager.HasBeenRevealed(tile, gameManager.revealedTilesInRun)) {
                tileManager.AddToRevealedTiles(tile, gameManager.revealedTilesInRun);
            }

            // Instantiate new tile and anchor it in the middle of the panel

            if (existingMiniMapTile) {
                GameObject tileObject = GameObject.Find(existingMiniMapTile.name.Substring(8));
                existingMiniMapTile.GetComponent<Image>().enabled = true;
                if (tileObject == tileManager.GetTileUnderPlayer()) {
                    existingMiniMapTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                    existingMiniMapTile.GetComponent<Image>().sprite = playerSprite;
                }
                else {
                    if (tileObject == gameManager.previousTile) {
                        existingMiniMapTile.GetComponent<Image>().sprite = floorSprite;
                    }

                    existingMiniMapTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(
                        tileManager.GetRelativePosition(tileManager.GetTileUnderPlayer(), tile)[0] * 10,
                        tileManager.GetRelativePosition(tileManager.GetTileUnderPlayer(), tile)[1] * 10,
                        0);
                }
            }
            else {
                Sprite tileSprite = GetTileSprite(tile.tag);
                GameObject newTile = new GameObject($"MiniMap_{tile.gameObject.name}");
                Image newImage = newTile.AddComponent<Image>();
                newTile.GetComponent<RectTransform>().SetParent(miniMapPanel.transform);
                newTile.GetComponent<RectTransform>().SetAsFirstSibling();
                newTile.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                newTile.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                newTile.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, 0f);

                // Set the position of the new tile
                if (tile == tileManager.GetTileUnderPlayer()) {
                    newTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                    tileSprite = playerSprite;
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
                newImage.sprite = tileSprite;

                // if (tile == gameManager.startingTile) DrawSpawnTileInFragment(newTile);
                newTile.SetActive(true);
            }
        }
    }

    private void AddTileToMap(GameObject tile) {
        /*
        Was used when drawing the whole map at once,
        Basically only the anchor is different
         */
        Sprite tileSprite = GetTileSprite(tile.tag);
        Vector3 position = tile.tag == "Player"
            ? gameManager.currentTile.transform.position
            : tile.transform.position;
        GameObject existingTile = GameObject.Find($"Map_{tile.gameObject.name}");
        if (existingTile) Destroy(existingTile);
        GameObject newTile =
            new GameObject($"Map_{tile.gameObject.name}");
        Image newImage = newTile.AddComponent<Image>();
        newTile.GetComponent<RectTransform>().SetParent(mapCanvas.transform);

        newTile.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        newTile.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        newTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(
            position.x * 10 + 5,
            position.z * 10 + 5,
            0
        );
        newTile.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 10);
        newTile.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
        // newImage.color = tileColor;
        newImage.sprite = tileSprite;
        newTile.SetActive(true);
    }

    public void DrawMap(List<string> tiles) {
        foreach (string tileName in tiles) {
            GameObject tile = GameObject.Find(tileName);
            if (tile == gameManager.currentTile) {
                AddTileToMap(gameManager.player);
                continue;
            }

            AddTileToMap(tile);
        }
    }

    private Sprite GetTileSprite(string tileTag) {
        switch (tileTag) {
            case "Wall":
                return wallSprite;
            case "Floor":
                return floorSprite;
            case "Obstacle":
                return obstacleSprite;
            case "Player":
                return playerSprite;
            case "Exit":
                return exitSprite;
            default:
                Debug.LogError("TAG_NOT_FOUND_FOR_TILE: " + tileTag);
                return floorSprite;
        }
    }

    private IEnumerator BlinkBatteryLevel(float timeToWait) {
        batteryLevelBlinking = true;
        batteryLevelText.SetActive(true);
        yield return new WaitForSeconds(timeToWait);
        batteryLevelText.SetActive(false);
        yield return new WaitForSeconds(timeToWait);
        batteryLevelBlinking = false;
    }


    public void ShowPauseUi() {
        pauseCanvas.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        player.GetComponent<Player>().lockPlayer = true;
    }

    public void ResumeGame() {
        gameManager.gameIsPaused = false;
        pauseCanvas.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        player.GetComponent<Player>().lockPlayer = false;
    }

    public void ShowExitUi() {
        exitReachedText.SetActive(true);
        exitReachedButtons.SetActive(true);
        nextLevelButton.onClick.AddListener(gameManager.NextLevel);
        backToMenuButton.onClick.AddListener(gameManager.BackToMenu);
        // set posX of ack to menu to 0
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        player.GetComponent<Player>().lockPlayer = true;
        if (SceneManager.GetActiveScene().name == "Level10") {
            exitReachedText.GetComponent<TextMeshProUGUI>().text =
                "Congrats beta tester, you've been through all the levels !!";
            exitReachedText.GetComponent<TextMeshProUGUI>().fontSize = 17;
            backToMenuButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        }
    }

    public void UpdateDiscoveryText(int discoveredTiles, int mapSize) {
        if (discoveredTiles > 0) {
            discoveryText.text = $"Discovered {Math.Round((double) discoveredTiles / mapSize * 100)}%";
        }
        else {
            discoveryText.text = "";
        }
    }

    public void AddInfoMessage(string message) {
        // Handle position of previous texts if any 
        if (infoPanel.transform.childCount > 0) {
            foreach (Transform previousText in infoPanel.transform) {
                RectTransform rectTransform = previousText.GetComponent<RectTransform>();
                if (rectTransform.anchoredPosition.y == 80) {
                    Destroy(previousText.gameObject);
                }

                rectTransform.anchoredPosition = new Vector3(0, rectTransform.anchoredPosition.y + 20, 0);
            }
        }

        // Instantiate new text
        GameObject infoText = Instantiate(infoTextPrefab, infoPanel.transform.position,
            infoPanel.transform.rotation, infoPanel.transform);
        totalInfoText++;
        infoText.name = $"InfoText{totalInfoText}";
        TextMeshProUGUI text = infoText.GetComponent<TextMeshProUGUI>();
        text.text = message;

        // Adjust Position
        infoText.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
        infoText.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
        infoText.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);
        infoText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);

        // Destroy it later
        Destroy(infoText, 5f);
    }
}