using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnMouseExit() {
        Debug.Log($"MouseExit {this.gameObject.name}");
        transform.GetComponent<TextMeshProUGUI>().fontSize -= 5;

    }

    private void OnMouseEnter() {
        Debug.Log($"MouseEnter {this.gameObject.name}");
        transform.GetComponent<TextMeshProUGUI>().fontSize += 5;
    }
}
