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
	// private float startRange;
	// private float startIntensity;
	// private float startAngle;
	private Light playerLamp;
	public bool lockPlayer;
	public GameManager gameManager;
	private int fuelMaxMultiplier;
	private float fuelConsumptionMultiplier;
	private enum FuelTank {
		Level1 = 700,
		Level2 = 700,
		Level3 = 1100,
	}
	private void Awake () {
		if (SceneManager.GetActiveScene().name != "MenuScene") {
			gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
			fuelTank = GetFuelTank();
        	fuelCount = fuelTank;
		}
		switch (PlayerPrefs.GetString("Difficulty")) {
			case "Easy":
				fuelMaxMultiplier = 50;
				fuelConsumptionMultiplier = 0.8f;
				break;
			case "Medium":
				fuelMaxMultiplier = 25;
				fuelConsumptionMultiplier = 1f;
				break;
			case "Hard":
				fuelMaxMultiplier = 10;
				fuelConsumptionMultiplier = 1.2f;
				break;
		}
		playerLamp = GameObject.Find("PlayerLamp").GetComponent<Light>();
		// startRange = playerLamp.range;
		// startIntensity = playerLamp.intensity;
		// startAngle = playerLamp.spotAngle;

		charController = GetComponent<CharacterController> ();
	}

	private void Update() {
		if (SceneManager.GetActiveScene().name == "MenuScene") return;
		if (Input.GetKey ("w") || Input.GetKey ("a") || Input.GetKey ("s") || Input.GetKey ("d")
		    || Input.GetKey ("up") || Input.GetKey ("left") || Input.GetKey ("right") || Input.GetKey ("down")) {
			if (fuelCount > 0 && !lockPlayer) {
				PlayerMovement ();
				if (playerLamp.enabled) {
					fuelCount -= (fuelConsumption * fuelConsumptionMultiplier) * 1.1f ;
				} else {
					fuelCount -= fuelConsumption * fuelConsumptionMultiplier;
				}
			}
		}
		// if (Input.GetKeyUp("l")) fuelCount += 100;
		// if (Input.GetKeyUp("k")) fuelCount -= 100;

	}

	private int GetFuelTank() {
		int multiplier = 1;
		switch (PlayerPrefs.GetString("Difficulty")) {
			case "Easy":
				multiplier = 50;
				break;
			case "Medium":
				multiplier = 25;
				break;
			case "Hard":
				multiplier = 10;
				break;
		}

		switch (SceneManager.GetActiveScene().name) {
			case "Level1":
				return (int) FuelTank.Level1 + (gameManager.tryCount * multiplier);
			case "Level2":
				return (int) FuelTank.Level2 + (gameManager.tryCount * multiplier);
			case "Level3":
				return (int) FuelTank.Level3 + (gameManager.tryCount * multiplier);
			default:
				return (int) FuelTank.Level1 + (gameManager.tryCount * multiplier);
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
