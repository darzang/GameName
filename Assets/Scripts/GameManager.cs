using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
    public static string CurrentVersion = "0.3";
    // Managers
    private MazeCellManager _mazeCellManager;
    private FragmentManager _fragmentManager;
    private UiManager _uiManager;

    // Player components / Related
    public Player player;
    public Transform playerPrefab;
    public Transform batteryPrefab;
    private GameObject _eyeLids;
    private Animation _anim;
    private Light _playerLamp;
    
    
    private AudioSource _lightAudio;
    private AudioSource _playerSoundsAudioSource;
    public AudioClip[] lightSounds;
    public AudioClip batteryDeadAudio;
    public AudioClip fragmentPickupAudio;
    public AudioClip batteryPickupAudio;
    public AudioClip youLostAudio;
    public AudioClip openingAudio;
    public AudioClip[] welcomeToLevelAudio;
    public AudioClip congratulationsAudio;

    // Cells
    public MazeCell currentCell;
    public int tryMax;
    
    // Environment
    public PlayerData playerData;
    public LevelData levelData;
    public bool gameIsPaused;
    private GameObject _batteries;
    public int levelNumber;
    public int onboardingStage;
    private bool _incrementOnboardingIsRunning;

    private void Awake() {
        _mazeCellManager = GameObject.Find("MazeCellManager").GetComponent<MazeCellManager>();
        _uiManager = GameObject.Find("UIManager").GetComponent<UiManager>();
        _fragmentManager = GameObject.Find("FragmentManager").GetComponent<FragmentManager>();
        _batteries = GameObject.Find("Batteries").gameObject;

        levelData = FileManager.LoadLevelDataFile(SceneManager.GetActiveScene().name);
        levelData.mapFragments = levelData.mapFragments ?? _fragmentManager.GenerateRandomFragments();
        levelData.tryCount += 1;
            
        string sceneName = SceneManager.GetActiveScene().name;
        int.TryParse(sceneName.Substring(sceneName.Length - 1), out levelNumber);

        tryMax = 4;
        InstantiateBatteries();

        foreach (Fragment fragment in levelData.mapFragments) {
            if (!fragment.discovered) _fragmentManager.InstantiateFragment(fragment);
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
        if (_mazeCellManager.GetTileUnder(player.transform.gameObject) != currentCell || CheckForTileDiscovery()) {
            currentCell = _mazeCellManager.GetTileUnder(player.transform.gameObject);
            if (currentCell != null) {
                if (currentCell.isExit) {
                    if (playerData.levelCompleted < levelNumber) {
                        playerData.levelCompleted += 1;
                        playerData.cash += 1;
                        _uiManager.AddInfoMessage($"Obtained 1 coin, total : {playerData.cash}");
                    }

                    FileManager.SavePlayerDataFile(playerData);
                    FileManager.SaveLevelDataFile(levelData, SceneManager.GetActiveScene().name);
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
            _uiManager.DrawWholeMap();
        }

        if (player.isDead) {
            _playerSoundsAudioSource.PlayOneShot(batteryDeadAudio);
            if (levelData.tryCount >= tryMax) _playerSoundsAudioSource.PlayOneShot(youLostAudio);
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
            if (tile.name != "Floor" || !tile.transform.parent.name.StartsWith("MazeCell")) continue;
            // Colliders detected will be floor and walls so we need to get the parent cell
            Transform transform = tile.transform;
            Transform parent = transform.parent;
            MazeCell mazeCell = _mazeCellManager.GetCellByName(parent.name);
            if (mazeCell.revealedForCurrentRun) continue;
            needMapUpdate = true;
            mazeCell.revealedForCurrentRun = true;
        }

        return needMapUpdate;
    }

    public void Retry() {
        string currentLevel = SceneManager.GetActiveScene().name;
        FileManager.SaveLevelDataFile(
            levelData.tryCount < tryMax ? levelData : null,
            currentLevel
        );
        //TODO: this will break with more than 10 levels
        int levelNumber = int.Parse(currentLevel.Substring(currentLevel.Length - 1));
        StartCoroutine(CloseEyes(levelNumber));

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GiveUp() {
        FileManager.SaveLevelDataFile(
            levelData.tryCount < tryMax ? levelData : null,
            SceneManager.GetActiveScene().name
        );
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
        // Get available cells
        List<MazeCell> mazeCells = _mazeCellManager.mazeCells.Where(cell => !cell.isExit &!cell.hasBattery && !cell.hasFragment).OrderBy(cell => cell.score).ToList();
        // Get 10 % furthest tiles
        int index = mazeCells.Count - Random.Range(1, (int) Math.Round(mazeCells.Count / 10f));
        MazeCell spawnCell = mazeCells[index];
        Transform playerTransform = Instantiate(playerPrefab, new Vector3(
            spawnCell.x,
            0.5f,
            spawnCell.z
        ), Quaternion.identity);
        player = playerTransform.GetComponent<Player>();
        currentCell = spawnCell;
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
        List<MazeCell> availableFloorTiles = _mazeCellManager.mazeCells.Where(cell => !cell.isExit && !cell.hasBattery && !cell.hasFragment).ToList();
        for (int i = 0; i < tryMax; i++) {
            MazeCell mazeCell = availableFloorTiles[Random.Range(0, availableFloorTiles.Count - 1)];
            Transform battery = Instantiate(batteryPrefab, new Vector3(
                mazeCell.x,
                0.35f,
                mazeCell.z
            ), Quaternion.identity);
            availableFloorTiles.Remove(mazeCell);
            battery.SetParent(_batteries.transform);
            _mazeCellManager.GetCellByName(mazeCell.name).hasBattery = true;
        }
    }

    public void PickupFragment(GameObject fragmentIn) {
        int row = (int) fragmentIn.transform.position.x;
        int column = (int) fragmentIn.transform.position.z;
        MazeCell mazeCell = _mazeCellManager.GetCellByName($"MazeCell_{row}_{column}");
        Fragment fragment = FragmentManager.GetFragmentForTile(levelData.mapFragments, mazeCell);
        fragment.discovered = true;
        _playerSoundsAudioSource.PlayOneShot(fragmentPickupAudio);
        fragment.cellsInFragment.ForEach(cell => {
            MazeCell cellInFragment = _mazeCellManager.GetCellByName(cell.name);
            cellInFragment.permanentlyRevealed = true;
        });
        _uiManager.UpdateDiscoveryText(_mazeCellManager.GetDiscoveredCellsCount(), _mazeCellManager.GetMapSize());
        _uiManager.AddInfoMessage("Fragment picked up");
        // Randomly spawn arrow
        // if (Random.Range(1, 100) <= playerData.spawnArrowChance) {
        //     _uiManager.AddInfoMessage("Helping arrow spawned");
        //     mazeCell.hasArrow = true;
        // }
        

        if (_mazeCellManager.AllCellsDiscovered()) {
            // All fragments have been found
            playerData.cash += 1;
            _uiManager.AddInfoMessage("Map fully discovered");
            _uiManager.AddInfoMessage($"Obtained 1 coin, total : {playerData.cash}");
        }
        Destroy(fragmentIn);
        FileManager.SaveLevelDataFile(levelData, SceneManager.GetActiveScene().name);
        FileManager.SavePlayerDataFile(playerData);
    }

    public void PickupBattery(GameObject batteryIn) {
        player.GetComponent<Player>().fuelCount += (float) Math.Round(playerData.batteryMax / 5f);
        _uiManager.AddInfoMessage("Battery picked up");
        _playerSoundsAudioSource.PlayOneShot(batteryPickupAudio);
        Destroy(batteryIn);
    }
}