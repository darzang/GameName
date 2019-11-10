using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	public Transform spawnTile;
	public TileManager tileManager;

	public GameObject player;
	public GameObject playerLamp;

	private AudioSource lightAudio;
	public AudioClip[] lightSounds;
	public int tryCount;

	public List<List<string>> mapFragments = new List<List<string>>();

	void Awake () {
		player = GameObject.Find ("Player");
//		GameDataManager.EraseFile();
		lightAudio = playerLamp.GetComponent<AudioSource> ();
		GameData gameData = GameDataManager.LoadFile();
		if (gameData == null){
			Debug.Log("No data to load");
			tryCount = 1;
			mapFragments = new  List<List<string>>();
		}else{
			mapFragments = gameData.mapFragments;
	        foreach (List<string> fragment in mapFragments)
	        {
	            Debug.Log("New Fragment:");
	            foreach (string tileName in fragment)
	            {
		            Debug.Log(tileName);
	            }
            }
            Debug.Log("Data loaded");
			tryCount = gameData.tryCount  + 1;
		}

		Debug.Log("Try #" + tryCount);
		// InstantiateSpawnTile ();
	}

	void Update () {
		if (Input.GetMouseButtonDown (0)) { // Left click
			Light light = playerLamp.GetComponent<Light> ();
			lightAudio.clip = light.enabled ? lightSounds[1] : lightSounds[0];
			lightAudio.Play ();
			light.enabled = (!light.enabled);
		}
	}

	void InstantiateSpawnTile ()
	{
		GameObject floorTile = tileManager.GetTileUnderPlayer();
		Instantiate (spawnTile, new Vector3 (
					floorTile.transform.position.x,
					floorTile.transform.position.y + 0.001f,
					floorTile.transform.position.z
					),
			floorTile.transform.rotation,
			GameObject.Find("Environment").transform
		);
	}

	public void Retry()
	{

		mapFragments.Add(tileManager.getRevealedTilesNames());
		GameDataManager.SaveFile(new GameData(tileManager.revealedTiles, tryCount, mapFragments));
		SceneManager.LoadScene("GameScene");
	}

	public void GiveUp()
	{
		GameDataManager.EraseFile();
		//TODO: Load Menu
		Application.Quit(); // Doesn't work with Unity editor
	}

}
