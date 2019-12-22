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
	public bool lockPlayer = false;

	void Awake () {
		fuelCount = fuelTank;
		playerLamp = GameObject.Find("PlayerLamp").GetComponent<Light>();
		startRange = playerLamp.range;
		startIntensity = playerLamp.intensity;
		startAngle = playerLamp.spotAngle;

		charController = GetComponent<CharacterController> ();
	}

	private void Update()
	{
		if (SceneManager.GetActiveScene().name != "MenuScene")
		{
			if (Input.GetKey ("z") || Input.GetKey ("q") || Input.GetKey ("s") || Input.GetKey ("d"))
	        {
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

	}
	private void PlayerMovement () {
		float horizInput = Input.GetAxis (horizontalInputName) * movementSpeed;
		float vertInput = Input.GetAxis (verticalInputName) * movementSpeed;
		Vector3  forwardMovement = transform.forward * vertInput;
		Vector3 rightMovement = transform.right * horizInput;
		charController.SimpleMove (forwardMovement + rightMovement);
	}
}
