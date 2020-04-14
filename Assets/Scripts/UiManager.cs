using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviour {
    
    // Managers
    private MazeCellManager _mazeCellManager;
    private GameManager _gameManager;
    
    // Texts
    [HideInInspector] public TextMeshProUGUI onboardingText;
    private GameObject _batteryLevelText;
    private TextMeshProUGUI _batteryDeadText;
    private GameObject _exitReachedText;
    private TextMeshProUGUI _discoveryText;
    private TextMeshProUGUI _tryCountText;
    private TextMeshProUGUI _batteryText;
    private int _totalInfoText;
    
    // Buttons
    private GameObject _exitReachedButtons;
    private Button _retryButton;
    private Button _nextLevelButton;
    private Button _giveUpButton;
    private Button _backToMenuButton;
    private Button _pauseRetryButton;
    private Button _pauseResumeButton;
    private Button _pauseBackButton;
    
    // Panels/Canvas
    private GameObject _mainCanvas;
    private GameObject _pauseCanvas;
    private GameObject _mapCanvas;
    private GameObject _miniMapCanvas;
    private GameObject _batteryBarCanvas;
    private GameObject _infoCanvas;
    private GameObject _exitReachedCanvas;
    private GameObject _batteryDeadCanvas;
    private GameObject _onboardingCanvas;    
    private GameObject _miniMapPanel;
    private GameObject _infoPanel;
    
    // Sprites/Prefabs etc
    private Image _batteryBarImage;
    public Sprite wallSprite;
    public Sprite exitSprite;
    public Sprite floorSprite;
    public Sprite playerSprite;
    public GameObject infoTextPrefab; 
    
    // Misc
    private GameObject _batteryBar;
    public GameObject player;
    private Player _player;
    private Quaternion _initialRotation;
    public float minDistanceMiniMap = 5;
    private bool _batteryLevelBlinking;
    [HideInInspector] public bool batteryOnboardingBlinking;
    
    // Map 
    private GameObject playerMapIcon;
    private List<GameObject> mapCells = new List<GameObject>();

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
        // Managers
        _mazeCellManager = GameObject.Find("MazeCellManager").GetComponent<MazeCellManager>();
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

        InstantiateMap();
        _tryCountText.text = $"Try number {_gameManager.levelData.tryCount} / {_gameManager.tryMax}";
        UpdateDiscoveryText(_mazeCellManager.GetDiscoveredCellsCount(), _mazeCellManager.mazeCells.Count);
        if (_mazeCellManager.GetDiscoveredCellsCount() > 0) AddInfoMessage("Previous data loaded");
        _nextLevelButton.onClick.AddListener(_gameManager.NextLevel);
        _backToMenuButton.onClick.AddListener(_gameManager.BackToMenu);
    }

    private void Update() {
        RotateMiniMap();
        UpdateMap();
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
            if (_gameManager.levelData.tryCount >= _gameManager.tryMax) {
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

    private void AddTileToMiniMap(MazeCell mazeCell) {
        GameObject existingMiniMapCell = GameObject.Find($"MiniMap_{mazeCell.name}");

        float distance =
            Vector3.Distance(new Vector3(mazeCell.x, 0f, mazeCell.z), _player.gameObject.transform.position);
        if (distance > minDistanceMiniMap) {
            // TODO: Maybe just disabled then later replace + reenable ?
            if (existingMiniMapCell) {
                existingMiniMapCell.GetComponent<SpriteRenderer>().enabled = false;
                // TODO: disable wall sprites
            }
        }
        else {
            if (existingMiniMapCell) {
                existingMiniMapCell.GetComponent<SpriteRenderer>().enabled = true;
                existingMiniMapCell.transform.localPosition = new Vector3(
                    _mazeCellManager.GetRelativePosition(_gameManager.player, mazeCell)[0] * 10,
                    _mazeCellManager.GetRelativePosition(_gameManager.player, mazeCell)[1] * 10,
                    0);
            }
            else {
                // Instantiate new cell
                GameObject newMiniMapCell = new GameObject($"MiniMap_{mazeCell.name}");
                SpriteRenderer floorRenderer = newMiniMapCell.AddComponent<SpriteRenderer>();
                floorRenderer.sprite = floorSprite;
                if (mazeCell.isExit) floorRenderer.sprite = exitSprite;

                newMiniMapCell.transform.SetParent(_miniMapPanel.transform);
                newMiniMapCell.transform.SetAsFirstSibling();
                newMiniMapCell.transform.localRotation = Quaternion.identity;
                newMiniMapCell.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                newMiniMapCell.layer = 5;


                // Set the position of the new tile
                newMiniMapCell.transform.localPosition = new Vector3(
                    _mazeCellManager.GetRelativePosition(_gameManager.player, mazeCell)[0] * 10,
                    _mazeCellManager.GetRelativePosition(_gameManager.player, mazeCell)[1] * 10,
                    0);

                newMiniMapCell.SetActive(true);
                AddWallSprites(mazeCell, newMiniMapCell.transform, "MiniMap");
            }
        }
    }

    private void AddTileToMap(MazeCell mazeCell) {
        GameObject newCellObject = new GameObject($"Map_{mazeCell.name}");
        newCellObject.transform.SetParent(_mapCanvas.transform);
        mapCells.Add(newCellObject);
        newCellObject.transform.localPosition = new Vector3(
            mazeCell.z * 10 + 5,
            -(mazeCell.x * 10 + 35),
            0
        );
        newCellObject.transform.localRotation = Quaternion.identity;
        newCellObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        newCellObject.layer = 5;

        newCellObject.SetActive(true);


        AddFloorSprite(mazeCell, newCellObject.transform);
        // Add walls sprites for each wall of the cell
        AddWallSprites(mazeCell, newCellObject.transform, "Map");
        foreach (Transform child in newCellObject.transform) {
            child.gameObject.GetComponent<SpriteRenderer>().enabled = mazeCell.permanentlyRevealed;
        }

        if (mazeCell == _gameManager.currentCell) AddPlayerSprite(newCellObject.transform);
    }

    private void InstantiateMap() {
        foreach (MazeCell mazeCell in _mazeCellManager.mazeCells) {
            AddTileToMap(mazeCell);
        }
    }

    public void UpdateMap() {
        // Make sure all discovered cells' sprites are enabled
        foreach (MazeCell mazeCell in _mazeCellManager.mazeCells) {
            if (mazeCell.permanentlyRevealed) {
                GameObject mapCell = mapCells.Find(cell => cell.name == $"Map_{mazeCell.name}");
                foreach (Transform sprite in mapCell.transform) {
                    sprite.GetComponent<SpriteRenderer>().enabled = true;
                }
            }
        }
        // Make sure the player is on the right tile
        MazeCell currentCell = _gameManager.currentCell;
        GameObject mapCurrentCell = mapCells.Find(cell => cell.name == $"Map_{currentCell.name}");
        MovePlayerSpriteToCell(mapCurrentCell.transform);
        
        UpdatePlayerSpritePositionRotation();

    }

    public void DrawWholeMap() {
        foreach (MazeCell cell in _mazeCellManager.mazeCells) {
            cell.permanentlyRevealed = true;
            UpdateMap();
        }
    }
    private void AddPlayerSprite(Transform canvasCell) {
        playerMapIcon = new GameObject("Map_Player");
        SpriteRenderer playerSpriteRenderer = playerMapIcon.AddComponent<SpriteRenderer>();
        playerSpriteRenderer.sprite = playerSprite;
        playerMapIcon.transform.SetParent(canvasCell);
        playerMapIcon.transform.localPosition = new Vector3(0f, 0f, 0f);
        playerMapIcon.transform.localScale = new Vector3(1f, 1f, 1f);
        playerMapIcon.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        playerSpriteRenderer.sortingOrder = 2;
    }

    private void AddFloorSprite(MazeCell mazeCell, Transform mapCell) {
        // Add floor sprite
        GameObject floorSpriteObject = new GameObject($"Map_{mazeCell.name}_Floor");
        SpriteRenderer floorRenderer = floorSpriteObject.AddComponent<SpriteRenderer>();
        floorRenderer.sprite = floorSprite;
        if (mazeCell.isExit) floorRenderer.sprite = exitSprite;
        floorSpriteObject.transform.SetParent(mapCell);
        floorSpriteObject.transform.localPosition = new Vector3(0f, 0f, 0f);
        floorSpriteObject.transform.localRotation = Quaternion.identity;
        floorSpriteObject.transform.localScale = new Vector3(1f, 1f, 1f);
        floorRenderer.sortingOrder = 1;
    }

    private void MovePlayerSpriteToCell(Transform canvasCell) {
        playerMapIcon.transform.SetParent(canvasCell);
        playerMapIcon.transform.localPosition = new Vector3(0f, 0f, 0f);
    }

    private void UpdatePlayerSpritePositionRotation() {
        // Position
            // TODO: Get playing position relative to current tile
        // Rotation
        playerMapIcon.transform.localRotation = Quaternion.Euler(new Vector3(0f,0f,-(player.transform.eulerAngles.y + 90)));
        // playerMapIcon.GetComponent<SpriteRenderer>().sortingOrder = 2;
    }

    private void AddWallSprites(MazeCell mazeCell, Transform canvasCell, String prefix) {
        MazeCell northCell = _mazeCellManager.GetCellIfExists(mazeCell.x - 1, mazeCell.z);
        MazeCell southCell = _mazeCellManager.GetCellIfExists(mazeCell.x + 1, mazeCell.z);
        MazeCell eastCell = _mazeCellManager.GetCellIfExists(mazeCell.x, mazeCell.z + 1);
        MazeCell westCell = _mazeCellManager.GetCellIfExists(mazeCell.x, mazeCell.z - 1);
        if (mazeCell.hasNorthWall || (northCell != null && northCell.hasSouthWall)) {
            GameObject northWallObject = new GameObject($"{prefix}_{mazeCell.name}_North_Wall");
            northWallObject.transform.SetParent(canvasCell);
            SpriteRenderer northWallSprite = northWallObject.AddComponent<SpriteRenderer>();
            northWallSprite.sprite = wallSprite;
            if (prefix == "MiniMap") {
                northWallObject.transform.localPosition = new Vector3(5f, 0f, 0f);
                northWallObject.transform.localRotation = Quaternion.identity;
            }
            else {
                northWallObject.transform.localPosition = new Vector3(0f, 5f, 0f);
                northWallObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            }
            northWallObject.transform.localScale = new Vector3(1f, 1f, 1f);
            northWallSprite.sortingOrder = 1;
        }

        if (mazeCell.hasSouthWall || (southCell != null && southCell.hasNorthWall)) {
            GameObject southWallObject = new GameObject($"{prefix}_{mazeCell.name}_South_Wall");
            southWallObject.transform.SetParent(canvasCell);
            SpriteRenderer southWallSprite = southWallObject.AddComponent<SpriteRenderer>();
            southWallSprite.sprite = wallSprite;
            if (prefix == "MiniMap") {
                southWallObject.transform.localPosition = new Vector3(-5f, 0f, 0f);
                southWallObject.transform.localRotation = Quaternion.identity;
            }
            else {
                southWallObject.transform.localPosition = new Vector3(0f, -5f, 0f);
                southWallObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            }
            southWallObject.transform.localScale = new Vector3(1f, 1f, 1f);
            southWallSprite.sortingOrder = 1;
        }

        if (mazeCell.hasEastWall || (eastCell != null && eastCell.hasWestWall)) {
            GameObject eastWallObject = new GameObject($"{prefix}_{mazeCell.name}_East_Wall");
            eastWallObject.transform.SetParent(canvasCell);
            SpriteRenderer eastWallSprite = eastWallObject.AddComponent<SpriteRenderer>();
            eastWallSprite.sprite = wallSprite;
           if (prefix == "MiniMap") {
                eastWallObject.transform.localPosition = new Vector3(0f, -5f, 0f);
                eastWallObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            }
            else {
                eastWallObject.transform.localPosition = new Vector3(5f, 0f, 0f);
                eastWallObject.transform.localRotation = Quaternion.identity;
            }
            eastWallObject.transform.localScale = new Vector3(1f, 1f, 1f);
            eastWallSprite.sortingOrder = 1;
        }

        if (mazeCell.hasWestWall || (westCell != null && westCell.hasEastWall)) {
            GameObject westWallObject = new GameObject($"{prefix}_{mazeCell.name}_West_Wall");
            westWallObject.transform.SetParent(canvasCell);
            SpriteRenderer westWallSprite = westWallObject.AddComponent<SpriteRenderer>();
            westWallSprite.sprite = wallSprite;
            
            if (prefix == "MiniMap") {
                westWallObject.transform.localPosition = new Vector3(0f, 5f, 0f);
                westWallObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            }
            else {
                westWallObject.transform.localPosition = new Vector3(-5f, 0f, 0f);
                westWallObject.transform.localRotation = Quaternion.identity;
            }

            westWallObject.transform.localScale = new Vector3(1f, 1f, 1f);
            westWallSprite.sortingOrder = 1;
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