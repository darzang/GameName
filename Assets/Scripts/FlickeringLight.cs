using System.Collections;
using UnityEngine;

public class FlickeringLight : MonoBehaviour {
    public Material lampOn;
    public Material lampOff;
    private new Light light;
    private new MeshRenderer renderer;

    private void Start() {
        light = transform.Find("Light").GetComponent<Light>();
        renderer = transform.Find("Lamp").GetComponent<MeshRenderer>();
        StartCoroutine(nameof(LightFlicker));
    }

    private IEnumerator LightFlicker()
    {
        float flickeringSpeed = 0.1f;
        while (true) {
            float trigger = Random.Range(0, 5);
            if (trigger >= 2) {
                light.enabled = false;
                renderer.material = lampOff;
            } else {
                light.enabled = true;
                renderer.material = lampOn;
            }
            yield return new WaitForSeconds(flickeringSpeed);
        }
    }
}
