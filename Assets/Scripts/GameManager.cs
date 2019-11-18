using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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
    public List<List<string>> mapFragments = new List<List<string>>();
    public List<string> spawnTilesString = new List<string>();
    public List<GameObject> spawnTiles = new List<GameObject>();
    public List<GameObject> revealedTiles;
    public List<GameObject> leftTiles;

    // Environment
    public GameObject ceiling;


    void Awake()
    {
//		ceiling.SetActive(true);
        InstantiatePlayer();
        playerLamp = player.GetComponentInChildren<Light>();
        Debug.Log(playerLamp);
        lightAudio = playerLamp.GetComponent<AudioSource>();
        GameData gameData = GameDataManager.LoadFile();
        tryCount = 1;
        if (gameData == null)
        {
            Debug.Log("No data to load");
            mapFragments = new List<List<string>>();
        }
        else
        {
            Debug.Log("Data loaded, " + gameData.mapFragments.Count + " fragments collected");
            mapFragments = gameData.mapFragments;
            spawnTilesString = gameData.spawnTiles;
            tryCount = gameData.tryCount + 1;
            Debug.Log("spawnTilesString length: " + spawnTilesString.Count);
            foreach (string tileName in spawnTilesString)
            {
                spawnTiles.Add(GameObject.Find(tileName));
//                Debug.Log("Spawn tile " + GameObject.Find(tileName).name + " correctly added to spawnTiles");
            }
        }

        spawnTilesString.Add(currentTile.gameObject.name);
        Debug.Log("Try #" + tryCount);
    }

    void Start()
    {
        uiManager.DrawMapFragments(mapFragments);

    }

    void Update()
    {
        // Is the player on a new tile ?
        if (tileManager.GetTileUnderPlayer() != currentTile)
        {
            currentTile = tileManager.GetTileUnderPlayer();
            GameObject recognizedTile = spawnTiles.Find(tile => tile.name == currentTile.name);
            if (recognizedTile)
            {
                int index = spawnTiles.IndexOf(recognizedTile);
                uiManager.ActivatePlayerThoughts();
                uiManager.MergeFragmentInMiniMap(mapFragments.ElementAt(index));
                Debug.Log("Yes, it's from fragment # " + (index + 1));
            }
        }

        CheckForTileDiscovery();

        // Toggle lamp
        if (Input.GetMouseButtonDown(0))
        {
            lightAudio.clip = playerLamp.enabled ? lightSounds[1] : lightSounds[0];
            lightAudio.Play();
            playerLamp.enabled = (!playerLamp.enabled);
        }

        // Useful for now, to remove later
        if (Input.GetKey("p")) GameDataManager.EraseFile();
    }

    public void CheckForTileDiscovery()
    {
        // Check forward
        Ray forwardRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(forwardRay, 1);
        bool needMapUpdate = false;
        foreach (RaycastHit rayHit in hits)
        {
            //TODO: Filter out tiles that should not be discovered (ex floor behind corner of walls)
            if (!tileManager.HasBeenRevealed(rayHit.collider.gameObject, revealedTiles) &&
                !rayHit.collider.gameObject.CompareTag("Ceiling"))
            {
                needMapUpdate = true;
//				Debug.Log("New tile discovered forward: " + rayHit.collider.gameObject.name);
                tileManager.AddToRevealedTiles(rayHit.collider.gameObject, revealedTiles);
            }
        }

        // Check on the left
        RaycastHit leftHit;
        Ray leftRay = new Ray(player.transform.position, forwardRay.direction + Vector3.left);
        if (Physics.Raycast(leftRay, out leftHit, 1))
        {
            if (!tileManager.HasBeenRevealed(leftHit.collider.gameObject, revealedTiles) &&
                !leftHit.collider.gameObject.CompareTag("Ceiling"))
            {
                needMapUpdate = true;
//				Debug.Log("New tile discovered on the left: " + leftHit.collider.gameObject.name);
                tileManager.AddToRevealedTiles(leftHit.collider.gameObject, revealedTiles);
            }
        }

        // Check on the right
        RaycastHit rightHit;
        Ray rightRay = new Ray(player.transform.position, forwardRay.direction + Vector3.right);
        if (Physics.Raycast(rightRay, out rightHit, 1))
        {
            if (!tileManager.HasBeenRevealed(rightHit.collider.gameObject, revealedTiles) &&
                !rightHit.collider.gameObject.CompareTag("Ceiling"))
            {
                needMapUpdate = true;
//				Debug.Log("New tile discovered on the right: " + rightHit.collider.gameObject.name);
                tileManager.AddToRevealedTiles(rightHit.collider.gameObject, revealedTiles);
            }
        }

        if (needMapUpdate) uiManager.UpdateMiniMap();
    }

    public void Retry()
    {
        mapFragments.Add(tileManager.GetTilesNames(revealedTiles));
        GameDataManager.SaveFile(new GameData(tryCount, mapFragments, spawnTilesString));
        SceneManager.LoadScene("GameScene");
    }

    public void GiveUp()
    {
        GameDataManager.EraseFile();
        //TODO: Load Menu
        Application.Quit(); // Doesn't work with Unity editor
    }

    public void InstantiatePlayer()
    {
        // Get available tiles
        List<GameObject> availableTiles = tileManager.GetTilesByType("Floor");
        // Get one at random
        GameObject spawnTileObject = availableTiles.ElementAt(Random.Range(0, availableTiles.Count - 1));
        // Instantiate player
        Transform playerTransform = Instantiate(playerPrefab, new Vector3(
            spawnTileObject.transform.position.x,
            spawnTileObject.transform.position.y + 0.53f,
            spawnTileObject.transform.position.z
        ), Quaternion.identity);
        player = playerTransform.gameObject;
        // Set StartingTile in gameManager
        startingTile = spawnTileObject;
        currentTile = spawnTileObject;
    }

    public bool isPreviousSpawnTile(GameObject tile)
    {
        if (tile.tag == "Floor" && spawnTiles.Find(spawnTile => spawnTile.name == tile.name))
        {
//            Debug.Log("Tile " + tile.name + " is a previous spawn tile");
            return true;
        }
//        Debug.Log("Tile " + tile.name + " is NOT a previous spawn tile");
        return false;
    }

    public int getSpawnTileTryNumber(GameObject tile)
    {
        return spawnTiles.IndexOf(tile) + 1;
    }
}
