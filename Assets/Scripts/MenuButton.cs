using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    private TextMeshProUGUI debugText;
    // Start is called before the first frame update
    private void Awake()
    {
        debugText = GameObject.Find("DebugText").GetComponent<TextMeshProUGUI>();
    }

    private void OnMouseDown()
    {
        Debug.Log("Click detected on" + this.gameObject.name);
        StopCoroutine("DisplayText");
        StartCoroutine("DisplayText");
    }

    IEnumerator DisplayText()
    {
        debugText.gameObject.SetActive(true);
        debugText.text = "Clicked on " + this.gameObject.name;
        yield return new WaitForSeconds(1f);
        debugText.gameObject.SetActive(false);
    }

}
