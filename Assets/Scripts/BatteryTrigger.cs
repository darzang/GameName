using UnityEngine;

public class BatteryTrigger : MonoBehaviour {
    public GameManager gameManager;

    private void Start() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    private void OnTriggerEnter(Collider collider) {
        gameManager.PickupBattery(transform.gameObject);
    }
}