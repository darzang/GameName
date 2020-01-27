using UnityEngine;

public class FragmentTrigger : MonoBehaviour {
    public GameManager gameManager;

    private void Start() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void Update() {
        transform.Rotate(1f, 1f, 1f, Space.Self);
    }
    private void OnTriggerEnter(Collider collider) {
        gameManager.PickupFragment(transform.gameObject);
    }
}