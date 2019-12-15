using UnityEngine;
using UnityEngine.SceneManagement;

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
		Debug.Log(SceneManager.GetActiveScene().name);
	}

	private void Update()
	{
		if (SceneManager.GetActiveScene().name != "MenuScene")
		{
			if (Input.GetKey ("z") || Input.GetKey ("q") || Input.GetKey ("s") || Input.GetKey ("d"))
	        {
	            if (fuelCount > 0) {
            		PlayerMovement ();
            		fuelCount -= fuelConsumption;
	            }
	        }

			if (Input.GetKeyUp("f")) fuelCount += 100;
			if (Input.GetKeyUp("g")) fuelCount -= 100;
		}

	}
	private void PlayerMovement () {
		float horizInput = Input.GetAxis (horizontalInputName) * movementSpeed;
		float vertInput = Input.GetAxis (verticalInputName) * movementSpeed;
		Vector3  forwardMovement = transform.forward * vertInput;
		Vector3 rightMovement = transform.right * horizInput;
		charController.SimpleMove (forwardMovement + rightMovement);
	}
}
