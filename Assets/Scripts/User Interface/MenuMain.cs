using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuMain : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    private void Awake()
    {
        Time.timeScale = 0;
    }

    private void Start()
    {
        Time.timeScale = 0;
    }

    public void StartGame() { 
        Time.timeScale = 1;
        mainMenuPanel.SetActive(false);
    }
}
