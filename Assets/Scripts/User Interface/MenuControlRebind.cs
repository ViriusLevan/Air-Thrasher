using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuControlRebind : MonoBehaviour
{
    [SerializeField]private GameObject controlRebindPanel;

    public void OpenControlRebindPanel()
    {
        controlRebindPanel.SetActive(true);
    }
    public void CloseControlRebindPanel()
    {
        controlRebindPanel.SetActive(false);
    }

}
