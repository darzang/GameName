using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public double fuelCount;
	[SerializeField] public double fuelTank = 500;

	GameObject player;
	GameObject lamp;

	bool lampActive;

	void Start () {
		fuelCount = fuelTank;
		player = GameObject.Find("Player");
		lamp = GameObject.Find("PlayerLamp");
		Debug.Log(lamp);
	}

	void Update () {
		if(Input.GetKeyDown("e")){
			TogglePlayerLamp();
		}
	}

	void TogglePlayerLamp(){
		lamp.GetComponent<Light>().gameObject.SetActive(!lamp.GetComponent<Light>().gameObject.activeSelf);
	}



}