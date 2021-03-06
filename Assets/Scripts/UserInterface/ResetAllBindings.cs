using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AirThrasher.Assets.Scripts.UserInterface
{
    public class ResetAllBindings : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActionAsset;

        public void ResetBindings()
        {
            foreach (InputActionMap map in inputActionAsset.actionMaps)
            {
                map.RemoveAllBindingOverrides();
            }
            PlayerPrefs.DeleteKey("rebinds");
        }
    }
}