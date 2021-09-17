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
    [SerializeField] private Toggle confineToggle, mouseMoveDisable;

    public delegate void OnToggled(bool newState);
    public static event OnToggled mouseMoveToggle;

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
        if (cursorLockTemp == -1 || cursorLockTemp == 1)
        {
            Cursor.lockState = CursorLockMode.Confined;
            confineToggle.isOn = true;
        }
        else {//0
            Cursor.lockState = CursorLockMode.None;
            confineToggle.isOn = false;
        }

        int mouseDisable = PlayerPrefs.GetInt("mouseMoveDisable", -1);
        if (mouseDisable == -1 || mouseDisable ==0)
        {
            StartCoroutine(WaitTillPlayerSpawns(false));
            mouseMoveDisable.isOn = false;
        }
        else
        {
            StartCoroutine(WaitTillPlayerSpawns(true));
            mouseMoveDisable.isOn = true;
        }
        PlayerPrefs.Save();
    }

    private IEnumerator WaitTillPlayerSpawns(bool state) {
        while (mouseMoveToggle == null)
            yield return new WaitForSeconds(0.1f);
        mouseMoveToggle(state);
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

    public void ToggleMouseDisable() {
        bool state = mouseMoveDisable.isOn;
        if (!state)
        {
            if(mouseMoveToggle!=null)
                mouseMoveToggle(false);
            PlayerPrefs.SetInt("mouseMoveDisable", 0);
        }
        else
        {
            if (mouseMoveToggle != null)
                mouseMoveToggle(true);
            PlayerPrefs.SetInt("mouseMoveDisable", 1);
        }
        PlayerPrefs.Save();
    }
}
