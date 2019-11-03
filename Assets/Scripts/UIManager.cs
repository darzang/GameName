using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour {

	public TileManager tileManager;
	GameObject fuelBar;
	GameObject miniMap;
	GameObject player;

	Color32 floorColor = new Color32 (255, 255, 255, 255);
	Color32 wallColor = new Color32 (0, 0, 0, 255);
	Color32 obstacleColor = new Color32 (50, 50, 50, 255);
	// Use this for initialization
	double fuelTank;
	double fuelCount;
	void Awake () {
		fuelBar = GameObject.Find ("FuelBar");
		miniMap = GameObject.Find ("MiniMapPanel");
		player = GameObject.Find ("Player");
		fuelTank = player.GetComponent<Player> ().fuelTank;
		DrawWholeMap (tileManager.Get3DMap ());
	}

	// Update is called once per frame
	void Update () {
		UpdateHealthBar ();
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

	void AddTileToPanel (string tileTag, Vector3 position) {
		Color32 tileColor;
		switch (tileTag) {
			case "Wall":
				tileColor = wallColor;
				break;
			case "Floor":
				tileColor = floorColor;
				break;
			case "Obstacle":
				tileColor = obstacleColor;
				break;
			default:
				tileColor = floorColor;
				Debug.Log ("TAG_NOT_FOUND_FOR_TILE");
				break;
		}
		GameObject newTile = new GameObject (position.x + "_" + position.y + "_" + tileTag);
		Image newImage = newTile.AddComponent<Image> ();
		newTile.GetComponent<RectTransform> ().SetParent (miniMap.transform);
		newTile.GetComponent<RectTransform> ().anchorMin = new Vector2(0,0);
		newTile.GetComponent<RectTransform> ().anchorMax = new Vector2(0,0);
		newTile.GetComponent<RectTransform> ().anchoredPosition = position;
		Debug.Log (newTile.GetComponent<RectTransform> ().localPosition.ToString ());
		newTile.GetComponent<RectTransform> ().sizeDelta = new Vector2 (10, 10);
		newImage.color = tileColor;
		newTile.SetActive (true);
	}

	public void DrawWholeMap (GameObject[, ] map3D) {
		for (int i = 0; i < map3D.GetLength(0); i++) {
			for (int j = 0; j < map3D.GetLength(1); j++) {
				AddTileToPanel (
					map3D[i, j].tag,
					new Vector3 (
						map3D[i, j].transform.position.x*10 + 5,
						map3D[i, j].transform.position.z*10 + 5,
						0
					)
				);
			}
		}
	}
}