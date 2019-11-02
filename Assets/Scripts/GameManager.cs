using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
	public Transform spawnTile;

	private GameObject player;
	private GameObject environment;
	void Start () {
		player = GameObject.Find ("Player");
		environment = GameObject.Find ("Environment");
		InstantiateSpawnTile ();
	}

	void Update () { }

	void InstantiateSpawnTile () {
		GameObject floorTile = GetTileUnderPlayer ();
		Instantiate (
			spawnTile,
			new Vector3 (
				floorTile.transform.position.x,
				floorTile.transform.position.y + 0.001f,
				floorTile.transform.position.z
				),
			floorTile.transform.rotation,
			environment.transform
		);

	}
	GameObject GetTileUnderPlayer () {
		RaycastHit hit;
		Ray ray = new Ray (player.transform.position, Vector3.down);
		if (Physics.Raycast (ray, out hit, 10)) {
			Debug.Log ("Player is above " + hit.collider.gameObject);
			return hit.collider.gameObject;
		} else {
			return null;
		}
	}
}