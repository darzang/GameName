using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
// Managers
    private MazeCellManager _mazeCellManager;
    private FragmentManager _fragmentManager;
    private UiManager _uiManager;

    // Player components
    public Transform playerPrefab;
    public Transform batteryPrefab;
    public GameObject player;
    private GameObject _eyeLids;
    private Animation _anim;
    private Light _playerLamp;


    private AudioSource _lightAudio;
    public AudioClip[] lightSounds;
    public AudioClip batteryDeadAudio;
    public AudioClip fragmentPickupAudio;
    public AudioClip batteryPickupAudio;
    public AudioClip youLostAudio;
    public AudioClip openingAudio;
    public AudioClip[] welcomeToLevelAudio;
    public AudioClip congratulationsAudio;

    private AudioSource _playerSoundsAudioSource;

    // Tiles
    public MazeCell previousCell;
    public MazeCell currentCell;
    public int tryCount;
    public int tryMax;
    public List<Fragment> mapFragments = new List<Fragment>();
    public List<MazeCell> revealedCellsInRun = new List<MazeCell>();
    public List<string> totalDiscoveredCellsNames = new List<string>();
    public List<MazeCell> totalDiscoveredCells = new List<MazeCell>();
    // Environment
    public PlayerData playerData;
    public LevelData levelData;
    public bool gameIsPaused;
    public bool allFragmentsPickedUp;
    private GameObject _batteries;
    private bool _isDead;
    public int levelNumber;
    public int onboardingStage;
    private bool _incrementOnboardingIsRunning;

    private void Awake() {
        tryCount = 1;
        _mazeCellManager = GameObject.Find("TileManager").GetComponent<MazeCellManager>();
        _uiManager = GameObject.Find("UIManager").GetComponent<UiManager>();
        _fragmentManager = GameObject.Find("FragmentManager").GetComponent<FragmentManager>();
        _batteries = GameObject.Find("Batteries").gameObject;

        levelData = FileManager.LoadLevelDataFile(SceneManager.GetActiveScene().name);
        if (levelData == null) {
            Debug.Log("No Data to load");
            mapFragments = _fragmentManager.GenerateRandomFragments(_mazeCellManager.GetAllTiles(), _mazeCellManager);
        }
        else {
            Debug.Log("Data to load");
            if (levelData.mapFragments == null) {
                mapFragments = _fragmentManager.GenerateRandomFragments(_mazeCellManager.GetAllTiles(), _mazeCellManager);
            }
            else {
                mapFragments = levelData.mapFragments;
                allFragmentsPickedUp = levelData.allFragmentsPickedUp;
                totalDiscoveredCellsNames = levelData.totalDiscoveredTiles;
            }

            tryCount = levelData.tryCount + 1;
            Debug.Log(
                $"Data loaded: try {tryCount} \n mapFragments {mapFragments.Count} \n discoveredTiles: {totalDiscoveredCellsNames.Count}");
        }

        string sceneName = SceneManager.GetActiveScene().name;
        int.TryParse(sceneName.Substring(sceneName.Length - 1), out levelNumber);

        tryMax = 4;
        InstantiateBatteries();

        foreach (Fragment fragment in mapFragments) {
            if (!fragment.discovered) _fragmentManager.InstantiateFragment(fragment);
            if (!fragment.arrowRevealed) continue;
            GameObject tile = GameObject.Find(fragment.spawnTile);
            _mazeCellManager.InstantiateArrow(tile.GetComponent<MazeCell>());
        }

        playerData = FileManager.LoadPlayerDataFile();
    }


    private void Start() {
        _mazeCellManager.DoPathPlanning();
        InstantiatePlayer();

        if (PlayerPrefs.GetInt("EnableSounds") == 0) {
            _playerSoundsAudioSource.enabled = false;
            _lightAudio.enabled = false;
            GameObject lights = GameObject.Find("Lights").gameObject;
            foreach (AudioSource lightAudio in lights.transform.GetComponentsInChildren<AudioSource>()) {
                lightAudio.enabled = false;
            }
        }

        if (PlayerPrefs.GetInt("EnableMusic") == 0) {
            player.transform.Find("BackgroundAudioSource").GetComponent<AudioSource>().enabled = false;
        }

        StartCoroutine(OpenEyes());
        _uiManager.Instantiation();
    }


    private void Update() {
        if (!playerData.onboardingDone) {
            HandleOnboarding();
        }

        // Is the player on a new tile ?
        if (_mazeCellManager.GetTileUnderPlayer() != currentCell || CheckForTileDiscovery()) {
            previousCell = currentCell;
            currentCell = _mazeCellManager.GetTileUnderPlayer();
            _uiManager.UpdateMiniMap();
            _uiManager.DrawMap(totalDiscoveredCellsNames);
            if (currentCell) {
                if (currentCell.isExit) {
                    if (playerData.levelCompleted < levelNumber) {
                        playerData.levelCompleted += 1;
                        playerData.cash += 1;
                        _uiManager.AddInfoMessage($"Obtained 1 coin, total : {playerData.cash}");
                    }

                    FileManager.SavePlayerDataFile(playerData);
                    FileManager.SaveLevelDataFile(
                        new LevelData(tryCount, mapFragments, totalDiscoveredCellsNames, allFragmentsPickedUp),
                        SceneManager.GetActiveScene().name);
                    _uiManager.ShowExitUi();
                    _playerSoundsAudioSource.PlayOneShot(congratulationsAudio);
                }
            }
            else {
                Debug.LogError("No current cell !");
            }
        }

        // Toggle lamp
        if (Input.GetMouseButtonDown(0)) {
            if (_playerLamp.enabled) {
                _lightAudio.clip = lightSounds[1];
                _lightAudio.Play();
                _uiManager.AddInfoMessage("Lamp Disabled");
            }
            else {
                _lightAudio.clip = lightSounds[0];
                _lightAudio.Play();
                _uiManager.AddInfoMessage("Lamp enabled");
            }

            _playerLamp.enabled = !_playerLamp.enabled;
        }

        if (Input.GetMouseButtonDown(1)) {
            _mazeCellManager.ShowCeiling(false);
        }

        if (player.GetComponent<Player>().fuelCount <= 0 && !_isDead) {
            _isDead = true;
            _playerSoundsAudioSource.PlayOneShot(batteryDeadAudio);
            if (tryCount >= tryMax) _playerSoundsAudioSource.PlayOneShot(youLostAudio);
        }

        // Useful for now, to remove later
        // if (Input.GetKeyUp("r")) FileManager.DeleteFile(SceneManager.GetActiveScene().name);
        // if (Input.GetKeyUp("n")) NextLevel();
        // if (Input.GetKeyUp("k")) FileManager.DeleteFile("playerData");
        if (Input.GetKeyUp("p") || Input.GetKeyUp(KeyCode.Escape)) {
            gameIsPaused = true;
            _uiManager.ShowPauseUi();
        }
    }

    private IEnumerator IncrementOnboardingAfter(float seconds = 3f, string stageText = "") {
        _incrementOnboardingIsRunning = true;
        if (onboardingStage != 0) {
            _uiManager.onboardingText.text = stageText;
            _uiManager.onboardingText.gameObject.SetActive(true);
            yield return new WaitForSeconds(seconds);
            _uiManager.onboardingText.gameObject.SetActive(false);
        }
        else {
            yield return new WaitForSeconds(seconds);
        }

        _incrementOnboardingIsRunning = false;
        onboardingStage++;
    }

    private void HandleOnboarding() {
        // TODO: Make text in Array / Dictionary for cleaner/ easier to maintain code
        float timeBetweenStages = 4f;
        switch (onboardingStage) {
            case 0:
                if (!_incrementOnboardingIsRunning) {
                    StartCoroutine(IncrementOnboardingAfter(2f));
                }

                break;
            case 1:
                if (!_incrementOnboardingIsRunning) {
                    StartCoroutine(IncrementOnboardingAfter(timeBetweenStages,
                        "Move with WASD keys. Look around with the mouse"));
                }

                break;
            case 2:
                if (!_incrementOnboardingIsRunning) {
                    StartCoroutine(IncrementOnboardingAfter(timeBetweenStages,
                        "Toggle your lamp with left mouse button. Be careful, it consumes your battery"));
                    StartCoroutine(_uiManager.OnboardingBlinkBattery());
                }

                break;
            case 3:
                if (!_incrementOnboardingIsRunning) {
                    StartCoroutine(IncrementOnboardingAfter(timeBetweenStages,
                        "Find the exit before your battery runs out. Collect batteries to recharge on the go"));
                }

                break;
            case 4:
                if (!_incrementOnboardingIsRunning) {
                    StartCoroutine(IncrementOnboardingAfter(timeBetweenStages,
                        "Collect fragments of the map to discover it permanently."));
                }

                break;
            case 5:
                if (!_incrementOnboardingIsRunning) {
                    StartCoroutine(IncrementOnboardingAfter(timeBetweenStages,
                        "You get a coin for discovering the whole map and reaching the exit. Use these coins to upgrade your skills in the menu"));
                }

                break;
            case 6:
                if (!_incrementOnboardingIsRunning) {
                    StartCoroutine(IncrementOnboardingAfter(timeBetweenStages, "Good luck.\n Don't die too much :)"));
                }

                break;
            case 7:
                _uiManager.onboardingText.gameObject.SetActive(false);
                playerData.onboardingDone = true;
                FileManager.SavePlayerDataFile(playerData);
                break;
        }
    }

    private IEnumerator OpenEyes() {
        _anim.Play("EyeLidOpen");
        yield return new WaitForSeconds(1);
        _eyeLids.SetActive(false);
        _playerSoundsAudioSource.PlayOneShot(welcomeToLevelAudio[levelNumber - 1]);
    }

    private IEnumerator CloseEyes(int levelNumber) {
        _eyeLids.SetActive(true);
        _playerLamp.enabled = false;
        _uiManager.HideCanvas();
        _playerSoundsAudioSource.PlayOneShot(openingAudio);
        _anim.Play("EyeLidClose");
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(levelNumber == 0 ? "MenuScene" : $"Level{levelNumber}");
    }

    private bool CheckForTileDiscovery() {
        bool needMapUpdate = false;
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, playerData.discoveryRange);
        foreach (Collider tile in hitColliders) {
            if (!_mazeCellManager.HasBeenRevealed(tile.GetComponent<MazeCell>(), revealedCellsInRun)
                && tile.gameObject.CompareTag("MazeCell")) {
                needMapUpdate = true;
                _mazeCellManager.AddToRevealedTiles(tile.GetComponent<MazeCell>(), revealedCellsInRun);
            }
        }

        return needMapUpdate;
    }

    public void Retry() {
        FileManager.SaveLevelDataFile(
            tryCount < tryMax
                ? new LevelData(tryCount, mapFragments, totalDiscoveredCellsNames, allFragmentsPickedUp)
                : new LevelData(0, null, null, allFragmentsPickedUp),
            SceneManager.GetActiveScene().name
        );

        string currentLevel = SceneManager.GetActiveScene().name;
        int levelNumber = int.Parse(currentLevel.Substring(currentLevel.Length - 1));
        StartCoroutine(CloseEyes(levelNumber));

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GiveUp() {
        if (tryCount < tryMax) {
            FileManager.SaveLevelDataFile(
                new LevelData(tryCount, mapFragments, totalDiscoveredCellsNames, allFragmentsPickedUp),
                SceneManager.GetActiveScene().name);
        }
        else {
            FileManager.DeleteFile(SceneManager.GetActiveScene().name);
        }

        BackToMenu();
    }

    public void BackToMenu() {
        StartCoroutine(CloseEyes(0));
    }

    public void NextLevel() {
        string currentLevel = SceneManager.GetActiveScene().name;
        int levelNumber = int.Parse(currentLevel.Substring(currentLevel.Length - 1));
        StartCoroutine(CloseEyes(levelNumber + 1));
    }

    private void InstantiatePlayer() {
        // Get floor tiles
        List<GameObject> floorTiles = new List<GameObject>();
        foreach (MazeCell mazeCell in _mazeCellManager.mazeCells) {
            floorTiles.Add(mazeCell.gameObject);
        }

        floorTiles = floorTiles.OrderBy(t => t.GetComponent<MazeCell>().score).ToList();
        // Get 10 % furthest tiles
        int index = floorTiles.Count - Random.Range(1, (int) Math.Round(floorTiles.Count / 10f));
        GameObject tile = floorTiles[index];
        Vector3 position = tile.transform.position;
        Transform playerTransform = Instantiate(playerPrefab, new Vector3(
            position.x,
            0.5f,
            position.z
        ), Quaternion.identity);
        player = playerTransform.gameObject;
        currentCell = tile.GetComponent<MazeCell>();
        _playerLamp = player.GetComponentInChildren<Light>();
        _playerLamp.enabled = false;
        _playerLamp.range *= playerData.lightMultiplier;
        _playerLamp.intensity *= playerData.lightMultiplier;
        _playerLamp.spotAngle *= playerData.lightMultiplier;
        _playerSoundsAudioSource = player.transform.Find("SoundsAudioSource").GetComponent<AudioSource>();
        _lightAudio = _playerLamp.GetComponent<AudioSource>();
        _eyeLids = GameObject.Find("EyeLids").gameObject;
        _anim = player.GetComponent<Animation>();
    }

    private void InstantiateBatteries() {
        List<GameObject> availableFloorTiles = _mazeCellManager.GetAllTiles();
        foreach (Fragment fragment in mapFragments) {
            availableFloorTiles.Remove(GameObject.Find(fragment.spawnTile));
        }

        for (int i = 0; i < tryMax; i++) {
            GameObject spawnTile = availableFloorTiles[Random.Range(0, availableFloorTiles.Count - 1)];
            Vector3 position = spawnTile.transform.position;
            Transform battery = Instantiate(batteryPrefab, new Vector3(
                position.x,
                position.y + 0.35f,
                position.z
            ), Quaternion.identity);
            availableFloorTiles.Remove(spawnTile);
            battery.SetParent(_batteries.transform);
        }
    }

    public void PickupFragment(GameObject fragmentIn) {
        int fragmentNumber = int.Parse(fragmentIn.name.Split('_')[1]);
        Fragment fragment = mapFragments.Find(frag => frag.number == fragmentNumber);
        fragment.discovered = true;
        _playerSoundsAudioSource.PlayOneShot(fragmentPickupAudio);
        fragment.tiles.ForEach(tile => {
            if (totalDiscoveredCellsNames.Count == 0 || !totalDiscoveredCellsNames.Contains(tile)) {
                totalDiscoveredCellsNames.Add(tile);
            }
        });
        _uiManager.DrawMap(totalDiscoveredCellsNames);
        _uiManager.UpdateDiscoveryText(totalDiscoveredCellsNames.Count, _mazeCellManager.GetMapSize());
        _uiManager.AddInfoMessage("Fragment picked up");
        // Randomly spawn arrow

        if (Random.Range(1, 100) <= playerData.spawnArrowChance) {
            _uiManager.AddInfoMessage("Helping arrow spawned");
            fragment.arrowRevealed = true;
        }

        if (fragment.arrowRevealed) {
            _mazeCellManager.InstantiateArrow(currentCell);
        }

        if (mapFragments.Where(fr => fr.discovered).ToList().Count == mapFragments.Count) {
            // All fragments have been found
            playerData.cash += 1;
            _uiManager.AddInfoMessage("Map fully discovered");
            _uiManager.AddInfoMessage($"Obtained 1 coin, total : {playerData.cash}");
            allFragmentsPickedUp = true;
        }

        Debug.Log($"Picking up fragment: {fragmentIn}");
        Destroy(fragmentIn);
    }

    public void PickupBattery(GameObject batteryIn) {
        player.GetComponent<Player>().fuelCount += (float) Math.Round(playerData.batteryMax / 5f);
        _uiManager.AddInfoMessage("Battery picked up");
        _playerSoundsAudioSource.PlayOneShot(batteryPickupAudio);
        Destroy(batteryIn);
    }
    
}