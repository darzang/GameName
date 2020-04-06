using System;
using System.Collections;
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
        
        if (_mazeCellManager.GetMazeAsList().Find(cell => cell.permanentlyRevealed)) {
            DrawMap();
        }

        _tryCountText.text = $"Try number {_gameManager.tryCount} / {_gameManager.tryMax}";
        UpdateDiscoveryText(_mazeCellManager.GetDiscoveredCellsCount(), _mazeCellManager.GetMapSize());
        if (_mazeCellManager.GetDiscoveredCellsCount() > 0) AddInfoMessage("Previous data loaded");
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
        while (angle > 360) {
            angle -= 360;
        }
        while (angle < 0) {
            angle += 360;
        }
        _miniMapPanel.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void UpdateMiniMap() {
        foreach (MazeCell cell in _mazeCellManager.mazeCells) {
            if (cell.revealedForCurrentRun) AddTileToMiniMap(cell);
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
        MazeCell realCell = tile.GetComponent<MazeCell>();
        GameObject existingMiniMapCell = GameObject.Find($"MiniMap_{tile.gameObject.name}");

        float distance = Vector3.Distance(tile.transform.position, _player.gameObject.transform.position);
        if (distance > minDistanceMiniMap) {
            // TODO: Maybe just disabled then later replace + reenable ?
            if (existingMiniMapCell) {
                existingMiniMapCell.GetComponent<SpriteRenderer>().enabled = false;
                // TODO: disable wall sprites
            }
        }
        else {
            // Instantiate new cell
            if (existingMiniMapCell) {
                existingMiniMapCell.GetComponent<SpriteRenderer>().enabled = true;
                existingMiniMapCell.transform.localPosition = new Vector3(
                    _mazeCellManager.GetRelativePosition(_mazeCellManager.GetTileUnder(_gameManager.player).gameObject, tile)
                        [0] * 10,
                    _mazeCellManager.GetRelativePosition(_mazeCellManager.GetTileUnder(_gameManager.player).gameObject, tile)
                        [1] * 10,
                    0);
                if (realCell == _gameManager.currentCell) {
                    AddPlayerSprite(existingMiniMapCell.transform, "MiniMap");
                }
            }
            else {
                GameObject newMiniMapCell = new GameObject($"MiniMap_{tile.gameObject.name}");
                SpriteRenderer floorRenderer = newMiniMapCell.AddComponent<SpriteRenderer>();
                floorRenderer.sprite = floorSprite;
                if (realCell && realCell.isExit) floorRenderer.sprite = exitSprite;

                newMiniMapCell.transform.SetParent(_miniMapPanel.transform);
                newMiniMapCell.transform.SetAsFirstSibling();
                newMiniMapCell.transform.localRotation = Quaternion.identity;
                newMiniMapCell.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                newMiniMapCell.layer = 5;

                
                // Set the position of the new tile
                newMiniMapCell.transform.localPosition = new Vector3(
                    _mazeCellManager.GetRelativePosition(_mazeCellManager.GetTileUnder(_gameManager.player).gameObject, tile)
                        [0] * 10,
                    _mazeCellManager.GetRelativePosition(_mazeCellManager.GetTileUnder(_gameManager.player).gameObject, tile)
                        [1] * 10,
                    0);

                newMiniMapCell.SetActive(true);
                if (realCell) {
                    AddWallSprite(realCell, newMiniMapCell.transform, "MiniMap");
                    if (realCell == _gameManager.currentCell) {
                        AddPlayerSprite(newMiniMapCell.transform, "MiniMap");
                    }
                }
            }
        }
    }

    private void AddTileToMap(GameObject tile) {
        MazeCell realCell = tile.GetComponent<MazeCell>();
        Vector3 position = tile.transform.position;
        GameObject existingTile = GameObject.Find($"Map_{tile.gameObject.name}");
        if (!existingTile) {
            GameObject newCellObject = new GameObject($"Map_{tile.gameObject.name}");
            SpriteRenderer floorRenderer = newCellObject.AddComponent<SpriteRenderer>();
            newCellObject.transform.SetParent(_mapCanvas.transform);
            floorRenderer.sprite = floorSprite;
            if (realCell && realCell.isExit) floorRenderer.sprite = exitSprite;
            newCellObject.transform.localPosition = new Vector3(
                position.z * 10 + 5,
                -(position.x * 10 + 35),
                0
            );
            newCellObject.transform.localRotation = Quaternion.identity;
            newCellObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            newCellObject.layer = 5;
            newCellObject.SetActive(true);

            // Add walls sprites for each wall of the cell
            AddWallSprite(realCell, newCellObject.transform, "Map");
            if (realCell == _gameManager.currentCell) {
                AddPlayerSprite(newCellObject.transform, "Map");
            }

        }
        else {
            if (realCell == _gameManager.currentCell) {
                AddPlayerSprite(existingTile.transform, "Map");
            }
        }

    }

    private void AddPlayerSprite(Transform canvasCell, String prefix) {
        MazeCell previousCell = _gameManager.previousCell;
        if (previousCell) {
            GameObject previousCellMap = GameObject.Find($"{prefix}_Player");
            if (previousCellMap) {
                Destroy(previousCellMap);
            }
        }

        GameObject playerObject = new GameObject($"{prefix}_Player");
        playerObject.transform.SetParent(canvasCell);
        SpriteRenderer playerSpriteRenderer = playerObject.AddComponent<SpriteRenderer>();
        playerSpriteRenderer.sprite = playerSprite;
        playerObject.transform.localPosition = new Vector3(0f, 0f, 0f);
        playerObject.transform.localScale = new Vector3(1f, 1f, 1f);
        Vector3 rotation = new Vector3(0f, 0f, 0f);
        Quaternion rotationQuaternion = Quaternion.Euler(rotation);
        playerObject.transform.localRotation = rotationQuaternion;
        playerSpriteRenderer.sortingOrder = 1;
    }

    private void AddWallSprite(MazeCell cell, Transform canvasCell, String prefix) {
        MazeCell northCell =
            _mazeCellManager.GetCellIfExists((int) cell.transform.position.x - 1, (int) cell.transform.position.z);
        MazeCell southCell =
            _mazeCellManager.GetCellIfExists((int) cell.transform.position.x + 1, (int) cell.transform.position.z);
        MazeCell eastCell =
            _mazeCellManager.GetCellIfExists((int) cell.transform.position.x, (int) cell.transform.position.z + 1);
        MazeCell westCell =
            _mazeCellManager.GetCellIfExists((int) cell.transform.position.x, (int) cell.transform.position.z - 1);
        if (cell.northWall || (northCell && northCell.southWall)) {
            GameObject northWallObject = new GameObject($"{prefix}_{cell.gameObject.name}_North_Wall");
            northWallObject.transform.SetParent(canvasCell);
            SpriteRenderer northWallSprite = northWallObject.AddComponent<SpriteRenderer>();
            northWallSprite.sprite = wallSprite;
            northWallObject.transform.localPosition = new Vector3(0f, 5f, 0f);
            northWallObject.transform.localScale = new Vector3(1f, 1f, 1f);
            Vector3 rotation = new Vector3(0f, 0f, 90f);
            Quaternion rotationQuaternion = Quaternion.Euler(rotation);
            northWallObject.transform.localRotation = rotationQuaternion;
            northWallSprite.sortingOrder = 1;
        }

        if (cell.southWall || (southCell && southCell.northWall)) {
            GameObject southWallObject = new GameObject($"{prefix}_{cell.gameObject.name}_South_Wall");
            southWallObject.transform.SetParent(canvasCell);
            SpriteRenderer southWallSprite = southWallObject.AddComponent<SpriteRenderer>();
            southWallSprite.sprite = wallSprite;
            southWallSprite.sortingOrder = 1;
            southWallObject.transform.localPosition = new Vector3(0f, -5f, 0f);
            southWallObject.transform.localScale = new Vector3(1f, 1f, 1f);
            Vector3 rotation = new Vector3(0f, 0f, 90f);
            Quaternion rotationQuaternion = Quaternion.Euler(rotation);
            southWallObject.transform.localRotation = rotationQuaternion;
        }

        if (cell.eastWall || (eastCell && eastCell.westWall)) {
            GameObject eastWallObject = new GameObject($"{prefix}_{cell.gameObject.name}_East_Wall");
            eastWallObject.transform.SetParent(canvasCell);
            SpriteRenderer eastWallSprite = eastWallObject.AddComponent<SpriteRenderer>();
            eastWallSprite.sprite = wallSprite;
            eastWallObject.transform.localPosition = new Vector3(5f, 0f, 0f);
            eastWallObject.transform.localRotation = Quaternion.identity;
            eastWallObject.transform.localScale = new Vector3(1f, 1f, 1f);
            eastWallSprite.sortingOrder = 1;
        }

        if (cell.westWall || (westCell && westCell.eastWall)) {
            GameObject westWallObject = new GameObject($"{prefix}_{cell.gameObject.name}_West_Wall");
            westWallObject.transform.SetParent(canvasCell);
            SpriteRenderer westWallSprite = westWallObject.AddComponent<SpriteRenderer>();
            westWallSprite.sprite = wallSprite;
            westWallObject.transform.localPosition = new Vector3(-5f, 0f, 0f);
            westWallObject.transform.localRotation = Quaternion.identity;
            westWallObject.transform.localScale = new Vector3(1f, 1f, 1f);
            westWallSprite.sortingOrder = 1;
        }
    }

    public void DrawMap() {
        foreach (MazeCell mazeCell in _mazeCellManager.mazeCells) {
            if (mazeCell.permanentlyRevealed) {
                AddTileToMap(mazeCell.gameObject);
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
        //TODO: Just handle 5 text max, don't destroy but animate/hide them instead
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