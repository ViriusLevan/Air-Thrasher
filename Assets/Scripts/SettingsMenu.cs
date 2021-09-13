using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer mainAM;
    [SerializeField] private Slider masterVSlider;
    [SerializeField] private Slider musicVSlider;
    [SerializeField] private Slider sfxVSlider;
    [SerializeField] private Toggle confineToggle;

    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.HasKey("masterVolume"))
        {
            mainAM.GetFloat("masterVolume", out float masterVolTemp);
            mainAM.GetFloat("musicVolume", out float musicVolTemp);
            mainAM.GetFloat("sfxVolume",out float sfxVolTemp);
            PlayerPrefs.SetFloat("masterVolume", masterVolTemp);
            PlayerPrefs.SetFloat("musicVolume", musicVolTemp);
            PlayerPrefs.SetFloat("sfxVolume", sfxVolTemp);
            masterVSlider.value = masterVolTemp;
            musicVSlider.value = musicVolTemp;
            sfxVSlider.value = sfxVolTemp;
            Debug.Log("Tits"+ masterVSlider.value);
        }
        else {
            float mastVol = PlayerPrefs.GetFloat("masterVolume");
            float musicVol = PlayerPrefs.GetFloat("musicVolume");
            float sfxVol = PlayerPrefs.GetFloat("sfxVolume");
            if (mastVol > 0)
            {
                mainAM.SetFloat("masterVolume", Mathf.Log10(mastVol) * 20);
                masterVSlider.value = mastVol;
            }
            if (musicVol > 0) {
                mainAM.SetFloat("musicVolume", Mathf.Log10(musicVol) * 20);
                musicVSlider.value = musicVol;
            }
            if (sfxVol > 0) {
                mainAM.SetFloat("sfxVolume", Mathf.Log10(sfxVol) * 20);
                sfxVSlider.value = sfxVol;
            }
        }
        int cursorLockTemp = PlayerPrefs.GetInt("cursorLock", -1);
        if (cursorLockTemp != -1)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
        else {
            if (cursorLockTemp == 0)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
        PlayerPrefs.Save();
    }

    public void ChangeMasterVolume() {
        float sliderValue = masterVSlider.value;
        mainAM.SetFloat("masterVolume", 
            Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("masterVolume", sliderValue);
        PlayerPrefs.Save();
    }
    public void ChangeBGMVolume()
    {
        float sliderValue = musicVSlider.value;
        mainAM.SetFloat("musicVolume",
            Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("musicVolume", sliderValue);
        PlayerPrefs.Save();
    }
    public void ChangeSFXVolume()
    {
        float sliderValue = sfxVSlider.value;
        mainAM.SetFloat("sfxVolume",
            Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("sfxVolume", sliderValue);
        PlayerPrefs.Save();
    }

    public void ToggleCursorConfined() {
        bool confined = confineToggle.isOn;
        if (confined)
        {
            Cursor.lockState = CursorLockMode.Confined;
            PlayerPrefs.SetInt("cursorLock", 1);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            PlayerPrefs.SetInt("cursorLock", 0);
        }
        PlayerPrefs.Save();
    }
}
