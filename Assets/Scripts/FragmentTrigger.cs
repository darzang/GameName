using UnityEngine;

public class FragmentTrigger : MonoBehaviour {
    public GameManager gameManager;

    private void Start() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void Update() {
        transform.Rotate(1f, 1f, 1f, Space.Self);
        // Quaternion rotation = transform.rotation;
        // rotation.y += 1f;
        // transform.rotation = Quaternion.Euler(0, transform.rotation.y + 10f, 60f);
        // transform.rotation = rotation;
        // transform.Rotate(Vector3.left, 5f, Space.Self);
        // transform.eulerAngles = new Vector3(0, transform.rotation.y + 10f, 60);
    }
    private void OnTriggerEnter(Collider collider) {
        gameManager.PickupFragment(transform.gameObject);
    }
}