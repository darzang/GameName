using System.Collections;
using UnityEngine;

public class FragmentTrigger : MonoBehaviour {
    public GameManager gameManager;
    private Animation anim;
    private bool isRotating;

    private void Start() {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        // anim = GetComponent<Animation>();
        // anim.wrapMode = WrapMode.Loop;
        // anim.Play();
    }
    
    private void OnTriggerEnter(Collider collider) {
        gameManager.PickupFragment(transform.gameObject);
    }
    
    private IEnumerator Rotate() {
        while (true) {
            if (!isRotating) {
                anim.Play("EyeLidClose");
                yield return new WaitForSeconds(2f);
                isRotating = false;
            }
        }
    }
}