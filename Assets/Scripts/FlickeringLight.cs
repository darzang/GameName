using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    public Material lampOn;

    public Material lampOff;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("LightFlicker");
    }

    IEnumerator LightFlicker()
    {
        float flickeringSpeed = 0.1f;
        Light light = transform.Find("Light").GetComponent<Light>();
        MeshRenderer renderer = transform.Find("Lamp").GetComponent<MeshRenderer>();
        while (true)
        {
            float trigger = Random.Range(0, 5);
            if (trigger >= 2)
            {
                light.enabled = false;
                renderer.material = lampOff;
            }
            else
            {
                light.enabled = true;
                renderer.material = lampOn;
            }
            yield return new WaitForSeconds(flickeringSpeed);
        }
    }
}
