using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour {

	GameObject fuelBar;
	GameObject player;
	// Use this for initialization
	double fuelTank;
	double fuelCount;
	void Start () {
		fuelBar = GameObject.Find ("FuelBar");
		player = GameObject.Find ("Player");
		fuelTank = player.GetComponent<Player> ().fuelTank;
	}

	// Update is called once per frame
	void Update () {
		UpdateHealthBar ();
	}

	void UpdateHealthBar () {
		fuelCount = player.GetComponent<Player> ().fuelCount ;
		// Update scale
		fuelBar.transform.localScale = new Vector3 (
			(float) (fuelCount / fuelTank),
			fuelBar.transform.localScale.y,
			fuelBar.transform.localScale.z
		);

		// Update color
		if (fuelCount > fuelTank / 2) {
			fuelBar.GetComponent<Image> ().color = new Color32 ((byte)(fuelTank - fuelCount), 255, 0, 255);
		} else {
			fuelBar.GetComponent<Image> ().color = new Color32 (255, (byte)fuelCount, 0, 255);
		}

	}
}