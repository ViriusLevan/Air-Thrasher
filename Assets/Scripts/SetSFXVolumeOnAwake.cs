using AirThrasher.Assets.Scripts.UserInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AirThrasher.Assets.Scripts
{
    public class SetSFXVolumeOnAwake : MonoBehaviour
    {
        private void Awake()
        {
            AudioSource[] aSources = GetComponents<AudioSource>();
            for (int i = 0; i < aSources.Length; i++)
            {
                aSources[i].volume = SettingsMenu.sfxVolume;
            }
        }
    }
}