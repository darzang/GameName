using System.Collections;
using UnityEngine;

public class FlickeringLight : MonoBehaviour {
    public Material lampOn;
    public Material lampOff;
    private Light _light;
    private MeshRenderer _renderer;

    private void Start() {
        _light = transform.Find("Light").GetComponent<Light>();
        _renderer = transform.Find("Lamp").GetComponent<MeshRenderer>();
        StartCoroutine(nameof(LightFlicker));
    }

    private IEnumerator LightFlicker()
    {
        float flickeringSpeed = 0.5f;
        while (true) {
            float trigger = Random.Range(0, 5);
            if (trigger >= 3) {
                _light.enabled = false;
                _renderer.material = lampOff;
            } else {
                _light.enabled = true;
                _renderer.material = lampOn;
            }
            yield return new WaitForSeconds(flickeringSpeed);
        }
    }
}
