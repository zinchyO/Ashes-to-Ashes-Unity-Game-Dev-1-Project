using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public static Menu MENU;

    [Header("Canvases")]
    public Canvas menuCanvas;
    public Canvas selectionCanvas;
    
    void Start()
    {
        MENU = this;
        menuCanvas.gameObject.SetActive(true);
        selectionCanvas.gameObject.SetActive(false);
    }

    public void GoToSelection()
    {
        menuCanvas.gameObject.SetActive(false);
        selectionCanvas.gameObject.SetActive(true);
    }
}
