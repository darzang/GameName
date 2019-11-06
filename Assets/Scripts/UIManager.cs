using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour {

	public TileManager tileManager;
	GameObject fuelBar;
	GameObject miniMapPanel;
	GameObject player;
	Color32 floorColor = new Color32 (255, 255, 255, 255);
	Color32 wallColor = new Color32 (25, 25, 25, 255);
	Color32 obstacleColor = new Color32 (50, 50, 50, 255);
	Color32 playerColor = new Color32 (0, 0, 255, 255);
	// Use this for initialization
	double fuelTank;
	double fuelCount;
	GameObject currentTile;
	void Awake () {
		// StartCoroutine (WaitForXSeconds (5));
		Debug.Log ("Starting Awake UI");
		fuelBar = GameObject.Find ("FuelBar");
		miniMapPanel = GameObject.Find ("MiniMapPanel");
		player = GameObject.Find ("Player");
		fuelTank = player.GetComponent<Player> ().fuelTank;
		currentTile = tileManager.GetTileUnderPlayer ();
		DrawStartingMiniMap ();
	}
	IEnumerator WaitForXSeconds (int x) {
		Debug.Log (Time.time);
		yield return new WaitForSeconds (x);
		Debug.Log (Time.time);
	}
	// Update is called once per frame
	void Update () {
		// UpdateHealthBar ();
		// UpdateMiniMap ();
		if (tileManager.GetTileUnderPlayer () != currentTile) {
			currentTile = tileManager.GetTileUnderPlayer ();
			// AddTileToMiniMap (currentTile);
			UpdateMiniMap ();
		}

	}

	void UpdateMiniMap () {
		List<GameObject> neighborsTiles = tileManager.GetNeighborsTiles ((int) currentTile.transform.position.x, (int) currentTile.transform.position.z);
		List<GameObject> tilesToDraw = new List<GameObject> ();
		foreach (GameObject neighbor in neighborsTiles) {
			if (!tileManager.HasBeenRevealed (neighbor)) {
				tilesToDraw.Add (neighbor);
			}
		}
		foreach (GameObject revealedTile in tileManager.revealedTiles) {
			tilesToDraw.Add (revealedTile);
		}
		foreach (GameObject tile in tilesToDraw) {
			AddTileToMiniMap (tile);
		}
	}
	void UpdateHealthBar () {
		fuelCount = player.GetComponent<Player> ().fuelCount;
		// Update scale
		fuelBar.transform.localScale = new Vector3 (
			(float) (fuelCount / fuelTank),
			fuelBar.transform.localScale.y,
			fuelBar.transform.localScale.z
		);

		// Update color
		if (fuelCount > fuelTank / 2) {
			fuelBar.GetComponent<Image> ().color = new Color32 ((byte) (fuelTank - fuelCount), 255, 0, 255);
		} else {
			fuelBar.GetComponent<Image> ().color = new Color32 (255, (byte) fuelCount, 0, 255);
		}

	}

	void DrawStartingMiniMap () {
		GameObject currentTile = tileManager.GetTileUnderPlayer ();
		AddTileToMiniMap (currentTile);
		Debug.Log ("DrawStartMiniMap from " + currentTile.transform.position.x + " | " + currentTile.transform.position.z);
		List<GameObject> neighborTiles = tileManager.GetNeighborsTiles ((int) currentTile.transform.position.x, (int) currentTile.transform.position.z);

		foreach (GameObject tile in neighborTiles) {
			AddTileToMiniMap (tile);
		}
	}
	void AddTileToMiniMap (GameObject tile) {

		if (tileManager.HasBeenRevealed (tile)) Destroy (GameObject.Find (tile.transform.position.x + "_" + tile.transform.position.z + "_" + tile.tag));
		Color32 tileColor = GetTileColor (tile.tag);
		GameObject newTile = new GameObject (tile.transform.position.x + "_" + tile.transform.position.z + "_" + tile.tag);
		Image newImage = newTile.AddComponent<Image> ();
		newTile.GetComponent<RectTransform> ().SetParent (miniMapPanel.transform);
		newTile.GetComponent<RectTransform> ().anchorMin = new Vector2 (0.5f, 0.5f);
		newTile.GetComponent<RectTransform> ().anchorMax = new Vector2 (0.5f, 0.5f);
		if (tile == tileManager.GetTileUnderPlayer ()) {
			newTile.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (0, 0, 0);
			tileColor = playerColor;
		} else {
			newTile.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (
				tileManager.GetRelativePosition (tileManager.GetTileUnderPlayer (), tile) [0] * 10,
				tileManager.GetRelativePosition (tileManager.GetTileUnderPlayer (), tile) [1] * 10,
				0);
		}

		newTile.GetComponent<RectTransform> ().sizeDelta = new Vector2 (10, 10);
		newTile.GetComponent<RectTransform> ().localScale = new Vector3 (1.0f, 1.0f, 1.0f);
		newImage.color = tileColor;
		newTile.SetActive (true);
		if (!tileManager.HasBeenRevealed (tile)) {
			tileManager.AddToRevealedTiles (tile);
		}
	}

	void AddTileToMap (GameObject tile, bool drawInMiddle) {
		/*
		Outdated
		 */
		Color32 tileColor = GetTileColor (tile.tag);
		GameObject newTile = new GameObject (tile.transform.position.x + "_" + tile.transform.position.z + "_" + tile.tag);
		Image newImage = newTile.AddComponent<Image> ();
		newTile.GetComponent<RectTransform> ().SetParent (miniMapPanel.transform);

		if (drawInMiddle) {
			newTile.GetComponent<RectTransform> ().anchorMin = new Vector2 (0.5f, 0.5f);
			newTile.GetComponent<RectTransform> ().anchorMax = new Vector2 (0.5f, 0.5f);
			newTile.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (0, 0, 0);
		} else {
			newTile.GetComponent<RectTransform> ().anchorMin = new Vector2 (0, 0);
			newTile.GetComponent<RectTransform> ().anchorMax = new Vector2 (0, 0);
			newTile.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (
				tile.transform.position.x * 10 + 5,
				tile.transform.position.z * 10 + 5,
				0
			);
		}
		newTile.GetComponent<RectTransform> ().sizeDelta = new Vector2 (10, 10);
		newImage.color = tileColor;
		newTile.SetActive (true);
		/*
		Outdated
		 */
	}

	Color32 GetTileColor (string tileTag) {
		switch (tileTag) {
			case "Wall":
				return wallColor;
			case "Floor":
				return floorColor;
			case "Obstacle":
				return obstacleColor;
			case "Player":
				return playerColor;
			default:
				Debug.Log ("TAG_NOT_FOUND_FOR_TILE");
				return floorColor;
		}
	}

	public void DrawWholeMap (GameObject[, ] map3D) {
		for (int i = 0; i < map3D.GetLength (0); i++) {
			for (int j = 0; j < map3D.GetLength (1); j++) {
				AddTileToMap (
					map3D[i, j],
					false
				);
			}
		}
	}

}