using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
// Managers
    private TileManager _tileManager;
    private FragmentManager _fragmentManager;
    private UiManager _uiManager;

    // Player components
    public Transform playerPrefab;
    public Transform arrowPrefab;
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
    public GameObject previousTile;
    public GameObject currentTile;
    public int tryCount;
    public int tryMax;
    public List<Fragment> mapFragments = new List<Fragment>();
    public List<GameObject> revealedTilesInRun = new List<GameObject>();
    public List<string> totalDiscoveredTiles = new List<string>();

    // Environment
    public PlayerData playerData;
    public LevelData levelData;
    public bool gameIsPaused;
    public bool allFragmentsPickedUp;
    private GameObject _arrows;
    private GameObject _batteries;
    private bool _isDead;
    public int levelNumber;
    public int onboardingStage;
    private bool _incrementOnboardingIsRunning;

    private void Awake() {
        tryCount = 1;
        _tileManager = GameObject.Find("TileManager").GetComponent<TileManager>();
        _uiManager = GameObject.Find("UIManager").GetComponent<UiManager>();
        _fragmentManager = GameObject.Find("FragmentManager").GetComponent<FragmentManager>();
        _arrows = GameObject.Find("Arrows").gameObject;
        _batteries = GameObject.Find("Batteries").gameObject;

        levelData = FileManager.LoadLevelDataFile(SceneManager.GetActiveScene().name);
        if (levelData == null) {
            Debug.Log("No Data to load");
            mapFragments = _fragmentManager.GenerateRandomFragments(_tileManager.GetAllTiles(),
                _tileManager.GetTilesByType("Floor"), _tileManager);
        }
        else {
            Debug.Log("Data to load");
            if (levelData.mapFragments == null) {
                mapFragments = _fragmentManager.GenerateRandomFragments(_tileManager.GetAllTiles(),
                    _tileManager.GetTilesByType("Floor"), _tileManager);
            }
            else {
                mapFragments = levelData.mapFragments;
                allFragmentsPickedUp = levelData.allFragmentsPickedUp;
                totalDiscoveredTiles = levelData.totalDiscoveredTiles;
            }

            tryCount = levelData.tryCount + 1;
            Debug.Log(
                $"Data loaded: try {tryCount} \n mapFragments {mapFragments.Count} \n discoveredTiles: {totalDiscoveredTiles.Count}");
        }

        string sceneName = SceneManager.GetActiveScene().name;
        int.TryParse(sceneName.Substring(sceneName.Length - 1), out levelNumber);

        tryMax = 4;
        InstantiateBatteries();

        foreach (Fragment fragment in mapFragments) {
            if (!fragment.discovered) _fragmentManager.InstantiateFragment(fragment);
            if (!fragment.arrowRevealed) continue;
            GameObject tile = GameObject.Find(fragment.spawnTile);
            InstantiateArrow(tile.transform, tile.GetComponent<Tile>().action);
        }

        playerData = FileManager.LoadPlayerDataFile();
    }


    private void Start() {
        _tileManager.DoPathPlanning();
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
        if (_tileManager.GetTileUnderPlayer() != currentTile || CheckForTileDiscovery()) {
            previousTile = currentTile;
            currentTile = _tileManager.GetTileUnderPlayer();
            _uiManager.UpdateMiniMap();
            _uiManager.DrawMap(totalDiscoveredTiles);
            if (currentTile.CompareTag("Exit")) {
                if (playerData.levelCompleted < levelNumber) {
                    playerData.levelCompleted += 1;
                    playerData.cash += 1;
                    _uiManager.AddInfoMessage($"Obtained 1 coin, total : {playerData.cash}");
                }

                FileManager.SavePlayerDataFile(playerData);
                FileManager.SaveLevelDataFile(
                    new LevelData(tryCount, mapFragments, totalDiscoveredTiles, allFragmentsPickedUp),
                    SceneManager.GetActiveScene().name);
                _uiManager.ShowExitUi();
                _playerSoundsAudioSource.PlayOneShot(congratulationsAudio);
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

        // if (Input.GetMouseButtonDown(1)) {
        //     onboardingStage++;
        //     Debug.Log(($"Onboarding stage: {onboardingStage}"));
        // }

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
                    StartCoroutine(IncrementOnboardingAfter(timeBetweenStages, "Move with WASD keys. Look around with the mouse"));
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
                    StartCoroutine(IncrementOnboardingAfter(timeBetweenStages, "Find the exit before your battery runs out. Collect batteries to recharge on the go"));
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
                    StartCoroutine(IncrementOnboardingAfter(timeBetweenStages, "You get a coin for discovering the whole map and reaching the exit. Use these coins to upgrade your skills in the menu"));
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
            if (!_tileManager.HasBeenRevealed(tile.gameObject, revealedTilesInRun)
                && (tile.gameObject.CompareTag("Floor")
                    || tile.gameObject.CompareTag("Obstacle")
                    || tile.gameObject.CompareTag("Exit")
                    || tile.gameObject.CompareTag("Wall"))) {
                needMapUpdate = true;
                _tileManager.AddToRevealedTiles(tile.gameObject, revealedTilesInRun);
            }
        }

        return needMapUpdate;
    }

    public void Retry() {
        FileManager.SaveLevelDataFile(
            tryCount < tryMax
                ? new LevelData(tryCount, mapFragments, totalDiscoveredTiles, allFragmentsPickedUp)
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
                new LevelData(tryCount, mapFragments, totalDiscoveredTiles, allFragmentsPickedUp),
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
        List<GameObject> floorTiles = _tileManager.GetTilesByType("Floor");
        floorTiles = floorTiles.OrderBy(t => t.GetComponent<Tile>().score).ToList();
        // Get 10 % furthest tiles
        int index = floorTiles.Count - Random.Range(1, (int) Math.Round(floorTiles.Count / 10f));
        GameObject tile = floorTiles[index];
        Vector3 position = tile.transform.position;
        Transform playerTransform = Instantiate(playerPrefab, new Vector3(
            position.x,
            0,
            position.z
        ), Quaternion.identity);
        player = playerTransform.gameObject;
        currentTile = tile;
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
        List<GameObject> availableFloorTiles = _tileManager.GetTilesByType("Floor");
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
            if (totalDiscoveredTiles.Count == 0 || !totalDiscoveredTiles.Contains(tile)) {
                totalDiscoveredTiles.Add(tile);
            }
        });
        _uiManager.DrawMap(totalDiscoveredTiles);
        _uiManager.UpdateDiscoveryText(totalDiscoveredTiles.Count, _tileManager.GetMapSize());
        _uiManager.AddInfoMessage("Fragment picked up");
        // Randomly spawn arrow

        if (Random.Range(1, 100) <= playerData.spawnArrowChance) {
            _uiManager.AddInfoMessage("Helping arrow spawned");
            fragment.arrowRevealed = true;
        }

        if (fragment.arrowRevealed) {
            InstantiateArrow(currentTile.transform, currentTile.GetComponent<Tile>().action);
        }

        if (mapFragments.Where(fr => fr.discovered).ToList().Count == mapFragments.Count) {
            // All fragments have been found
            playerData.cash += 1;
            _uiManager.AddInfoMessage("Map fully discovered");
            _uiManager.AddInfoMessage($"Obtained 1 coin, total : {playerData.cash}");
            allFragmentsPickedUp = true;
        }

        Destroy(fragmentIn);
    }

    public void PickupBattery(GameObject batteryIn) {
        player.GetComponent<Player>().fuelCount += (float) Math.Round(playerData.batteryMax / 5f);
        _uiManager.AddInfoMessage("Battery picked up");
        _playerSoundsAudioSource.PlayOneShot(batteryPickupAudio);
        Destroy(batteryIn);
    }

    private void InstantiateArrow(Transform tileTransform, string direction) {
        if (GameObject.Find($"Arrow_{tileTransform.gameObject.name}")) {
            Destroy(GameObject.Find($"Arrow_{tileTransform.gameObject.name}"));
        }

        float angle = 0;
        switch (direction) {
            case "BACKWARD":
                angle = 0;
                break;
            case "LEFT":
                angle = 90;
                break;
            case "FORWARD":
                angle = 180;
                break;
            case "RIGHT":
                angle = 270;
                break;
        }

        Transform arrow = Instantiate(arrowPrefab, new Vector3(
            tileTransform.position.x,
            tileTransform.position.y + 0.05f,
            tileTransform.position.z
        ), Quaternion.identity);
        arrow.name = $"Arrow_{tileTransform.gameObject.name}";
        arrow.transform.eulerAngles = new Vector3(0, angle, 0);
        arrow.SetParent(_arrows.transform);
    }
}