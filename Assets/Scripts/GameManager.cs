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
    public GameObject player;
    private Light playerLamp;
    private AudioSource lightAudio;
    public AudioClip[] lightSounds;

    // Tiles
    public GameObject startingTile;
    public GameObject currentTile;
    public int tryCount;
    public List<Fragment> mapFragments = new List<Fragment>();
    public List<string> spawnTilesString = new List<string>();
    public List<GameObject> spawnTiles = new List<GameObject>();
    public List<GameObject> revealedTiles;

    // Environment
    public GameObject ceiling;
    public float discoveryRange = 0.75f;

    void Awake() {
        ceiling.SetActive(true);
        InstantiatePlayer();
        playerLamp = player.GetComponentInChildren<Light>();
        lightAudio = playerLamp.GetComponent<AudioSource>();
        GameData gameData = GameDataManager.LoadFile(SceneManager.GetActiveScene().name);
        tryCount = 1;
        if (gameData != null) {
            mapFragments = gameData.mapFragments;
            spawnTilesString = gameData.spawnTiles;
            tryCount = gameData.tryCount + 1;
            foreach (string tileName in spawnTilesString) spawnTiles.Add(GameObject.Find(tileName));
        }

        spawnTilesString.Add(currentTile.gameObject.name); //TODO: maybe move this above?
    }

    void Start() {
        uiManager.DrawMapFragments(mapFragments);
    }

    void Update()
    {
        // Is the player on a new tile ?
        if (tileManager.GetTileUnderPlayer() != currentTile) {
            currentTile = tileManager.GetTileUnderPlayer();
            GameObject recognizedTile = spawnTiles.Find(tile => tile.name == currentTile.name);
            if (recognizedTile) {
                // Merge the fragment off the previous run and delete it
                int index = spawnTiles.IndexOf(recognizedTile);
                uiManager.ActivatePlayerThoughts();
//                uiManager.MergeFragmentInMiniMap(mapFragments.ElementAt(index));
//                mapFragments.RemoveAt(index);
                spawnTiles.RemoveAt(index);
                uiManager.DrawMapFragments(mapFragments);
            }
            if (currentTile.tag == "Exit") uiManager.ShowExitUI();
        }

        CheckForTileDiscovery();

        // Toggle lamp
        if (Input.GetMouseButtonDown(0)) {
            lightAudio.clip = playerLamp.enabled ? lightSounds[1] : lightSounds[0];
            lightAudio.Play();
            playerLamp.enabled = (!playerLamp.enabled);
        }

        // Useful for now, to remove later
        if (Input.GetKey("p")) GameDataManager.EraseFile(SceneManager.GetActiveScene().name);
        if (Input.GetKey("n")) NextLevel();
    }

    public void CheckForTileDiscovery() {
        bool needMapUpdate = false;
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, discoveryRange);
        foreach (Collider tile in hitColliders) {
            if (!tileManager.HasBeenRevealed(tile.gameObject, revealedTiles)
            && !tile.gameObject.CompareTag("Ceiling")
            && !tile.gameObject.CompareTag("Player")) {
                needMapUpdate = true;
                tileManager.AddToRevealedTiles(tile.gameObject, revealedTiles);
            }
        }
        if (needMapUpdate) uiManager.UpdateMiniMap();
    }

    public void Retry()
    {

        Fragment currentFragment = createFragment(tileManager.GetTilesNames(revealedTiles), startingTile.name, tryCount);
        mapFragments.Add(currentFragment);
        GameDataManager.SaveFile(new GameData(tryCount, mapFragments, spawnTilesString), SceneManager.GetActiveScene().name);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GiveUp() {
        Fragment currentFragment = createFragment(tileManager.GetTilesNames(revealedTiles), startingTile.name, tryCount);
        mapFragments.Add(currentFragment);
        GameDataManager.SaveFile(new GameData(tryCount, mapFragments, spawnTilesString), SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("MenuScene");
    }

    public void BackToMenu() {
        Debug.Log("BackToMenu clicked");
        SceneManager.LoadScene("MenuScene");
    }

    public void NextLevel() {
        string currentLevel = SceneManager.GetActiveScene().name;
        int levelNumber = Int32.Parse(currentLevel.Substring(currentLevel.Length-1));
        string sceneToLoad = $"Level{levelNumber + 1}";
        SceneManager.LoadScene(sceneToLoad);
        Application.Quit(); // Doesn't work with Unity editor
    }

    public void InstantiatePlayer() {
        List<GameObject> availableTiles = tileManager.GetTilesByType("Floor");
        GameObject spawnTileObject = availableTiles.ElementAt(Random.Range(0, availableTiles.Count - 1));
        Transform playerTransform = Instantiate(playerPrefab, new Vector3(
            spawnTileObject.transform.position.x,
            spawnTileObject.transform.position.y + 0.53f,
            spawnTileObject.transform.position.z
        ), Quaternion.identity);
        player = playerTransform.gameObject;
        startingTile = spawnTileObject;
        currentTile = spawnTileObject;
    }

    public bool isPreviousSpawnTile(GameObject tile) {
        return tile.tag == "Floor" && spawnTiles.Find(spawnTile => spawnTile.name == tile.name);
    }

    public int getSpawnTileTryNumber(GameObject tile) {
        return spawnTiles.IndexOf(tile) + 1;
    }

    public Fragment createFragment(List<string> tiles, string spawnTile, int number) {
        return new Fragment(tiles, spawnTile, number);
    }
}
