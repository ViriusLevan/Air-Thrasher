using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AirThrasher.Assets.Scripts.UserInterface
{
    public class MenuConfirmation : MonoBehaviour
    {
        [SerializeField] private GameObject menuPanel;

        public void OpenMenuPanel()
        {
            menuPanel.gameObject.SetActive(true);
        }

        public void CloseMenuPanel()
        {
            menuPanel.gameObject.SetActive(false);
        }
    }
}