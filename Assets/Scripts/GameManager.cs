using System;
using System.Collections.Generic;
using System.Linq;
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

    public GameObject arrows;
    public GameObject fragments;
    // Tiles
    public GameObject startingTile;
    public GameObject currentTile;
    public bool exitRevealed;
    public int tryCount;
    public List<Fragment> mapFragments = new List<Fragment>();
    public List<GameObject> revealedTiles;
    public List<string> discoveredTiles = new List<string>();

    // Environment
    public GameObject ceiling;
    public float discoveryRange = 0.75f;

    private void Awake() {
        tryCount = 1;
        ceiling.SetActive(true);
        InstantiatePlayer();
        playerLamp = player.GetComponentInChildren<Light>();
        lightAudio = playerLamp.GetComponent<AudioSource>();
        tileManager.DoPathPlanning();
        GameData gameData = GameDataManager.LoadFile(SceneManager.GetActiveScene().name);
        if (gameData == null) {
            Debug.Log("No Data to load");
        } else {
            exitRevealed = gameData.exitRevealed;
            mapFragments = gameData.mapFragments;
            tryCount = gameData.tryCount + 1;
            discoveredTiles = gameData.totalDiscoveredTiles;
            foreach (Fragment fragment in mapFragments.Where(fragment => fragment.discovered == false)) {
                InstantiateFragment(fragment);
            }
            uiManager.DrawMap(discoveredTiles);
            uiManager.UpdateDiscoveryText(discoveredTiles.Count,tileManager.GetMapSize());
            Debug.Log($"Data loaded: exitRevealed: {gameData.exitRevealed} | try {tryCount} \n mapFragments {mapFragments.Count} \n discoveredTiles: {discoveredTiles.Count}");
            if(discoveredTiles.Count > 0) uiManager.AddInfoMessage("Previous data loaded");
        }

        
        // StartCoroutine(tileManager.DoPathPlanningCoroutine());
    }
    private void Update()
    {
        // Is the player on a new tile ?
        if (tileManager.GetTileUnderPlayer() != currentTile) {
            uiManager.UpdateMiniMap();
            currentTile = tileManager.GetTileUnderPlayer();
            if (currentTile.CompareTag("Exit")) uiManager.ShowExitUi();
        }

        CheckForTileDiscovery();

        // Toggle lamp
        if (Input.GetMouseButtonDown(0)) {
            lightAudio.clip = playerLamp.enabled ? lightSounds[1] : lightSounds[0];
            lightAudio.Play();
            playerLamp.enabled = !playerLamp.enabled;
        }

        // Useful for now, to remove later
        if (Input.GetKeyUp("p")) GameDataManager.EraseFile(SceneManager.GetActiveScene().name);
        if (Input.GetKeyUp("n")) NextLevel();
        if (Input.GetKeyUp("t")) uiManager.AddInfoMessage("Heyyyyyyyy");
    }

    private void CheckForTileDiscovery() {
        bool needMapUpdate = false;
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, discoveryRange);
        foreach (Collider tile in hitColliders) {
            if (!tileManager.HasBeenRevealed(tile.gameObject, revealedTiles)
            && (tile.gameObject.CompareTag("Floor")
            || tile.gameObject.CompareTag("Obstacle")
            || tile.gameObject.CompareTag("Exit")
            || tile.gameObject.CompareTag("Wall"))) {
                needMapUpdate = true;
                tileManager.AddToRevealedTiles(tile.gameObject, revealedTiles);
            }
        }
        if (needMapUpdate) uiManager.UpdateMiniMap();
    }

    public void Retry() {
        Fragment currentFragment = CreateFragment(tileManager.GetTilesNames(revealedTiles), currentTile.name, tryCount);
        mapFragments.Add(currentFragment);
        GameDataManager.SaveFile(new GameData(tryCount, mapFragments, discoveredTiles, exitRevealed), SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GiveUp() {
        Fragment currentFragment = CreateFragment(tileManager.GetTilesNames(revealedTiles), currentTile.name, tryCount);
        mapFragments.Add(currentFragment);
        GameDataManager.SaveFile(new GameData(tryCount, mapFragments, discoveredTiles, exitRevealed), SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("MenuScene");
    }

    public void BackToMenu() {
        SceneManager.LoadScene("MenuScene");
    }

    public void NextLevel() {
        string currentLevel = SceneManager.GetActiveScene().name;
        int levelNumber = int.Parse(currentLevel.Substring(currentLevel.Length-1));
        string sceneToLoad = $"Level{levelNumber + 1}";
        SceneManager.LoadScene(sceneToLoad);
    }

    private void InstantiatePlayer() {
        List<GameObject> availableTiles = tileManager.GetTilesByType("Floor");
        GameObject spawnTileObject = availableTiles.ElementAt(Random.Range(0, availableTiles.Count - 1));
        Vector3 position = spawnTileObject.transform.position;
        Transform playerTransform = Instantiate(playerPrefab, new Vector3(
            position.x,
            position.y + 0.53f,
            position.z
        ), Quaternion.identity);
        player = playerTransform.gameObject;
        startingTile = spawnTileObject;
        currentTile = spawnTileObject;
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
        int fragmentNumber = int.Parse(fragmentIn.name.Split('_')[1]);
        Fragment fragment = mapFragments.Find(frag => frag.number == fragmentNumber);
        fragment.discovered = true;
        fragment.tiles.ForEach(tile => {
            if (discoveredTiles.Count == 0 || !discoveredTiles.Contains(tile)) {
                discoveredTiles.Add(tile);
                if (!exitRevealed && GameObject.Find(tile).CompareTag("Exit")) {
                    exitRevealed = true;
                }
            }
        });
        uiManager.DrawMap(discoveredTiles);
        uiManager.UpdateDiscoveryText(discoveredTiles.Count, tileManager.GetMapSize());
        uiManager.AddInfoMessage("Fragment picked up");
        if (exitRevealed) {
            InstantiateArrow(currentTile.transform, currentTile.GetComponent<Tile>().action);
        }

        Destroy(fragmentIn);
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
        arrow.transform.eulerAngles = new Vector3(0,angle,0);
        arrow.SetParent(arrows.transform);
    }
}
