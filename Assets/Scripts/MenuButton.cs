using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MenuButton : MonoBehaviour
{
    public MenuManager menuManager;
    // Start is called before the first frame update
    private void Awake()
    {
        menuManager = GameObject.Find("MenuManager").GetComponent<MenuManager>();
    }

    private void OnMouseDown()
    {
       menuManager.HandleClick(gameObject);
    }

    private void OnMouseExit() {
        transform.GetComponent<TextMeshPro>().fontSize -= 5;
    }

    private void OnMouseEnter() {
        transform.GetComponent<TextMeshPro>().fontSize += 5;
    }
}
