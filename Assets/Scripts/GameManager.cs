﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
// Managers
    public TileManager tileManager;

    public UIManager uiManager;

    // Player components
    public Transform playerPrefab;
    public Transform fragmentPrefab;
    public Transform arrowPrefab;
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
    public GameObject currentTile;
    public int tryCount;
    public int tryMax;
    public List<Fragment> mapFragments = new List<Fragment>();
    public List<GameObject> revealedTilesInRun;
    public List<string> discoveredTiles = new List<string>();


    // Environment
    public GameObject ceiling;

    public float discoveryRange = 0.75f;
    public PlayerData playerData;

    private void Awake() {
        tryCount = 1;
        ceiling.SetActive(true);
        tileManager.DoPathPlanning();

        LevelData levelData = FileManager.LoadLevelDataFile(SceneManager.GetActiveScene().name);
        if (levelData == null) {
            Debug.Log("No Data to load");
        }
        else {
            mapFragments = levelData.mapFragments;
            tryCount = levelData.tryCount + 1;
            discoveredTiles = levelData.totalDiscoveredTiles;
            foreach (Fragment fragment in mapFragments) {
                if (!fragment.discovered) InstantiateFragment(fragment);
                if (fragment.arrowRevealed) {
                    GameObject tile = GameObject.Find(fragment.spawnTile);
                    InstantiateArrow(tile.transform, tile.GetComponent<Tile>().action);
                }
            }
            uiManager.DrawMap(discoveredTiles);
            uiManager.UpdateDiscoveryText(discoveredTiles.Count, tileManager.GetMapSize());
            Debug.Log($"Data loaded: try {tryCount} \n mapFragments {mapFragments.Count} \n discoveredTiles: {discoveredTiles.Count}");
            if (discoveredTiles.Count > 0) uiManager.AddInfoMessage("Previous data loaded");
        }

        playerData = FileManager.LoadPlayerDataFile();
        
        tryMax = playerData.levelCompleted + 4;
        InstantiatePlayer();
        if (levelData != null) {
            uiManager.DrawMap(discoveredTiles);
        }

        playerAudio = player.GetComponent<AudioSource>();
        playerLamp = player.GetComponentInChildren<Light>();
        lightAudio = playerLamp.GetComponent<AudioSource>();

        // StartCoroutine(tileManager.DoPathPlanningCoroutine());
    }

    private void Update() {
        // Is the player on a new tile ?
        if (tileManager.GetTileUnderPlayer() != currentTile) {
            currentTile = tileManager.GetTileUnderPlayer();
            uiManager.UpdateMiniMap();
            uiManager.DrawMap(discoveredTiles);
            if (currentTile.CompareTag("Exit")) {
                FileManager.SaveLevelDataFile(new LevelData(0, new List<Fragment>(), new List<string>()),
                    SceneManager.GetActiveScene().name);
                uiManager.ShowExitUi();
            }
        }

        CheckForTileDiscovery();

        // Toggle lamp
        if (Input.GetMouseButtonDown(0)) {
            lightAudio.clip = playerLamp.enabled ? lightSounds[1] : lightSounds[0];
            lightAudio.Play();
            playerLamp.enabled = !playerLamp.enabled;
        }

        if (player.GetComponent<Player>().fuelCount <= 0 && !isDead) {
            isDead = true;
            playerAudio.PlayOneShot(batteryDeadAudio);
            if (tryCount >= tryMax) playerAudio.PlayOneShot(youLostAudio);
        }

        // Useful for now, to remove later
        // if (Input.GetKeyUp("r")) GameDataManager.EraseFile(SceneManager.GetActiveScene().name);
        // if (Input.GetKeyUp("n")) NextLevel();
        if (Input.GetKeyUp("p") || Input.GetKeyUp(KeyCode.Escape)) uiManager.ShowPauseUi();
    }

    private void CheckForTileDiscovery() {
        bool needMapUpdate = false;
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, discoveryRange);
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

        if (needMapUpdate) uiManager.UpdateMiniMap();
    }

    public void Retry() {
        if (tryCount < tryMax) {
            Fragment currentFragment =
                CreateFragment(tileManager.GetTilesNames(revealedTilesInRun), currentTile.name, tryCount);
            mapFragments.Add(currentFragment);
            FileManager.SaveLevelDataFile(new LevelData(tryCount, mapFragments, discoveredTiles),
                SceneManager.GetActiveScene().name);
        }
        else {
            FileManager.DeleteFile(SceneManager.GetActiveScene().name);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GiveUp() {
        if (tryCount < tryMax) {
            Fragment currentFragment =
                CreateFragment(tileManager.GetTilesNames(revealedTilesInRun), currentTile.name, tryCount);
            mapFragments.Add(currentFragment);
            FileManager.SaveLevelDataFile(new LevelData(tryCount, mapFragments, discoveredTiles),
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

        int tilesCount = (int) Math.Round((double) floorTiles.Count);
        Debug.Log($"{tilesCount} tiles available");
        GameObject tile = floorTiles[floorTiles.Count - Random.Range(1, tilesCount)];
        Vector3 position = tile.transform.position;
        Transform playerTransform = Instantiate(playerPrefab, new Vector3(
            position.x,
            position.y + 0.53f,
            position.z
        ), Quaternion.identity);
        player = playerTransform.gameObject;
        startingTile = tile;
        currentTile = tile;
        Debug.Log(
            $"Instantiated player at {tile.GetComponent<Tile>().score} distance, max is {GetFurthestTile().GetComponent<Tile>().score}");
    }

    private void InstantiateFragment(Fragment fragmentIn) {
        GameObject spawnTile = GameObject.Find(fragmentIn.spawnTile);
        Vector3 position = spawnTile.transform.position;
        Transform fragment = Instantiate(fragmentPrefab, new Vector3(
            position.x,
            position.y + 0.35f,
            position.z
        ), Quaternion.identity);
        fragment.name = $"Fragment_{fragmentIn.number}";
        fragment.SetParent(fragments.transform);
    }

    public bool IsPreviousSpawnTile(GameObject tile) {
        return mapFragments.Any(fragment => fragment.spawnTile == tile.name);
    }

    public int GetSpawnTileTryNumber(GameObject tile) {
        foreach (Fragment fragment in mapFragments) {
            if (fragment.spawnTile == tile.name) return fragment.number;
        }

        return 0;
    }

    private Fragment CreateFragment(List<string> tiles, string spawnTile, int number) {
        return new Fragment(tiles, spawnTile, number);
    }

    public void PickupFragment(GameObject fragmentIn) {
        Destroy(fragmentIn);
        int fragmentNumber = int.Parse(fragmentIn.name.Split('_')[1]);
        Fragment fragment = mapFragments.Find(frag => frag.number == fragmentNumber);
        fragment.discovered = true;
        player.GetComponent<AudioSource>().PlayOneShot(fragmentPickupAudio);
        fragment.tiles.ForEach(tile => {
            if (discoveredTiles.Count == 0 || !discoveredTiles.Contains(tile)) {
                discoveredTiles.Add(tile);
            }
        });
        uiManager.DrawMap(discoveredTiles);
        uiManager.UpdateDiscoveryText(discoveredTiles.Count, tileManager.GetMapSize());
        uiManager.AddInfoMessage("Fragment picked up");
        // Randomly spawn arrow

        if (Random.Range(1, 100) <= playerData.spawnArrowChance) {
            uiManager.AddInfoMessage("Helping arrow spawned");
            fragment.arrowRevealed = true;
        }

        if (fragment.arrowRevealed) {
            InstantiateArrow(currentTile.transform, currentTile.GetComponent<Tile>().action);
        }
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

    public GameObject GetFurthestTile() {
        GameObject[] floorTiles = GameObject.FindGameObjectsWithTag("Floor");
        GameObject furthestTile = null;
        int maxDistance = 0;
        foreach (GameObject floorTile in floorTiles) {
            if (floorTile.GetComponent<Tile>().score > maxDistance) {
                maxDistance = floorTile.GetComponent<Tile>().score;
                furthestTile = floorTile;
            }
        }

        Debug.Log($"Furthest tile: {maxDistance}");
        return furthestTile;
    }
}