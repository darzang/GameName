using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
	public List<List<string>> mapFragments = new List<List<string>>();
	public List<string> spawnTilesString = new List<string>();
	public List<GameObject> spawnTiles = new List<GameObject>();
	public List<GameObject> revealedTiles;

	// Environment
	public GameObject ceiling;


	void Awake ()
	{
		ceiling.SetActive(true);
		InstantiatePlayer();
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
		// Is the player on a new tile ?
		if (tileManager.GetTileUnderPlayer() != currentTile)
		{
			currentTile = tileManager.GetTileUnderPlayer();
//			uiManager.UpdateMiniMap();
			GameObject recognizedTile = spawnTiles.Find(tile => tile.name == currentTile.name);
			if (recognizedTile)
			{
				int index = spawnTiles.IndexOf(recognizedTile);
				uiManager.ActivatePlayerThoughts();
				Debug.Log("Yes, it's from fragment # " + (index + 1));
				uiManager.MergeFragmentInMiniMap(mapFragments.ElementAt(index));
			}
		}
		// Toggle lamp
		if (Input.GetMouseButtonDown (0)) { // Left click
			lightAudio.clip = playerLamp.enabled ? lightSounds[1] : lightSounds[0];
			lightAudio.Play ();
			playerLamp.enabled = (!playerLamp.enabled);
		}
		// Useful for now, to remove later
		if(Input.GetKey("p")) GameDataManager.EraseFile();

		// Did the player discover a new tile ?
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

//		RaycastHit hit;
		RaycastHit[] hits;
		hits = Physics.RaycastAll(ray, 1);
		bool needMapUpdate = false;
		foreach (RaycastHit rayHit in hits)
		{
			if (!tileManager.HasBeenRevealed(rayHit.collider.gameObject, revealedTiles) && ! rayHit.collider.gameObject.CompareTag("Ceiling"))
			{
				needMapUpdate = true;
				Debug.Log("New tile revealed: " + rayHit.collider.gameObject.name + " with tag: " + rayHit.collider.gameObject.tag);
				tileManager.AddToRevealedTiles(rayHit.collider.gameObject, revealedTiles);
			}

		}
		//TODO: Check why the Vector's direction seems to not be linked to the player's rotation/angle
		//TODO: Maybe try to get the camera's forward as above and modify it 90° left and right should do the trick...
//		RaycastHit leftHit;
//		Ray leftRay = new Ray (player.transform.position, Vector3.left);
//		if (Physics.Raycast (ray, out hit, 10)) {
//			return hit.collider.gameObject;
//		}

		if(needMapUpdate) uiManager.UpdateMiniMap();
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
