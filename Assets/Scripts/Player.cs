using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {
	[SerializeField] private string horizontalInputName;
	[SerializeField] private string verticalInputName;
	[SerializeField] public float movementSpeed;
	[SerializeField] public float fuelConsumption;
	private CharacterController charController;
	public float fuelCount;
	[SerializeField] public float fuelTank = 1000;
	private float startRange;
	private float startIntensity;
	private float startAngle;
	private Light playerLamp;
	public bool lockPlayer;

	private enum FuelTank {
		Level1 = 600,  // 500 - 600
		Level2 = 600,
		Level3 = 1100,
	}
	private void Awake () {
		fuelTank = GetFuelTank();
		fuelCount = fuelTank;
		playerLamp = GameObject.Find("PlayerLamp").GetComponent<Light>();
		startRange = playerLamp.range;
		startIntensity = playerLamp.intensity;
		startAngle = playerLamp.spotAngle;

		charController = GetComponent<CharacterController> ();
	}

	private void Update() {
		if (SceneManager.GetActiveScene().name == "MenuScene") return;
		if (Input.GetKey ("z") || Input.GetKey ("q") || Input.GetKey ("s") || Input.GetKey ("d")) {
			if (fuelCount > 0 && !lockPlayer) {
				PlayerMovement ();
				fuelCount -= fuelConsumption;
				playerLamp.range = startRange * (fuelCount / fuelTank);
				playerLamp.intensity = startIntensity * (fuelCount / fuelTank);
				playerLamp.spotAngle = startAngle * (fuelCount / fuelTank);
			}
		}
		if (Input.GetKeyUp("f")) fuelCount += 100;
		if (Input.GetKeyUp("g")) fuelCount -= 100;

	}

	private int GetFuelTank() {
		switch (SceneManager.GetActiveScene().name) {
			case "Level1":
				return (int) FuelTank.Level1;
			case "Level2":
				return (int) FuelTank.Level2;
			case "Level3":
				return (int) FuelTank.Level3;
			default:
				return (int) FuelTank.Level1;
		}
	}
	private void PlayerMovement () {
		float horizontalInput = Input.GetAxis (horizontalInputName) * movementSpeed;
		float verticalInput = Input.GetAxis (verticalInputName) * movementSpeed;
		Transform playerTransform = transform;
		Vector3  forwardMovement = playerTransform.forward * verticalInput;
		Vector3 rightMovement = playerTransform.right * horizontalInput;
		charController.SimpleMove (forwardMovement + rightMovement);
	}
}
