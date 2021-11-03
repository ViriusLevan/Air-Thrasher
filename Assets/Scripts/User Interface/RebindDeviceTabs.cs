using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RebindDeviceTabs : MonoBehaviour
{
    [SerializeField] private Button[] tabs;
    [SerializeField] private GameObject[] deviceDisplays;
    private Color32 selected = new Color32(132, 217, 224, 255);
    public void SwitchTab(int index) {
        for (int i = 0; i < tabs.Length; i++)
        {
            if (i == index)
            {
                tabs[i].image.color = selected;
                deviceDisplays[i].SetActive(true);
            }
            else
            {
                tabs[i].image.color = Color.white;
                deviceDisplays[i].SetActive(false);
            }

        }
    }
}
