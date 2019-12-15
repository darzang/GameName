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

    // Environment
    public GameObject ceiling;


    void Awake()
    {
        ceiling.SetActive(true);
        InstantiatePlayer();
        playerLamp = player.GetComponentInChildren<Light>();
        lightAudio = playerLamp.GetComponent<AudioSource>();
        GameData gameData = GameDataManager.LoadFile();
        tryCount = 1;
        if (gameData != null)
        {
            mapFragments = gameData.mapFragments;
            spawnTilesString = gameData.spawnTiles;
            tryCount = gameData.tryCount + 1;
            foreach (string tileName in spawnTilesString)
            {
                spawnTiles.Add(GameObject.Find(tileName));
            }
        }

        spawnTilesString.Add(currentTile.gameObject.name); //TODO: maybe move this above?
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
            uiManager.UpdateMiniMap();
            currentTile = tileManager.GetTileUnderPlayer();
            GameObject recognizedTile = spawnTiles.Find(tile => tile.name == currentTile.name);
            if (recognizedTile)
            {
                int index = spawnTiles.IndexOf(recognizedTile);
                uiManager.ActivatePlayerThoughts();
                uiManager.MergeFragmentInMiniMap(mapFragments.ElementAt(index));
                uiManager.UpdateMiniMap();
                Debug.Log("Yes, it's from fragment # " + (index + 1));
            }

            if (currentTile.tag == "Exit")
            {
                Debug.Log("You reached the exit !!");
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
        RaycastHit[] hits = Physics.RaycastAll(forwardRay, playerLamp.range / 2);
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
        SceneManager.LoadScene("Level1");
    }

    public void GiveUp()
    {
        GameDataManager.EraseFile();
        //TODO: Load Menu
        Application.Quit(); // Doesn't work with Unity editor
    }

    public void InstantiatePlayer()
    {
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

    public bool isPreviousSpawnTile(GameObject tile)
    {
        return tile.tag == "Floor" && spawnTiles.Find(spawnTile => spawnTile.name == tile.name);
    }

    public int getSpawnTileTryNumber(GameObject tile)
    {
        return spawnTiles.IndexOf(tile) + 1;
    }
}
