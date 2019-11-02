using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour {
	[SerializeField] private string horizontalInputName;
	[SerializeField] private string verticalInputName;
	[SerializeField] public float movementSpeed;
	[SerializeField] public float fuelconsumption;

	private CharacterController charController;
	private GameObject player;
	private void Awake () {
		charController = GetComponent<CharacterController> ();
		player = GameObject.Find ("Player");
	}

	private void Update () {
		PlayerMovement();
		// player.GetComponent<Player>().fuelCount -= fuelconsumption;
		// if (Input.GetKey ("z") || Input.GetKey ("q") || Input.GetKey ("s") || Input.GetKey ("d")) {
		// 	if (player.GetComponent<Player> ().fuelCount > 0) {
		// 		PlayerMovement ();
		// 	} else {
		// 		Debug.Log ("Player doesn't have fuel left");
		// 	}
		// }
	}

	private void PlayerMovement () {
		float horizInput = Input.GetAxis (horizontalInputName) * movementSpeed;
		float vertInput = Input.GetAxis (verticalInputName) * movementSpeed;
		Vector3 forwardMovement = transform.forward * vertInput;
		Vector3 rightMovement = transform.right * horizInput;
		charController.SimpleMove (forwardMovement + rightMovement);
	}

}