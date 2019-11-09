using System;
using UnityEngine;

public class Player : MonoBehaviour {
	[SerializeField] private string horizontalInputName;
	[SerializeField] private string verticalInputName;
	[SerializeField] public float movementSpeed;
	[SerializeField] public float fuelConsumption;
	private CharacterController charController;
	public double fuelCount;
	[SerializeField] public double fuelTank = 500;

	void Awake () {
		fuelCount = fuelTank;
		charController = GetComponent<CharacterController> ();
	}

	private void Update()
	{
		if (Input.GetKey ("z") || Input.GetKey ("q") || Input.GetKey ("s") || Input.GetKey ("d"))
		{
			Debug.Log("Input");
			if (fuelCount > 0) {
				PlayerMovement ();
				fuelCount -= fuelConsumption;
			} else {
				Debug.Log ("Player doesn't have fuel left");
			}
		}
	}
	private void PlayerMovement () {
		float horizInput = Input.GetAxis (horizontalInputName) * movementSpeed;
		float vertInput = Input.GetAxis (verticalInputName) * movementSpeed;
		Vector3 forwardMovement = transform.forward * vertInput;
		Vector3 rightMovement = transform.right * horizInput;
		charController.SimpleMove (forwardMovement + rightMovement);
	}
}
