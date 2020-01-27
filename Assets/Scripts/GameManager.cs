using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
// Managers
    public TileManager tileManager;
    public FragmentManager fragmentManager;
    public UIManager uiManager;

    // Player components
    public Transform playerPrefab;
    public Transform fragmentPrefab;
    public Transform arrowPrefab;
    public Transform batteryPrefab;
    public GameObject player;
    private Light playerLamp;
    private AudioSource lightAudio;
    public AudioClip[] lightSounds;
    public AudioClip batteryDeadAudio;
    private AudioSource playerAudio;
    private bool isDead;
    public AudioClip fragmentPickupAudio;
    public AudioClip youLostAudio;
    public GameObject arrows;

    public GameObject fragments;

    // Tiles
    public GameObject startingTile;
    public GameObject previousTile;
    public GameObject currentTile;
    public int tryCount;
    public int tryMax;
    public List<Fragment> mapFragments = new List<Fragment>();
    public List<GameObject> revealedTilesInRun;
    public List<string> totalDiscoveredTiles = new List<string>();

    // Environment
    public GameObject ceiling;
    public PlayerData playerData;
    public bool gameIsPaused;
    public bool allFragmentsPickedUp;

    private void Awake() {
        tryCount = 1;
        ceiling.SetActive(true);
        tileManager.DoPathPlanning();
        LevelData levelData = FileManager.LoadLevelDataFile(SceneManager.GetActiveScene().name);
        if (levelData == null) {
            Debug.Log("No Data to load");
            mapFragments = fragmentManager.GenerateRandomFragments(tileManager.GetAllTiles(),
                tileManager.GetTilesByType("Floor"), tileManager);
        }
        else {
            if (levelData.mapFragments == null) {
                mapFragments = fragmentManager.GenerateRandomFragments(tileManager.GetAllTiles(),
                    tileManager.GetTilesByType("Floor"), tileManager);
            }
            else {
                mapFragments = levelData.mapFragments;
                allFragmentsPickedUp = levelData.allFragmentsPickedUp;
                totalDiscoveredTiles = levelData.totalDiscoveredTiles;
                uiManager.DrawMap(totalDiscoveredTiles);
                uiManager.UpdateDiscoveryText(totalDiscoveredTiles.Count, tileManager.GetMapSize());
                if (totalDiscoveredTiles.Count > 0) uiManager.AddInfoMessage("Previous data loaded");
            }

            tryCount = levelData.tryCount + 1;


            Debug.Log(
                $"Data loaded: try {tryCount} \n mapFragments {mapFragments.Count} \n discoveredTiles: {totalDiscoveredTiles.Count}");
        }

        string sceneName = SceneManager.GetActiveScene().name;
        Int32.TryParse(sceneName.Substring(sceneName.Length - 1), out int levelNumber);

        tryMax = levelNumber + 3;
        InstantiateBatteries();

        foreach (Fragment fragment in mapFragments) {
            if (!fragment.discovered) fragmentManager.InstantiateFragment(fragment);
            // if (!fragment.discovered) InstantiateFragment(fragment);
            if (fragment.arrowRevealed) {
                GameObject tile = GameObject.Find(fragment.spawnTile);
                InstantiateArrow(tile.transform, tile.GetComponent<Tile>().action);
            }
        }

        playerData = FileManager.LoadPlayerDataFile();

        InstantiatePlayer();
        if (levelData != null) {
            uiManager.DrawMap(totalDiscoveredTiles);
        }

        playerAudio = player.GetComponent<AudioSource>();
        playerLamp = player.GetComponentInChildren<Light>();
        lightAudio = playerLamp.GetComponent<AudioSource>();
        playerLamp.range = 1.5f * playerData.lightMultiplier;
        playerLamp.intensity = 1 * playerData.lightMultiplier;
        playerLamp.spotAngle = playerLamp.spotAngle * playerData.lightMultiplier;
    }

    private void Update() {
        // Is the player on a new tile ?
        if (tileManager.GetTileUnderPlayer() != currentTile || CheckForTileDiscovery()) {
            previousTile = currentTile;
            currentTile = tileManager.GetTileUnderPlayer();
            uiManager.UpdateMiniMap();
            uiManager.DrawMap(totalDiscoveredTiles);
            if (currentTile.CompareTag("Exit")) {
                string sceneName = SceneManager.GetActiveScene().name;
                // FileManager.DeleteFile(sceneName);
                Int32.TryParse(sceneName.Substring(sceneName.Length - 1), out int levelNumber);
                if (playerData.levelCompleted < levelNumber) {
                    playerData.levelCompleted += 1;
                    if (levelNumber % 5 == 0) {
                        playerData.cash += 5;
                        uiManager.AddInfoMessage($"Obtained 5 coin, total : {playerData.cash}");
                    }
                    else {
                        playerData.cash += 1;
                        uiManager.AddInfoMessage($"Obtained 1 coin, total : {playerData.cash}");
                    }
                }

                FileManager.SavePlayerDataFile(playerData);
                FileManager.SaveLevelDataFile(
                    new LevelData(tryCount, mapFragments, totalDiscoveredTiles, allFragmentsPickedUp),
                    SceneManager.GetActiveScene().name);
                uiManager.ShowExitUi();
            }
        }

        // Toggle lamp
        if (Input.GetMouseButtonDown(0)) {
            if (playerLamp.enabled) {
                lightAudio.clip = lightSounds[1];
                uiManager.AddInfoMessage("Lamp Disabled");
            }
            else {
                lightAudio.clip = lightSounds[0];
                uiManager.AddInfoMessage("Lamp enabled");
            }

            lightAudio.Play();
            playerLamp.enabled = !playerLamp.enabled;
        }

        if (player.GetComponent<Player>().fuelCount <= 0 && !isDead) {
            isDead = true;
            playerAudio.PlayOneShot(batteryDeadAudio);
            if (tryCount >= tryMax) playerAudio.PlayOneShot(youLostAudio);
        }

        // Useful for now, to remove later
        if (Input.GetKeyUp("r")) FileManager.DeleteFile(SceneManager.GetActiveScene().name);
        // if (Input.GetKeyUp("n")) NextLevel();
        if (Input.GetKeyUp("p") || Input.GetKeyUp(KeyCode.Escape)) {
            gameIsPaused = true;
            uiManager.ShowPauseUi();
        }
    }

    private bool CheckForTileDiscovery() {
        bool needMapUpdate = false;
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, playerData.discoveryRange);
        foreach (Collider tile in hitColliders) {
            if (!tileManager.HasBeenRevealed(tile.gameObject, revealedTilesInRun)
                && (tile.gameObject.CompareTag("Floor")
                    || tile.gameObject.CompareTag("Obstacle")
                    || tile.gameObject.CompareTag("Exit")
                    || tile.gameObject.CompareTag("Wall"))) {
                needMapUpdate = true;
                tileManager.AddToRevealedTiles(tile.gameObject, revealedTilesInRun);
            }
        }

        return needMapUpdate;
    }

    public void Retry() {
        if (tryCount < tryMax) {
            FileManager.SaveLevelDataFile(
                new LevelData(tryCount, mapFragments, totalDiscoveredTiles, allFragmentsPickedUp),
                SceneManager.GetActiveScene().name);
        }
        else {
            FileManager.SaveLevelDataFile(new LevelData(0, null, null, allFragmentsPickedUp),
                SceneManager.GetActiveScene().name);
        }

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

        SceneManager.LoadScene("MenuScene");
    }

    public void BackToMenu() {
        SceneManager.LoadScene("MenuScene");
    }

    public void NextLevel() {
        string currentLevel = SceneManager.GetActiveScene().name;
        int levelNumber = int.Parse(currentLevel.Substring(currentLevel.Length - 1));
        string sceneToLoad = $"Level{levelNumber + 1}";
        SceneManager.LoadScene(sceneToLoad);
    }

    private void InstantiatePlayer() {
        // Get the furthest tile
        List<GameObject> floorTiles = tileManager.GetTilesByType("Floor");
        floorTiles = floorTiles.OrderBy(t => t.GetComponent<Tile>().score).ToList();
        // Get 10 % furthest tiles
        int index = floorTiles.Count - Random.Range(1, (int) Math.Round(floorTiles.Count / 10f));
        GameObject tile = floorTiles[index];
        Vector3 position = tile.transform.position;
        Transform playerTransform = Instantiate(playerPrefab, new Vector3(
            position.x,
            position.y + 0.53f,
            position.z
        ), Quaternion.identity);
        player = playerTransform.gameObject;
        startingTile = tile;
        currentTile = tile;
    }

    private void InstantiateBatteries() {
        List<GameObject> availablesFloorTiles = tileManager.GetTilesByType("Floor");
        foreach (Fragment fragment in mapFragments) {
            availablesFloorTiles.Remove(GameObject.Find(fragment.spawnTile));
        }
        
        for (int i = 0; i < tryMax; i++) {
            GameObject spawnTile = availablesFloorTiles[Random.Range(0, availablesFloorTiles.Count - 1)];
            Vector3 position = spawnTile.transform.position;
            Transform battery = Instantiate(batteryPrefab, new Vector3(
                position.x,
                position.y + 0.35f,
                position.z
            ), Quaternion.identity);
            availablesFloorTiles.Remove(spawnTile);
        }
    }
    
    public void PickupFragment(GameObject fragmentIn) {
        int fragmentNumber = int.Parse(fragmentIn.name.Split('_')[1]);
        Fragment fragment = mapFragments.Find(frag => frag.number == fragmentNumber);
        fragment.discovered = true;
        player.GetComponent<AudioSource>().PlayOneShot(fragmentPickupAudio);
        fragment.tiles.ForEach(tile => {
            if (totalDiscoveredTiles.Count == 0 || !totalDiscoveredTiles.Contains(tile)) {
                totalDiscoveredTiles.Add(tile);
            }
        });
        uiManager.DrawMap(totalDiscoveredTiles);
        uiManager.UpdateDiscoveryText(totalDiscoveredTiles.Count, tileManager.GetMapSize());
        uiManager.AddInfoMessage("Fragment picked up");
        // Randomly spawn arrow

        if (Random.Range(1, 100) <= playerData.spawnArrowChance) {
            uiManager.AddInfoMessage("Helping arrow spawned");
            fragment.arrowRevealed = true;
        }

        if (fragment.arrowRevealed) {
            InstantiateArrow(currentTile.transform, currentTile.GetComponent<Tile>().action);
        }

        if (mapFragments.Where(fr => fr.discovered == true).ToList().Count == mapFragments.Count) {
            // All fragments have been found
            playerData.cash += 1;
            uiManager.AddInfoMessage("Map fully discovered");
            uiManager.AddInfoMessage($"Obtained 1 coin, total : {playerData.cash}");
            allFragmentsPickedUp = true;
        }

        Destroy(fragmentIn);
    }

    public void PickupBattery(GameObject batteryIn) {
        player.GetComponent<Player>().fuelCount += (float) Math.Round(playerData.batteryMax / 5f);
        uiManager.AddInfoMessage("Battery picked up");
        Destroy(batteryIn);
    }

    public void InstantiateArrow(Transform tileTransform, string direction) {
        if (GameObject.Find($"Arrow_{tileTransform.gameObject.name}")) {
            Destroy(GameObject.Find($"Arrow_{tileTransform.gameObject.name}"));
        }

        float angle = 0;
        Quaternion transformRotation = Quaternion.identity;
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

        transformRotation.y = angle;
        Transform arrow = Instantiate(arrowPrefab, new Vector3(
            tileTransform.position.x,
            tileTransform.position.y + 0.05f,
            tileTransform.position.z
        ), Quaternion.identity);
        arrow.name = $"Arrow_{tileTransform.gameObject.name}";
        arrow.transform.eulerAngles = new Vector3(0, angle, 0);
        arrow.SetParent(arrows.transform);
    }
}