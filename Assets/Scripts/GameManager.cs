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

	void Awake () {
		player = GameObject.Find ("Player");
		lightAudio = playerLamp.GetComponent<AudioSource> ();
		string gameData = GameDataSaver.LoadFile();
		if (gameData == null)
		{
			Debug.Log("No data to load");
		}
		else
		{
			Debug.Log("Loaded data: " + gameData);
		}
		// InstantiateSpawnTile ();
	}

	void Update () {
		if (Input.GetMouseButtonDown (0)) { // Left click
			Light light = playerLamp.GetComponent<Light> ();
			lightAudio.clip = light.enabled ? lightSounds[1] : lightSounds[0];
			Debug.Log("Playing audio");
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
		GameDataSaver.SaveFile(new GameData(tileManager.revealedTiles));
		SceneManager.LoadScene("GameScene");
	}

	public void GiveUp()
	{
		Application.Quit(); // This quit the game entirely and therefore doesn't work while testing in the Unity editor
	}

}
