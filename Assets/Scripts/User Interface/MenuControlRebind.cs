using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuControlRebind : MonoBehaviour
{
    [SerializeField]private GameObject controlRebindPanel;

    public void OpenControlRebindPanel()
    {
        Manager.menuState = Manager.MenuState.Rebind;
        controlRebindPanel.SetActive(true);
    }
    public void CloseControlRebindPanel()
    {
        Manager.menuState = Manager.MenuState.Settings;
        controlRebindPanel.SetActive(false);
    }

}
