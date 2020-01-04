using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {
	[SerializeField] private string horizontalInputName;
	[SerializeField] private string verticalInputName;
	[SerializeField] public float movementSpeed;
	[SerializeField] public float fuelConsumption;
	private CharacterController charController;
	public float fuelCount;
	[SerializeField] public float fuelTank = 500;
	private float startRange;
	private float startIntensity;
	private float startAngle;
	private Light playerLamp;
	public bool lockPlayer;

	private void Awake () {
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
				playerLamp.range = startRange * (fuelCount / fuelTank) * 2 ;
				if (playerLamp.range > startRange) playerLamp.range = startRange;
				playerLamp.intensity = startIntensity * (fuelCount / fuelTank) * 2;
				if (playerLamp.intensity > startIntensity) playerLamp.intensity = startIntensity;
				playerLamp.spotAngle = startAngle * (fuelCount / fuelTank) * 2;
				if (playerLamp.spotAngle > startAngle) playerLamp.spotAngle = startAngle;
			}
		}
		if (Input.GetKeyUp("f")) fuelCount += 100;
		if (Input.GetKeyUp("g")) fuelCount -= 100;

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
