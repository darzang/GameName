using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    // Managers
    private MazeCellManager _mazeCellManager;
    private GameManager _gameManager;

    // Texts
    [HideInInspector] public TextMeshProUGUI onboardingText;
    private GameObject _batteryLevelText;
    private TextMeshProUGUI _batteryDeadText;
    private TextMeshProUGUI _exitReachedText;
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
    private Player _player;
    private Quaternion _initialRotation;
    private bool _batteryLevelBlinking;
    [HideInInspector] public bool batteryOnboardingBlinking;

    // Map 
    private GameObject _playerMapIcon;
    private List<GameObject> _mapCells = new List<GameObject>();
    List<SpriteRenderer> cellRenderers;
    private List<SpriteRenderer> _mapCellsRenderer = new List<SpriteRenderer>();
    MazeCell currentCell;
    GameObject mapCurrentCell;
    float distancePlayerCell;
    // MiniMap
    private RectTransform miniMapRectTransform;
    public float minDistanceMiniMap = 5;
    private List<GameObject> _miniMapCells = new List<GameObject>();
    private List<SpriteRenderer> _miniMapCellsRenderer = new List<SpriteRenderer>();
    float miniMapRotationAngle;
    Vector3 mazeCellPosition;
    Vector3 playerCellVector;
    GameObject miniMapCell;


    private void Awake()
    {
        Cursor.visible = false;

        // Variables instantiation
        _mainCanvas = GameObject.Find("MainCanvas").gameObject;
        _pauseCanvas = _mainCanvas.transform.Find("PauseCanvas").gameObject;
        _mapCanvas = _mainCanvas.transform.Find("MapCanvas").gameObject;
        _discoveryText = _mapCanvas.transform.Find("DiscoveryText").GetComponent<TextMeshProUGUI>();
        _tryCountText = _mapCanvas.transform.Find("TryCountText").GetComponent<TextMeshProUGUI>();
        _miniMapCanvas = _mainCanvas.transform.Find("MiniMapCanvas").gameObject;
        _miniMapPanel = _miniMapCanvas.transform.Find("MiniMapPanel").gameObject;
        miniMapRectTransform = _miniMapPanel.GetComponent<RectTransform>();
        _batteryBarCanvas = _mainCanvas.transform.Find("BatteryBarCanvas").gameObject;
        _infoCanvas = _mainCanvas.transform.Find("InfoCanvas").gameObject;
        _infoPanel = _infoCanvas.transform.Find("InfoPanel").gameObject;
        _exitReachedCanvas = _mainCanvas.transform.Find("ExitReachedCanvas").gameObject;
        _batteryDeadCanvas = _mainCanvas.transform.Find("BatteryDeadCanvas").gameObject;
        _exitReachedText = _exitReachedCanvas.transform.Find("ExitReachedText").gameObject
            .GetComponent<TextMeshProUGUI>();
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

    public void Instantiation()
    {
        // Managers
        _mazeCellManager = GameObject.Find("MazeCellManager").GetComponent<MazeCellManager>();
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        // Button listeners
        _player = _gameManager.player;
        _mainCanvas.GetComponent<Canvas>().worldCamera = _player.transform.Find("PlayerCamera").GetComponent<Camera>();
        _mainCanvas.GetComponent<Canvas>().planeDistance = 0.1f;
        _retryButton.onClick.AddListener(_gameManager.Retry);
        _giveUpButton.onClick.AddListener(_gameManager.GiveUp);
        _pauseResumeButton.onClick.AddListener(ResumeGame);
        _pauseRetryButton.onClick.AddListener(_gameManager.Retry);
        _pauseBackButton.onClick.AddListener(_gameManager.GiveUp);
        mazeCellPosition = new Vector3();
        playerCellVector = new Vector3();
        InstantiateMap();
        InstantiateMiniMap();
        _tryCountText.text = $"Try number {_gameManager.levelData.tryCount} / {_gameManager.tryMax}";
        UpdateDiscoveryText(_mazeCellManager.GetDiscoveredCellsCount(), _mazeCellManager.mazeCells.Count);
        if (_mazeCellManager.GetDiscoveredCellsCount() > 0) AddInfoMessage("Previous data loaded");
        _nextLevelButton.onClick.AddListener(_gameManager.NextLevel);
        _backToMenuButton.onClick.AddListener(_gameManager.BackToMenu);
    }

    private void Update()
    {
        RotateMiniMap();
        UpdateMap();
        UpdateMiniMap();
        // _player.fuelCount = _player.fuelCount;
        if (_player.fuelCount > 0)
        {
            UpdateBatteryLevel();
            if (_player.fuelCount > _gameManager.playerData.batteryMax * 0.5)
            {
                _batteryText.text = "";
            }
            else if (_player.fuelCount > _gameManager.playerData.batteryMax * 0.2)
            {
                _batteryText.text = "BATTERY LEVEL LOW";
                if (!_batteryLevelBlinking) StartCoroutine(BlinkBatteryLevel(0.5f));
            }
            else
            {
                _batteryText.text = "BATTERY LEVEL CRITICAL";
                if (!_batteryLevelBlinking) StartCoroutine(BlinkBatteryLevel(0.25f));
            }
        }
        else if (!_batteryDeadCanvas.activeSelf)
        {
            Cursor.visible = true;
            _batteryLevelText.SetActive(false);
            StopCoroutine(nameof(BlinkBatteryLevel));
            _batteryDeadCanvas.SetActive(true);
            if (_gameManager.levelData.tryCount >= _gameManager.tryMax)
            {
                _batteryDeadText.fontSize = 18;
                _batteryDeadText.text =
                    "Well, I told you, don't die too much...\n And one more map exploration lost forever...";
            }

            Cursor.lockState = CursorLockMode.None;
        }
    }


    public void HideCanvas()
    {
        _mainCanvas.SetActive(false);
    }

    private void RotateMiniMap()
    {
        try
        {
            miniMapRotationAngle = _player.transform.eulerAngles.y + 180;
            while (miniMapRotationAngle > 360)
            {
                miniMapRotationAngle -= 360;
            }

            while (miniMapRotationAngle < 0)
            {
                miniMapRotationAngle += 360;
            }

            miniMapRectTransform.localRotation = Quaternion.Euler(0, 0, miniMapRotationAngle);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in RotateMiniMap: {e}");
        }
    }

    private void UpdateBatteryLevel()
    {
        // Update scale
        Vector3 localScale = _batteryBar.transform.localScale;
        localScale = new Vector3(
            _player.fuelCount / _gameManager.playerData.batteryMax,
            localScale.y,
            localScale.z
        );
        _batteryBar.transform.localScale = localScale;
        // Update color
        if (!batteryOnboardingBlinking)
        {
            _batteryBarImage.color = _player.fuelCount > _gameManager.playerData.batteryMax / 2
                ? new Color32((byte)(_gameManager.playerData.batteryMax - _player.fuelCount), 255, 0, 255)
                : new Color32(255, (byte)_player.fuelCount, 0, 255);
        }
    }

    public IEnumerator OnboardingBlinkBattery()
    {
        batteryOnboardingBlinking = true;
        while (_gameManager.onboardingStage == 2)
        {
            _batteryBarImage.color = _batteryBarImage.color == Color.red ? Color.yellow : Color.red;
            yield return new WaitForSeconds(0.5f);
        }

        batteryOnboardingBlinking = false;
        yield return null;
    }

    private void AddTileToMiniMap(MazeCell mazeCell)
    {
        GameObject newMiniMapCell = new GameObject($"MiniMap_{mazeCell.name}");

        float distance = Vector3.Distance(
            new Vector3(mazeCell.x, 0f, mazeCell.z),
            _player.gameObject.transform.position
        );
        // Instantiate new cell

        newMiniMapCell.transform.SetParent(_miniMapPanel.transform);
        // newMiniMapCell.transform.SetAsFirstSibling();
        newMiniMapCell.transform.localRotation = Quaternion.identity;
        newMiniMapCell.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        newMiniMapCell.layer = 5;

        AddFloorSprite(mazeCell, newMiniMapCell.transform, "MiniMap");
        // Set the position of the new tile
        newMiniMapCell.transform.localPosition = new Vector3(
            MazeCellManager.GetRelativePosition(_gameManager.player.gameObject, mazeCell)[0] * 10,
            MazeCellManager.GetRelativePosition(_gameManager.player.gameObject, mazeCell)[1] * 10,
            0);

        newMiniMapCell.SetActive(true);
        AddWallSprites(mazeCell, newMiniMapCell.transform, "MiniMap");

        // TODO: Maybe just disabled then later replace + reenable ?
        foreach (Transform child in newMiniMapCell.transform)
        {
            SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
            renderer.enabled = !(distance > minDistanceMiniMap) && mazeCell.revealedForCurrentRun;
            _miniMapCellsRenderer.Add(renderer);
        }

        _miniMapCells.Add(newMiniMapCell);
    }

    private void UpdateMiniMap()
    {
        // Checks which mazecell needs to be renderer or not
        foreach (MazeCell mazeCell in _mazeCellManager.mazeCells)
        {
            // We only need to render the ones that are revealed
            if (!mazeCell.revealedForCurrentRun) continue;

            mazeCellPosition.Set(mazeCell.x, 0f, mazeCell.z);
            distancePlayerCell = Vector3.Distance(mazeCellPosition, _player.gameObject.transform.position);
            miniMapCell = _miniMapCells.Find(cell => cell.name == $"MiniMap_{mazeCell.name}");

            // Hide cells that are out of range
            if (distancePlayerCell > minDistanceMiniMap)
            {
                SetCellRenderersState(mazeCell.name, false);
                continue;
            }
            // Adjust position and visibility of reachable cells
            Debug.Log($"playerCellVector before: " + playerCellVector.ToString());
            playerCellVector = MazeCellManager.GetRelativePosition(_gameManager.player.gameObject,mazeCell);
            Debug.Log($"playerCellVector after: " + playerCellVector.ToString());
            Debug.Log($"miniMapCell before: " + miniMapCell.transform.localPosition.ToString());
            miniMapCell.transform.localPosition = playerCellVector;
            Debug.Log($"miniMapCell after: " + miniMapCell.transform.localPosition.ToString());
            SetCellRenderersState(mazeCell.name, true);
        }
    }

    private void SetCellRenderersState(string cellName, bool state)
    {
        foreach (SpriteRenderer renderer in _miniMapCellsRenderer.Where(cell => cell.transform.name.Contains(cellName)).ToList())
        {
            renderer.enabled = state;
        }
    }

    private void InstantiateMiniMap()
    {
        foreach (MazeCell mazeCell in _mazeCellManager.mazeCells)
        {
            AddTileToMiniMap(mazeCell);
        }
    }

    private void InstantiateMap()
    {
        foreach (MazeCell mazeCell in _mazeCellManager.mazeCells)
        {
            AddTileToMap(mazeCell);
        }
    }

    private void AddTileToMap(MazeCell mazeCell)
    {
        GameObject newCellObject = new GameObject($"Map_{mazeCell.name}");
        newCellObject.transform.SetParent(_mapCanvas.transform);
        _mapCells.Add(newCellObject);
        newCellObject.transform.localPosition = new Vector3(
            mazeCell.z * 10 + 5,
            -(mazeCell.x * 10 + 35),
            0
        );
        newCellObject.transform.localRotation = Quaternion.identity;
        newCellObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        newCellObject.layer = 5;

        newCellObject.SetActive(true);


        AddFloorSprite(mazeCell, newCellObject.transform, "Map");
        // Add walls sprites for each wall of the cell
        AddWallSprites(mazeCell, newCellObject.transform, "Map");
        foreach (Transform child in newCellObject.transform)
        {
            SpriteRenderer renderer = child.gameObject.GetComponent<SpriteRenderer>();
            renderer.enabled = mazeCell.permanentlyRevealed;
            _mapCellsRenderer.Add(renderer);
        }

        if (mazeCell == _gameManager.currentCell) AddPlayerSprite(newCellObject.transform);
    }

    private void UpdateMap()
    {
        // Make sure all discovered cells' sprites are enabled
        foreach (MazeCell mazeCell in _mazeCellManager.mazeCells)
        {
            if (mazeCell.permanentlyRevealed)
            {
                cellRenderers =
                    _mapCellsRenderer.Where(cell => cell.name.Contains(mazeCell.name)).ToList();
                foreach (SpriteRenderer renderer in cellRenderers)
                {
                    renderer.enabled = true;
                }
            }
        }

        // Make sure the player is on the right tile
        currentCell = _gameManager.currentCell;
        mapCurrentCell = _mapCells.Find(cell => cell.name == $"Map_{currentCell.name}");
        MovePlayerSpriteToCell(mapCurrentCell.transform);
        UpdatePlayerSpritePositionRotation();
    }

    public void DrawWholeMap()
    {
        foreach (MazeCell cell in _mazeCellManager.mazeCells)
        {
            cell.permanentlyRevealed = true;
            UpdateMap();
        }
    }

    private void AddPlayerSprite(Transform canvasCell)
    {
        _playerMapIcon = new GameObject("Map_Player");
        SpriteRenderer playerSpriteRenderer = _playerMapIcon.AddComponent<SpriteRenderer>();
        playerSpriteRenderer.sprite = playerSprite;
        _playerMapIcon.transform.SetParent(canvasCell);
        _playerMapIcon.transform.localPosition = new Vector3(0f, 0f, 0f);
        _playerMapIcon.transform.localScale = new Vector3(1f, 1f, 1f);
        _playerMapIcon.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        playerSpriteRenderer.sortingOrder = 2;
    }

    private void AddFloorSprite(MazeCell mazeCell, Transform mapCell, string prefix)
    {
        // Add floor sprite
        GameObject floorSpriteObject = new GameObject($"{prefix}_{mazeCell.name}_Floor");
        SpriteRenderer floorRenderer = floorSpriteObject.AddComponent<SpriteRenderer>();
        floorRenderer.sprite = floorSprite;
        if (mazeCell.isExit) floorRenderer.sprite = exitSprite;
        floorSpriteObject.transform.SetParent(mapCell);
        floorSpriteObject.transform.localPosition = new Vector3(0f, 0f, 0f);
        floorSpriteObject.transform.localRotation = Quaternion.identity;
        floorSpriteObject.transform.localScale = new Vector3(1f, 1f, 1f);
        floorRenderer.sortingOrder = 1;
    }

    private void MovePlayerSpriteToCell(Transform canvasCell)
    {
        _playerMapIcon.transform.SetParent(canvasCell);
        _playerMapIcon.transform.localPosition = new Vector3(0f, 0f, 0f);
    }

    private void UpdatePlayerSpritePositionRotation()
    {
        // Position
        // TODO: Get playing position relative to current tile
        // Rotation
        _playerMapIcon.transform.localRotation =
            Quaternion.Euler(new Vector3(0f, 0f, -(_gameManager.player.gameObject.transform.eulerAngles.y + 90)));
    }

    private void AddWallSprites(MazeCell mazeCell, Transform canvasCell, String prefix)
    {
        MazeCell northCell = _mazeCellManager.GetCellIfExists(mazeCell.x - 1, mazeCell.z);
        MazeCell southCell = _mazeCellManager.GetCellIfExists(mazeCell.x + 1, mazeCell.z);
        MazeCell eastCell = _mazeCellManager.GetCellIfExists(mazeCell.x, mazeCell.z + 1);
        MazeCell westCell = _mazeCellManager.GetCellIfExists(mazeCell.x, mazeCell.z - 1);
        if (mazeCell.hasNorthWall || northCell != null && northCell.hasSouthWall)
        {
            GameObject northWallObject = new GameObject($"{prefix}_{mazeCell.name}_North_Wall");
            northWallObject.transform.SetParent(canvasCell);
            SpriteRenderer northWallSprite = northWallObject.AddComponent<SpriteRenderer>();
            northWallSprite.sprite = wallSprite;
            if (prefix == "MiniMap")
            {
                northWallObject.transform.localPosition = new Vector3(5f, 0f, 0f);
                northWallObject.transform.localRotation = Quaternion.identity;
            }
            else
            {
                northWallObject.transform.localPosition = new Vector3(0f, 5f, 0f);
                northWallObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            }

            northWallObject.transform.localScale = new Vector3(1f, 1f, 1f);
            northWallSprite.sortingOrder = 1;
        }

        if (mazeCell.hasSouthWall || southCell != null && southCell.hasNorthWall)
        {
            GameObject southWallObject = new GameObject($"{prefix}_{mazeCell.name}_South_Wall");
            southWallObject.transform.SetParent(canvasCell);
            SpriteRenderer southWallSprite = southWallObject.AddComponent<SpriteRenderer>();
            southWallSprite.sprite = wallSprite;
            if (prefix == "MiniMap")
            {
                southWallObject.transform.localPosition = new Vector3(-5f, 0f, 0f);
                southWallObject.transform.localRotation = Quaternion.identity;
            }
            else
            {
                southWallObject.transform.localPosition = new Vector3(0f, -5f, 0f);
                southWallObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            }

            southWallObject.transform.localScale = new Vector3(1f, 1f, 1f);
            southWallSprite.sortingOrder = 1;
        }

        if (mazeCell.hasEastWall || eastCell != null && eastCell.hasWestWall)
        {
            GameObject eastWallObject = new GameObject($"{prefix}_{mazeCell.name}_East_Wall");
            eastWallObject.transform.SetParent(canvasCell);
            SpriteRenderer eastWallSprite = eastWallObject.AddComponent<SpriteRenderer>();
            eastWallSprite.sprite = wallSprite;
            if (prefix == "MiniMap")
            {
                eastWallObject.transform.localPosition = new Vector3(0f, -5f, 0f);
                eastWallObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            }
            else
            {
                eastWallObject.transform.localPosition = new Vector3(5f, 0f, 0f);
                eastWallObject.transform.localRotation = Quaternion.identity;
            }

            eastWallObject.transform.localScale = new Vector3(1f, 1f, 1f);
            eastWallSprite.sortingOrder = 1;
        }

        if (mazeCell.hasWestWall || westCell != null && westCell.hasEastWall)
        {
            GameObject westWallObject = new GameObject($"{prefix}_{mazeCell.name}_West_Wall");
            westWallObject.transform.SetParent(canvasCell);
            SpriteRenderer westWallSprite = westWallObject.AddComponent<SpriteRenderer>();
            westWallSprite.sprite = wallSprite;

            if (prefix == "MiniMap")
            {
                westWallObject.transform.localPosition = new Vector3(0f, 5f, 0f);
                westWallObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            }
            else
            {
                westWallObject.transform.localPosition = new Vector3(-5f, 0f, 0f);
                westWallObject.transform.localRotation = Quaternion.identity;
            }

            westWallObject.transform.localScale = new Vector3(1f, 1f, 1f);
            westWallSprite.sortingOrder = 1;
        }
    }

    private IEnumerator BlinkBatteryLevel(float timeToWait)
    {
        _batteryLevelBlinking = true;
        _batteryLevelText.SetActive(true);
        yield return new WaitForSeconds(timeToWait);
        _batteryLevelText.SetActive(false);
        yield return new WaitForSeconds(timeToWait);
        _batteryLevelBlinking = false;
    }


    public void ShowPauseUi()
    {
        _pauseCanvas.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _player.lockPlayer = true;
    }

    private void ResumeGame()
    {
        _gameManager.gameIsPaused = false;
        _pauseCanvas.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        _player.lockPlayer = false;
    }

    public void ShowExitUi()
    {
        _exitReachedText.enabled = true;
        _exitReachedButtons.SetActive(true);
        // set posX of ack to menu to 0
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        _player.lockPlayer = true;
        if (SceneManager.GetActiveScene().name != "Level7") return;
        _exitReachedText.text = "Congrats beta tester, you've been through all the levels !!";
        _exitReachedText.fontSize = 17;
        _backToMenuButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    }

    public void UpdateDiscoveryText(int discoveredTiles, int mapSize)
    {
        _discoveryText.text = discoveredTiles > 0
            ? $"Discovered {Math.Round((double)discoveredTiles / mapSize * 100)}%"
            : "";
    }

    public void AddInfoMessage(string message)
    {
        //TODO: Just handle 5 text max, don't destroy but animate/hide them instead
        // Handle position of previous texts if any 
        if (_infoPanel.transform.childCount > 0)
        {
            foreach (Transform previousText in _infoPanel.transform)
            {
                RectTransform rectTransform = previousText.GetComponent<RectTransform>();
                if (rectTransform.anchoredPosition.y == 80)
                {
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