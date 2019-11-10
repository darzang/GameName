using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour {

	public TileManager tileManager;
	public GameManager gameManager;
	public GameObject batteryDead;
	public GameObject batteryLevel;
	public Button retryButton;
    public Button giveUpButton;
    public GameObject fuelBar;
	public GameObject miniMapPanel;
	public GameObject mapFragmentsPanel;
	public GameObject buttonPanel;
	public GameObject player;
	public GameObject fragmentPanelPrefab;
	Color32 floorColor = new Color32 (255, 255, 255, 255);
	Color32 wallColor = new Color32 (25, 25, 25, 255);
	Color32 obstacleColor = new Color32 (50, 50, 50, 255);
	Color32 playerColor = new Color32 (0, 0, 255, 255);
	Color32 spawnTextColor = new Color32(0,0,0,255);
	double fuelTank;
	double fuelCount;
	GameObject currentTile;
	private bool batteryLevelBlinking = false;


	void Awake ()
	{
		Debug.Log("Awake UI");
		fuelTank = player.GetComponent<Player> ().fuelTank;
		currentTile = tileManager.GetTileUnderPlayer ();
		retryButton.onClick.AddListener(gameManager.Retry);
		giveUpButton.onClick.AddListener(gameManager.GiveUp);
	}

	void Start()
	{
		DrawStartingMiniMap ();
	}
	void Update () {
		fuelCount = player.GetComponent<Player> ().fuelCount;
		if (tileManager.GetTileUnderPlayer () != currentTile) {
			currentTile = tileManager.GetTileUnderPlayer ();
			UpdateMiniMap ();
		}
		if (player.GetComponent<Player>().fuelCount > 0){
			UpdateHealthBar();
		}
		else
		{
			if (!batteryDead.gameObject.activeSelf) batteryDead.SetActive(true);
			StopCoroutine("BlinkBatteryLevel");
			buttonPanel.SetActive(true);
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
		if (fuelCount <= fuelTank * 0.5 ){
			if (!batteryLevelBlinking) StartCoroutine(BlinkBatteryLevel());
		}
		RotateMiniMap();
	}

	void RotateMiniMap()
	{
		float angle  = player.transform.eulerAngles.y+180;
		miniMapPanel.transform.rotation = Quaternion.Euler(0, 0, angle);
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
		Debug.Log(gameManager.startingTile);
		GameObject currentTile = tileManager.GetTileUnderPlayer ();
		AddTileToMiniMap (currentTile);
		List<GameObject> neighborTiles = tileManager.GetNeighborsTiles (
			(int) currentTile.transform.position.x,
			(int) currentTile.transform.position.z
			);

		foreach (GameObject tile in neighborTiles) {
			AddTileToMiniMap (tile);
		}
	}
	void AddTileToMiniMap (GameObject tile) {

		// Regenerate previously drawn tiles
		if (tileManager.HasBeenRevealed(tile)){
			Destroy (GameObject.Find (tile.transform.position.x + "_" + tile.transform.position.z + "_" + tile.tag));
		}
		if (!tileManager.HasBeenRevealed (tile)) {
			tileManager.AddToRevealedTiles (tile);
		}
		// Instantiate new tile and anchor it in the middle of the panel
		GameObject newTile = new GameObject (tile.transform.position.x + "_" + tile.transform.position.z + "_" + tile.tag);
		Image newImage = newTile.AddComponent<Image> ();
		Color32 tileColor = GetTileColor (tile.tag);

		newTile.GetComponent<RectTransform> ().SetParent (miniMapPanel.transform);
		newTile.GetComponent<RectTransform> ().anchorMin = new Vector2 (0.5f, 0.5f);
		newTile.GetComponent<RectTransform> ().anchorMax = new Vector2 (0.5f, 0.5f);

		// Set the position of the new tile
		if (tile == tileManager.GetTileUnderPlayer ()) {
			newTile.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (0, 0, 0);
			tileColor = playerColor;
		} else {
			newTile.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (
				tileManager.GetRelativePosition (tileManager.GetTileUnderPlayer (), tile) [0] * 10,
				tileManager.GetRelativePosition (tileManager.GetTileUnderPlayer (), tile) [1] * 10,
				0);
		}

		// Set the size and scale of the tile
		newTile.GetComponent<RectTransform> ().sizeDelta = new Vector2 (10, 10);
		newTile.GetComponent<RectTransform> ().localScale = new Vector3 (1.0f, 1.0f, 1.0f);
		newImage.color = tileColor;

		if (tile == gameManager.startingTile)
		{
			DrawSpawnTile(newTile);
		}
		newTile.SetActive (true);
	}

	void DrawSpawnTile(GameObject tile)
	{

			GameObject spawnTile = new GameObject("SpawnTile");
			spawnTile.transform.parent = tile.transform;
			spawnTile.AddComponent<TextMeshProUGUI>();
			TextMeshProUGUI spawnText = spawnTile.GetComponent<TextMeshProUGUI>();
			spawnText.text = "S"+gameManager.tryCount;
			spawnText.GetComponent<RectTransform>().pivot = new Vector2(0.5f,0.5f);
			spawnText.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f,0.5f);
			spawnText.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f,0.5f);
			spawnText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,0,0);
			spawnText.GetComponent<RectTransform>().sizeDelta = tile.GetComponent<RectTransform>().sizeDelta;
			spawnText.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
			spawnText.fontSize = 8 * tile.GetComponent<RectTransform>().sizeDelta.x / 10;
			spawnText.color = spawnTextColor;
			spawnText.alignment = TextAlignmentOptions.MidlineJustified;

	}
	void AddTileToMap (GameObject tile) {
		/*
		Was used when drawing the whole map at once,
		Basically only the anchor is different
		 */
		Color32 tileColor = GetTileColor (tile.tag);
		GameObject newTile = new GameObject (tile.transform.position.x + "_" + tile.transform.position.z + "_" + tile.tag);
		Image newImage = newTile.AddComponent<Image> ();
		newTile.GetComponent<RectTransform> ().SetParent (miniMapPanel.transform);

		newTile.GetComponent<RectTransform> ().anchorMin = new Vector2 (0, 0);
		newTile.GetComponent<RectTransform> ().anchorMax = new Vector2 (0, 0);
		newTile.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (
			tile.transform.position.x * 10 + 5,
			tile.transform.position.z * 10 + 5,
			0
			);
		newTile.GetComponent<RectTransform> ().sizeDelta = new Vector2 (10, 10);
		newImage.color = tileColor;
		newTile.SetActive (true);
	}
	public void DrawWholeMap (GameObject[, ] map3D) {
		for (int i = 0; i < map3D.GetLength (0); i++) {
			for (int j = 0; j < map3D.GetLength (1); j++) {
				AddTileToMap (map3D[i, j]);
			}
		}
	}
	Color32 GetTileColor (string Tag) {
		switch (Tag) {
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

	IEnumerator BlinkBatteryLevel()
	{
		batteryLevelBlinking = true;
		TextMeshProUGUI batteryText = batteryLevel.GetComponent<TextMeshProUGUI>();
		while (true)
		{
			if (fuelCount > fuelTank * 0.2){
				batteryText.text = "BATTERY LEVEL LOW";
				batteryLevel.SetActive(true);
				yield return new WaitForSeconds(0.4f);
				batteryLevel.SetActive(false);
				yield return new WaitForSeconds(0.4f);
			}else{
				batteryText.text = "BATTERY LEVEL CRITICAL";
				batteryLevel.SetActive(true);
				yield return new WaitForSeconds(0.2f);
				batteryLevel.SetActive(false);
				yield return new WaitForSeconds(0.2f);
			}
		}
	}

	public void DrawMapFragments(List<List<string>> mapFragments)
	{
		int fragmentNumber = 1;
		foreach (List<string> fragment in mapFragments)
		{
			DrawMapFragment(fragment, fragmentNumber);
			fragmentNumber++;
		}
	}
	public void DrawMapFragment(List<string> fragment, int fragmentNumber)
	{
		// Instantiate fragment panel
		GameObject fragmentPanel = Instantiate (fragmentPanelPrefab, new Vector3 (0,0,0),mapFragmentsPanel.transform.rotation,mapFragmentsPanel.transform);

//		GameObject fragmentPanel = new GameObject ("Fragment_"+fragmentNumber+"_Panel");
		fragmentPanel.GetComponent<RectTransform> ().SetParent (mapFragmentsPanel.transform);
		switch (fragmentNumber)
		{
			case 1:
				fragmentPanel.GetComponent<RectTransform> ().anchorMin = new Vector2 (0, 1);
				fragmentPanel.GetComponent<RectTransform> ().anchorMax = new Vector2 (0, 1);
				fragmentPanel.GetComponent<RectTransform> ().pivot = new Vector2 (0, 1);
				break;
			case 2:
				fragmentPanel.GetComponent<RectTransform> ().anchorMin = new Vector2 (1, 1);
				fragmentPanel.GetComponent<RectTransform> ().anchorMax = new Vector2 (1, 1);
				fragmentPanel.GetComponent<RectTransform> ().pivot = new Vector2 (1, 1);
				break;
			case 3:
				fragmentPanel.GetComponent<RectTransform> ().anchorMin = new Vector2 (0, 0.5f);
				fragmentPanel.GetComponent<RectTransform> ().anchorMax = new Vector2 (0, 0.5f);
				fragmentPanel.GetComponent<RectTransform> ().pivot = new Vector2 (0, 0.5f);
				break;
			case 4:
				fragmentPanel.GetComponent<RectTransform> ().anchorMin = new Vector2 (1, 0.5f);
				fragmentPanel.GetComponent<RectTransform> ().anchorMax = new Vector2 (1, 0.5f);
				fragmentPanel.GetComponent<RectTransform> ().pivot = new Vector2 (1, 0.5f);
				break;
			case 5:
				fragmentPanel.GetComponent<RectTransform> ().anchorMin = new Vector2 (0, 0);
				fragmentPanel.GetComponent<RectTransform> ().anchorMax = new Vector2 (0, 0);
				fragmentPanel.GetComponent<RectTransform> ().pivot = new Vector2 (0, 0);
				break;
			case 6:
				fragmentPanel.GetComponent<RectTransform> ().anchorMin = new Vector2 (1, 0);
				fragmentPanel.GetComponent<RectTransform> ().anchorMax = new Vector2 (1, 0);
				fragmentPanel.GetComponent<RectTransform> ().pivot = new Vector2 (1, 0);
				break;
			default:
				Debug.LogError("Fragment number case not found for " + fragmentNumber);
				break;
		}
		fragmentPanel.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (0,0,0);
		fragmentPanel.GetComponent<RectTransform> ().sizeDelta = new Vector2 (60, 60);
		fragmentPanel.GetComponent<RectTransform> ().localScale = new Vector3 (1.0f, 1.0f, 1.0f);
		foreach (string tileName in fragment)
		{
			AddTileToFragment(GameObject.Find(tileName), fragmentPanel);
		}


	}

	public void AddTileToFragment(GameObject tile, GameObject panel)
	{
		// Instantiate new tile and anchor it in the middle of the panel
		GameObject newTile = new GameObject ("Fragment_" + tile.transform.position.x + "_" + tile.transform.position.z + "_" + tile.tag);

		Image newImage = newTile.AddComponent<Image> ();
		Color32 tileColor = GetTileColor (tile.tag);

		newTile.GetComponent<RectTransform> ().SetParent (panel.transform);
		newTile.GetComponent<RectTransform> ().anchorMin = new Vector2 (0.5f, 0.5f);
		newTile.GetComponent<RectTransform> ().anchorMax = new Vector2 (0.5f, 0.5f);
		newTile.GetComponent<RectTransform> ().anchoredPosition = new Vector3 (
			tileManager.GetRelativePosition (tileManager.GetTileUnderPlayer (), tile) [0] * 5,
			tileManager.GetRelativePosition (tileManager.GetTileUnderPlayer (), tile) [1] * 5,
			0);

		// Set the size and scale of the tile
		newTile.GetComponent<RectTransform> ().sizeDelta = new Vector2 (5, 5);
		newTile.GetComponent<RectTransform> ().localScale = new Vector3 (1.0f, 1.0f, 1.0f);
		newImage.color = tileColor;
		if (tile == gameManager.startingTile)
		{
			Debug.Log("Found spawn tile in fragment");
			DrawSpawnTile(newTile);
		}
		newTile.SetActive (true);
	}




}
