using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public Button pauseRetryButton;
    public Button pauseResumeButton;
    public Button pauseBackButton;
    public GameObject fuelBar;
    public GameObject miniMapPanel;
    public GameObject pausePanel;
    public GameObject mapPanel;
    public GameObject mapFragmentsPanel;
    public GameObject buttonPanel;
    public Player player;
    public GameObject fragmentPanelPrefab;
    public GameObject infoPanel;
    public TextMeshProUGUI discoveryText;
    public TextMeshProUGUI tryCountText;
    public TextMeshProUGUI resetText;
    public GameObject infoTextPrefab;
    private bool batteryLevelBlinking;
    private Quaternion initialRotation;
    private int totalInfoText = 0;
    public Sprite wallSprite;
    public Sprite obstacleSprite;
    public Sprite exitSprite;
    public Sprite floorSprite;
    public Sprite playerSprite;

    public float minDistanceMiniMap = 5;

    private void Awake() {
        Cursor.visible = false;

        retryButton.onClick.AddListener(gameManager.Retry);
        giveUpButton.onClick.AddListener(gameManager.GiveUp);
    }

    private void Start() {
        player = gameManager.player.GetComponent<Player>();
        DrawStartingMiniMap();
        tryCountText.text = $"Try number {gameManager.tryCount} / {gameManager.tryMax}";
    }

    private void Update() {
        RotateMiniMap();
        player.fuelCount = player.GetComponent<Player>().fuelCount;
        if (player.fuelCount >= 0) {
            UpdateBatteryLevel();
            if (player.fuelCount <= gameManager.playerData.batteryMax * 0.5 && !batteryLevelBlinking)
                StartCoroutine(BlinkBatteryLevel());
        }

        if (player.fuelCount <= 0 && !batteryDead.gameObject.activeSelf) {
            batteryDead.SetActive(true);
            Cursor.visible = true;
            StopCoroutine(nameof(BlinkBatteryLevel));
            buttonPanel.SetActive(true);
            if (gameManager.tryCount >= gameManager.tryMax) {
                batteryDead.SetActive(false);
                resetText.gameObject.SetActive(true);
                GameObject.Find("GiveUpText").GetComponent<TextMeshProUGUI>().text = "Fuck off and die";
            }

            Cursor.lockState = CursorLockMode.None;
        }

        // RotateMiniMap();
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
        Vector3 localScale = fuelBar.transform.localScale;
        localScale = new Vector3(
            player.fuelCount / gameManager.playerData.batteryMax,
            localScale.y,
            localScale.z
        );
        fuelBar.transform.localScale = localScale;
        // Update color
        fuelBar.GetComponent<Image>().color = player.fuelCount > gameManager.playerData.batteryMax / 2
            ? new Color32((byte) (gameManager.playerData.batteryMax - player.fuelCount), 255, 0, 255)
            : new Color32(255, (byte) player.fuelCount, 0, 255);
    }

    private void DrawStartingMiniMap() {
        AddTileToMiniMap(gameManager.startingTile);
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
        GameObject newTile =
            new GameObject($"Map_{tile.gameObject.name}");
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

    private IEnumerator BlinkBatteryLevel() {
        batteryLevelBlinking = true;
        TextMeshProUGUI batteryText = batteryLevel.GetComponent<TextMeshProUGUI>();
        while (true) {
            if (player.fuelCount > gameManager.playerData.batteryMax * 0.2) {
                batteryText.text = "BATTERY LEVEL LOW";
                batteryLevel.SetActive(true);
                yield return new WaitForSeconds(0.4f);
                batteryLevel.SetActive(false);
                yield return new WaitForSeconds(0.4f);
            }
            else {
                batteryText.text = "BATTERY LEVEL CRITICAL";
                batteryLevel.SetActive(true);
                yield return new WaitForSeconds(0.2f);
                batteryLevel.SetActive(false);
                yield return new WaitForSeconds(0.2f);
            }
        }
    }


    public void ShowPauseUi() {
        pausePanel.SetActive(true);
        pauseResumeButton.onClick.AddListener(HidePauseUi);
        pauseRetryButton.onClick.AddListener(gameManager.Retry);
        pauseBackButton.onClick.AddListener(gameManager.GiveUp);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        player.GetComponent<Player>().lockPlayer = true;
    }

    public void HidePauseUi() {
        gameManager.gameIsPaused = false;
        pausePanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        player.GetComponent<Player>().lockPlayer = false;
    }

    public void ShowExitUi() {
        exitReached.SetActive(true);
        exitReachedButtons.SetActive(true);
        nextLevelButton.onClick.AddListener(gameManager.NextLevel);
        backToMenuButton.onClick.AddListener(gameManager.BackToMenu);
        // set posX of ack to menu to 0
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        player.GetComponent<Player>().lockPlayer = true;
        if (SceneManager.GetActiveScene().name == "Level3") {
            exitReached.GetComponent<TextMeshProUGUI>().text =
                "Congrats beta tester, you've been through all the levels !!";
            exitReached.GetComponent<TextMeshProUGUI>().fontSize = 17;
            nextLevelButton.gameObject.SetActive(false);
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