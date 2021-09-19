using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
