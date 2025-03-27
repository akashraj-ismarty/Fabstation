using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject lightPanel;
    [SerializeField] private GameObject texturePanel;

    private int currentTexture;

    public GameObject centerPoint;
    private static bool isFileBrowserOpen;
    public static bool arePanelsOpen;

    public GameObject xAxisIndicator;
    public GameObject yAxisIndicator;
    public GameObject zAxisIndicator;


    private void Start()
    {
        // arePanelsOpen = false;
        // isFileBrowserOpen = false;
        // currentTexture = 0;

    }

    private void Update()
    {

    }

    //Opens and closes light panel
    public void OpenLightPanel()
    {
        // if (!lightPanel.activeSelf && !texturePanel.activeSelf)
        // {
        //     lightPanel.SetActive(true);
        // }
        // else
        // {
        //     lightPanel.SetActive(false);
        // }
        UpdatePanels();
    }

    public void OpenTexturePanel()
    {
        // if (!texturePanel.activeSelf && !lightPanel.activeSelf)
        // {
        //     texturePanel.SetActive(true);
        // }
        // else
        // {
        //     texturePanel.SetActive(false);
        // }
        UpdatePanels();
    }

    private void UpdatePanels()
    {
        // if (lightPanel.activeSelf || isFileBrowserOpen || texturePanel.activeSelf)
        //     arePanelsOpen = true;
        // else
        //     arePanelsOpen = false;
    }

    public void SetFileBrowserOpen(bool state)
    {
        isFileBrowserOpen = state;
        UpdatePanels();
    }

    //Back button to Menu
    public void OpenMenu()
    {
        SceneManager.LoadScene(0); // Load the menu scene
    }

  
   

    public void DisableAxisIndicators()
    {
        if (xAxisIndicator.activeSelf)
        {
            xAxisIndicator.SetActive(false);
            yAxisIndicator.SetActive(false);
            zAxisIndicator.SetActive(false);
        }
        else
        {
            xAxisIndicator.SetActive(true);
            yAxisIndicator.SetActive(true);
            zAxisIndicator.SetActive(true);
        }
    }

    
}
