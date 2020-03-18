using UnityEngine;

public class FragmentTrigger : MonoBehaviour {
    public GameManager gameManager;
    private Animation _anim;
    private bool _isRotating;

    private void Start() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    
    private void OnTriggerEnter(Collider collider) {
        gameManager.PickupFragment(transform.gameObject);
    }
}