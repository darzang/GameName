using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviour {
    private MazeCellManager _mazeCellManager;
    private GameManager _gameManager;
    [HideInInspector] public TextMeshProUGUI onboardingText;
    private GameObject _batteryLevelText;
    private TextMeshProUGUI _batteryDeadText;
    private GameObject _exitReachedText;
    private GameObject _exitReachedButtons;
    private Button _retryButton;
    private Button _nextLevelButton;
    private Button _giveUpButton;
    private Button _backToMenuButton;
    private Button _pauseRetryButton;
    private Button _pauseResumeButton;
    private Button _pauseBackButton;
    private GameObject _batteryBar;
    private Image _batteryBarImage;
    private GameObject _miniMapPanel;
    private Player _player;
    private GameObject _infoPanel;
    private TextMeshProUGUI _discoveryText;
    private TextMeshProUGUI _tryCountText;
    private TextMeshProUGUI _batteryText;


    private Quaternion _initialRotation;
    private int _totalInfoText;
    public Sprite wallSprite;
    public Sprite obstacleSprite;
    public Sprite exitSprite;
    public Sprite floorSprite;
    public Sprite playerSprite;
    public GameObject infoTextPrefab;

    public float minDistanceMiniMap = 5;

    private GameObject _mainCanvas;
    private GameObject _pauseCanvas;
    private GameObject _mapCanvas;
    private GameObject _miniMapCanvas;
    private GameObject _batteryBarCanvas;
    private GameObject _infoCanvas;
    private GameObject _exitReachedCanvas;
    private GameObject _batteryDeadCanvas;
    private GameObject _onboardingCanvas;

    private bool _batteryLevelBlinking;
    [HideInInspector] public bool batteryOnboardingBlinking;

    private void Awake() {
        Cursor.visible = false;

        // Variables instantiation
        _mainCanvas = GameObject.Find("MainCanvas").gameObject;
        _pauseCanvas = _mainCanvas.transform.Find("PauseCanvas").gameObject;
        _mapCanvas = _mainCanvas.transform.Find("MapCanvas").gameObject;
        _discoveryText = _mapCanvas.transform.Find("DiscoveryText").GetComponent<TextMeshProUGUI>();
        _tryCountText = _mapCanvas.transform.Find("TryCountText").GetComponent<TextMeshProUGUI>();
        _miniMapCanvas = _mainCanvas.transform.Find("MiniMapCanvas").gameObject;
        _miniMapPanel = _miniMapCanvas.transform.Find("MiniMapPanel").gameObject;
        _batteryBarCanvas = _mainCanvas.transform.Find("BatteryBarCanvas").gameObject;
        _infoCanvas = _mainCanvas.transform.Find("InfoCanvas").gameObject;
        _infoPanel = _infoCanvas.transform.Find("InfoPanel").gameObject;
        _exitReachedCanvas = _mainCanvas.transform.Find("ExitReachedCanvas").gameObject;
        _batteryDeadCanvas = _mainCanvas.transform.Find("BatteryDeadCanvas").gameObject;
        _exitReachedText = _exitReachedCanvas.transform.Find("ExitReachedText").gameObject;
        _exitReachedButtons = _exitReachedCanvas.transform.Find("ExitReachedButtons").gameObject;
        _nextLevelButton = _exitReachedButtons.transform.Find("NextLevelButton").GetComponent<Button>();
        _backToMenuButton = _exitReachedButtons.transform.Find("BackToMenuButton").GetComponent<Button>();
        _giveUpButton = _batteryDeadCanvas.transform.Find("BatteryDeadButtons").transform.Find("GiveUpButton")
            .GetComponent<Button>();
        _retryButton = _batteryDeadCanvas.transform.Find("BatteryDeadButtons").transform.Find("RetryButton")
            .GetComponent<Button>();
        _pauseBackButton = _pauseCanvas.transform.Find("PauseBackButton").GetComponent<Button>();
        _pauseResumeButton = _pauseCanvas.transform.Find("PauseResumeButton").GetComponent<Button>();
        _pauseRetryButton = _pauseCanvas.transform.Find("PauseRetryButton").GetComponent<Button>();
        _onboardingCanvas = _mainCanvas.transform.Find("OnboardingCanvas").gameObject;
        onboardingText = _onboardingCanvas.transform.Find("OnboardingText").GetComponent<TextMeshProUGUI>();

        _batteryDeadText = _batteryDeadCanvas.transform.Find("BatteryDeadText").GetComponent<TextMeshProUGUI>();
        _batteryLevelText = _batteryBarCanvas.transform.Find("BatteryLevelText").gameObject;
        _batteryText = _batteryLevelText.GetComponent<TextMeshProUGUI>();
        _batteryBar = _batteryBarCanvas.transform.Find("BatteryBar").gameObject;
        _batteryBarImage = _batteryBar.GetComponent<Image>();
    }

    public void Instantiation() {
        Debug.Log("UI Instantiation");
        // Managers
        _mazeCellManager = GameObject.Find("TileManager").GetComponent<MazeCellManager>();
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        // Button listeners
        _player = _gameManager.player.GetComponent<Player>();
        _mainCanvas.GetComponent<Canvas>().worldCamera = _player.transform.Find("PlayerCamera").GetComponent<Camera>();
        _mainCanvas.GetComponent<Canvas>().planeDistance = 0.1f;
        _retryButton.onClick.AddListener(_gameManager.Retry);
        _giveUpButton.onClick.AddListener(_gameManager.GiveUp);
        _pauseResumeButton.onClick.AddListener(ResumeGame);
        _pauseRetryButton.onClick.AddListener(_gameManager.Retry);
        _pauseBackButton.onClick.AddListener(_gameManager.GiveUp);
        if (_gameManager.totalDiscoveredCellsNames.Count > 0) {
            DrawMap(_gameManager.totalDiscoveredCellsNames);
            GameObject existingTile = GameObject.Find($"Map_MazeCell_6_9");
            Debug.Log("Hey");
        }
        _tryCountText.text = $"Try number {_gameManager.tryCount} / {_gameManager.tryMax}";
        UpdateDiscoveryText(_gameManager.totalDiscoveredCellsNames.Count, _mazeCellManager.GetMapSize());
        if (_gameManager.totalDiscoveredCellsNames.Count > 0) AddInfoMessage("Previous data loaded");
        _nextLevelButton.onClick.AddListener(_gameManager.NextLevel);
        _backToMenuButton.onClick.AddListener(_gameManager.BackToMenu);
    }

    private void Update() {
        RotateMiniMap();
        _player.fuelCount = _player.GetComponent<Player>().fuelCount;
        if (_player.fuelCount > 0) {
            UpdateBatteryLevel();
            if (_player.fuelCount > _gameManager.playerData.batteryMax * 0.5) {
                _batteryText.text = "";
            }
            else if (_player.fuelCount > _gameManager.playerData.batteryMax * 0.2) {
                _batteryText.text = "BATTERY LEVEL LOW";
                if (!_batteryLevelBlinking) StartCoroutine(BlinkBatteryLevel(0.5f));
            }
            else {
                _batteryText.text = "BATTERY LEVEL CRITICAL";
                if (!_batteryLevelBlinking) StartCoroutine(BlinkBatteryLevel(0.25f));
            }
        }
        else if (!_batteryDeadCanvas.activeSelf) {
            Cursor.visible = true;
            _batteryLevelText.SetActive(false);
            StopCoroutine(nameof(BlinkBatteryLevel));
            _batteryDeadCanvas.SetActive(true);
            if (_gameManager.tryCount >= _gameManager.tryMax) {
                _batteryDeadText.fontSize = 18;
                _batteryDeadText.text =
                    "Well, I told you, don't die too much...\n And one more map exploration lost forever...";
            }

            Cursor.lockState = CursorLockMode.None;
        }
    }


    public void HideCanvas() {
        _mainCanvas.SetActive(false);
    }

    private void RotateMiniMap() {
        float angle = _player.transform.eulerAngles.y + 180;
        Image[] tiles = _miniMapPanel.GetComponentsInChildren<Image>();
        foreach (Image tile in tiles) tile.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        _miniMapPanel.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void UpdateMiniMap() {
        foreach (MazeCell cell in _gameManager.revealedCellsInRun) {
            if (cell) AddTileToMiniMap(cell);
        }
    }

    private void UpdateBatteryLevel() {
        // Update scale
        Vector3 localScale = _batteryBar.transform.localScale;
        localScale = new Vector3(
            _player.fuelCount / _gameManager.playerData.batteryMax,
            localScale.y,
            localScale.z
        );
        _batteryBar.transform.localScale = localScale;
        // Update color
        if (!batteryOnboardingBlinking) {
            _batteryBarImage.color = _player.fuelCount > _gameManager.playerData.batteryMax / 2
                ? new Color32((byte) (_gameManager.playerData.batteryMax - _player.fuelCount), 255, 0, 255)
                : new Color32(255, (byte) _player.fuelCount, 0, 255);
        }
    }

    public IEnumerator OnboardingBlinkBattery() {
        batteryOnboardingBlinking = true;
        while (_gameManager.onboardingStage == 2) {
            _batteryBarImage.color = _batteryBarImage.color == Color.red ? Color.yellow : Color.red;
            yield return new WaitForSeconds(0.5f);
        }

        batteryOnboardingBlinking = false;
        yield return null;
    }

    private void AddTileToMiniMap(MazeCell tile) {
        GameObject existingMiniMapTile = GameObject.Find($"MiniMap_{tile.gameObject.name}");

        float distance = Vector3.Distance(tile.transform.position, _player.gameObject.transform.position);
        if (distance > minDistanceMiniMap) {
            // TODO: Maybe just disabled then later replace + reenable ?
            if (existingMiniMapTile) {
                existingMiniMapTile.GetComponent<Image>().enabled = false;
            }
        }
        else {
            if (!_mazeCellManager.HasBeenRevealed(tile, _gameManager.revealedCellsInRun)) {
                _mazeCellManager.AddToRevealedTiles(tile, _gameManager.revealedCellsInRun);
            }

            // Instantiate new tile and anchor it in the middle of the panel

            if (existingMiniMapTile) {
                GameObject tileObject = GameObject.Find(existingMiniMapTile.name.Substring(8));
                existingMiniMapTile.GetComponent<Image>().enabled = true;
                if (tileObject == _mazeCellManager.GetTileUnderPlayer()) {
                    existingMiniMapTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                    existingMiniMapTile.GetComponent<Image>().sprite = playerSprite;
                }
                else {
                    if (tileObject == _gameManager.previousCell) {
                        existingMiniMapTile.GetComponent<Image>().sprite = floorSprite;
                    }

                    existingMiniMapTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(
                        _mazeCellManager.GetRelativePosition(_mazeCellManager.GetTileUnderPlayer().gameObject, tile)[0] * 10,
                        _mazeCellManager.GetRelativePosition(_mazeCellManager.GetTileUnderPlayer().gameObject, tile)[1] * 10,
                        0);
                }
            }
            else {
                Sprite tileSprite = GetTileSprite(tile.tag);
                GameObject newTile = new GameObject($"MiniMap_{tile.gameObject.name}");
                Image newImage = newTile.AddComponent<Image>();
                newTile.GetComponent<RectTransform>().SetParent(_miniMapPanel.transform);
                newTile.GetComponent<RectTransform>().SetAsFirstSibling();
                newTile.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                newTile.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                newTile.GetComponent<RectTransform>().rotation = Quaternion.Euler(0f, 0f, 0f);

                // Set the position of the new tile
                if (tile == _mazeCellManager.GetTileUnderPlayer().gameObject) {
                    newTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
                    tileSprite = playerSprite;
                }
                else {
                    newTile.GetComponent<RectTransform>().anchoredPosition = new Vector3(
                        _mazeCellManager.GetRelativePosition(_mazeCellManager.GetTileUnderPlayer().gameObject, tile)[0] * 10,
                        _mazeCellManager.GetRelativePosition(_mazeCellManager.GetTileUnderPlayer().gameObject, tile)[1] * 10,
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
        Vector3 position = tile.CompareTag("Player")
            ? _gameManager.currentCell.transform.position
            : tile.transform.position;
        GameObject existingTile = GameObject.Find($"Map_{tile.gameObject.name}");
        if (existingTile) {
            // TODO: Only destroy if necessary
            return;
            //TODO: Maybe put this back later
            Destroy(existingTile);
        }
        GameObject newTile = new GameObject($"Map_{tile.gameObject.name}");
        SpriteRenderer floorRenderer = newTile.AddComponent<SpriteRenderer>();
        newTile.transform.SetParent(_mapCanvas.transform);
        floorRenderer.sprite = floorSprite;
        newTile.transform.localPosition = new Vector3(
        position.x * 10 + 5,
        - (position.z * 10 + 35),
        0
        );
        newTile.transform.localRotation = Quaternion.identity;
        newTile.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        newTile.layer = 5;

        Debug.Log($"Added new tile to map: {newTile.name} \n {newTile.transform.rotation}");
        newTile.SetActive(true);
    }

    public void DrawMap(List<string> tiles) {
        foreach (string tileName in tiles) {
            GameObject tile = GameObject.Find(tileName);
            if (tile.GetComponent<MazeCell>() == _gameManager.currentCell) {
                AddTileToMap(_gameManager.player);
            }
            else {
                AddTileToMap(tile);
            }
        }
    }
    
    public void DrawWholeMap() {
        foreach (MazeCell cell in _mazeCellManager.mazeCells) {
            AddTileToMap(cell.gameObject);
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
                Debug.LogWarning("TAG_NOT_FOUND_FOR_TILE: " + tileTag);
                return floorSprite;
        }
    }

    private IEnumerator BlinkBatteryLevel(float timeToWait) {
        _batteryLevelBlinking = true;
        _batteryLevelText.SetActive(true);
        yield return new WaitForSeconds(timeToWait);
        _batteryLevelText.SetActive(false);
        yield return new WaitForSeconds(timeToWait);
        _batteryLevelBlinking = false;
    }


    public void ShowPauseUi() {
        _pauseCanvas.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _player.GetComponent<Player>().lockPlayer = true;
    }

    private void ResumeGame() {
        _gameManager.gameIsPaused = false;
        _pauseCanvas.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _player.GetComponent<Player>().lockPlayer = false;
    }

    public void ShowExitUi() {
        _exitReachedText.SetActive(true);
        _exitReachedButtons.SetActive(true);
        // set posX of ack to menu to 0
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _player.GetComponent<Player>().lockPlayer = true;
        if (SceneManager.GetActiveScene().name != "Level7") return;
        _exitReachedText.GetComponent<TextMeshProUGUI>().text =
            "Congrats beta tester, you've been through all the levels !!";
        _exitReachedText.GetComponent<TextMeshProUGUI>().fontSize = 17;
        _backToMenuButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    }

    public void UpdateDiscoveryText(int discoveredTiles, int mapSize) {
        _discoveryText.text = discoveredTiles > 0
            ? $"Discovered {Math.Round((double) discoveredTiles / mapSize * 100)}%"
            : "";
    }

    public void AddInfoMessage(string message) {
        // Handle position of previous texts if any 
        if (_infoPanel.transform.childCount > 0) {
            foreach (Transform previousText in _infoPanel.transform) {
                RectTransform rectTransform = previousText.GetComponent<RectTransform>();
                if (rectTransform.anchoredPosition.y == 80) {
                    Destroy(previousText.gameObject);
                }
                rectTransform.anchoredPosition = new Vector3(0, rectTransform.anchoredPosition.y + 20, 0);
            }
        }

        // Instantiate new text
        GameObject infoText = Instantiate(infoTextPrefab, _infoPanel.transform.position,
            _infoPanel.transform.rotation, _infoPanel.transform);
        _totalInfoText++;
        infoText.name = $"InfoText{_totalInfoText}";
        TextMeshProUGUI text = infoText.GetComponent<TextMeshProUGUI>();
        text.text = message;

        // Adjust Position
        RectTransform infoTextTransform = infoText.GetComponent<RectTransform>();
        infoTextTransform.anchorMin = new Vector2(0.5f, 0);
        infoTextTransform.anchorMax = new Vector2(0.5f, 0);
        infoTextTransform.pivot = new Vector2(0.5f, 0);
        infoTextTransform.anchoredPosition = new Vector3(0, 0, 0);

        // Destroy it later
        Destroy(infoText, 5f);
    }
}