using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour {
	public Transform spawnTile;
	public Transform playerPrefab;
	public TileManager tileManager;
	public UIManager uiManager;

	public GameObject player;
	private Light playerLamp;
	public GameObject startingTile;
	public GameObject currentTile;

	private AudioSource lightAudio;
	public AudioClip[] lightSounds;
	public int tryCount;

	public List<List<string>> mapFragments = new List<List<string>>();
	[FormerlySerializedAs("spawnTiles")] public List<string> spawnTilesString = new List<string>();
	public List<GameObject> spawnTiles = new List<GameObject>();

	void Awake () {
		InstantiatePlayer();
//		playerLamp = GameObject.Find("playerLamp");
		playerLamp = player.GetComponentInChildren<Light>();
		Debug.Log(playerLamp);
		lightAudio = playerLamp.GetComponent<AudioSource> ();
	}

	void Start()
	{

        GameData gameData = GameDataManager.LoadFile();
        tryCount = 1;
        if (gameData == null){
        	Debug.Log("No data to load");
        	mapFragments = new  List<List<string>>();
        }else{
        	Debug.Log("Data loaded, " + gameData.mapFragments.Count + " fragments collected");
        	mapFragments = gameData.mapFragments;
            spawnTilesString = gameData.spawnTiles;
        	tryCount = gameData.tryCount  + 1;
        	uiManager.DrawMapFragments(mapFragments);
            foreach (string tileName in spawnTilesString)
            {
	            spawnTiles.Add(GameObject.Find(tileName));
            }
        }

        spawnTilesString.Add(currentTile.gameObject.name);
        Debug.Log("Try #" + tryCount);
	}

	void Update ()
	{
		if (tileManager.GetTileUnderPlayer() != currentTile.gameObject)
		{
			currentTile = tileManager.GetTileUnderPlayer();
			uiManager.UpdateMiniMap();
			GameObject recognizedTile = spawnTiles.Find(tile => tile.name == currentTile.name);
			if (recognizedTile)
			{
				Debug.Log("Hey, I've been here already");
				int index = spawnTiles.IndexOf(recognizedTile);
				Debug.Log("Yes, it's from fragment # " + (index + 1));
			}
		}
		if (Input.GetMouseButtonDown (0)) { // Left click
//			Light light = playerLamp.GetComponent<Light> ();
			lightAudio.clip = playerLamp.enabled ? lightSounds[1] : lightSounds[0];
			lightAudio.Play ();
			playerLamp.enabled = (!playerLamp.enabled);
		}
		if(Input.GetKey("p")) GameDataManager.EraseFile();
	}

//	void InstantiateSpawnTile ()
//	{
//		GameObject floorTile = tileManager.GetTileUnderPlayer();
//		Instantiate (spawnTile, new Vector3 (
//					floorTile.transform.position.x,
//					floorTile.transform.position.y + 0.001f,
//					floorTile.transform.position.z
//					),
//			floorTile.transform.rotation,
//			GameObject.Find("Environment").transform
//		);
//	}

	public void Retry()
	{
		mapFragments.Add(tileManager.getRevealedTilesNames());
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
		GameObject spawnTileObject = availableTiles.ElementAt(Random.Range(0, availableTiles.Count-1));
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

}
