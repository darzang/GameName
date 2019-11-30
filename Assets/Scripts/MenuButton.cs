using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class MenuButton : MonoBehaviour
{
    public MenuManager menuManager;
    private TextMeshProUGUI debugText;
    // Start is called before the first frame update
    private void Awake()
    {
        menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
        debugText = GameObject.Find("DebugText").GetComponent<TextMeshProUGUI>();
    }

    private void OnMouseDown()
    {
        menuManager.HandleClick(this.gameObject);

//        StopCoroutine("DisplayText");
//        StartCoroutine("DisplayText");
    }

    IEnumerator DisplayText()
    {
        debugText.gameObject.SetActive(true);
        debugText.text = "Clicked on " + this.gameObject.name;
        yield return new WaitForSeconds(1f);
        debugText.gameObject.SetActive(false);
    }

}
