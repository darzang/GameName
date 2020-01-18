using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {
	[SerializeField] private string horizontalInputName;
	[SerializeField] private string verticalInputName;
	[SerializeField] public float movementSpeed;
	[SerializeField] public float fuelConsumption;
	private CharacterController charController;
	public float fuelCount;
	[SerializeField] public float fuelTank = 800;
	private Light playerLamp;
	public bool lockPlayer;
	public GameManager gameManager;
	private int fuelMaxMultiplier;
	private float fuelConsumptionMultiplier;
	private void Awake () {
		if (SceneManager.GetActiveScene().name != "MenuScene") {
			gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
			fuelTank = gameManager.playerData.batteryMax;
			fuelCount = fuelTank;
		}
		playerLamp = GameObject.Find("PlayerLamp").GetComponent<Light>();

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
	
	private void PlayerMovement () {
		float horizontalInput = Input.GetAxis (horizontalInputName) * movementSpeed;
		float verticalInput = Input.GetAxis (verticalInputName) * movementSpeed;
		Transform playerTransform = transform;
		Vector3  forwardMovement = playerTransform.forward * verticalInput;
		Vector3 rightMovement = playerTransform.right * horizontalInput;
		charController.SimpleMove (forwardMovement + rightMovement);
	}
}
