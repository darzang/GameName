using UnityEngine;


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



}
