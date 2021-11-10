using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace AirThrasher.Assets.Scripts.UserInterface
{
    public class SettingsMenu : MonoBehaviour
    {
        public static float musicVolume, sfxVolume;
        //[SerializeField] private AudioMixer mainAM;
        //[SerializeField] private Slider masterVSlider;
        [SerializeField] private Slider musicVSlider;
        [SerializeField] private Slider sfxVSlider;
        [SerializeField] private Toggle confineToggle, mouseMoveDisable;

        public delegate void OnToggled(bool newState);
        public static event OnToggled mouseMoveToggle;

        public delegate void OnVolumeChanged(float newVolume);
        public static event OnVolumeChanged musicVolumeChange, sfxVolumeChange;

        void Awake()
        {
            //InputSystem.AddDevice(new InputDeviceDescription
            //{
            //    deviceClass = "Mouse",
            //    interfaceName = "HID",
            //    product= "Gaming Mouse G400",
            //    version= "26880",
            //    manufacturer= "Logitech",
            //    capabilities= "{\"vendorId\":1133,\"productId\":49733,\"usage\":128,\"usagePage\":65408,\"inputReportSize\":2,\"outputReportSize\":0,\"featureReportSize\":2,\"elements\":[],\"collections\":[]}"
            //});
            if (!PlayerPrefs.HasKey("musicVolume"))
            {
                //mainAM.GetFloat("masterVolume", out float masterVolTemp);
                //mainAM.GetFloat("musicVolume", out float musicVolTemp);
                //mainAM.GetFloat("sfxVolume",out float sfxVolTemp);
                //PlayerPrefs.SetFloat("masterVolume", masterVolume);

                musicVolume = 0.3f;
                sfxVolume = 0.10f;
                PlayerPrefs.SetFloat("musicVolume", musicVolume);
                PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
                //masterVSlider.value = masterVolume;
                musicVSlider.value = musicVolume;
                sfxVSlider.value = sfxVolume;
                musicVolumeChange?.Invoke(musicVolume);
                sfxVolumeChange?.Invoke(sfxVolume);
            }
            else
            {
                // mastVol = PlayerPrefs.GetFloat("masterVolume");
                float musicVol = PlayerPrefs.GetFloat("musicVolume");
                float sfxVol = PlayerPrefs.GetFloat("sfxVolume");
                //if (mastVol > 0)
                //{
                //    //mainAM.SetFloat("masterVolume", Mathf.Log10(mastVol) * 20);
                //    masterVolume = mastVol;
                //    masterVSlider.value = mastVol;
                //}
                if (musicVol >= 0)
                {
                    //mainAM.SetFloat("musicVolume", Mathf.Log10(musicVol) * 20);
                    musicVolume = musicVol;
                    musicVSlider.value = musicVol;
                    musicVolumeChange?.Invoke(musicVolume);
                }
                if (sfxVol >= 0)
                {
                    //mainAM.SetFloat("sfxVolume", Mathf.Log10(sfxVol) * 20);
                    sfxVolume = sfxVol;
                    sfxVSlider.value = sfxVol;
                    sfxVolumeChange?.Invoke(sfxVolume);
                }
            }
            int cursorLockTemp = PlayerPrefs.GetInt("cursorLock", -1);
            if (cursorLockTemp == -1 || cursorLockTemp == 1)
            {
                Cursor.lockState = CursorLockMode.Confined;
                confineToggle.isOn = true;
            }
            else
            {//0
                Cursor.lockState = CursorLockMode.None;
                confineToggle.isOn = false;
            }

            int mouseDisable = PlayerPrefs.GetInt("mouseMoveDisable", -1);
            if (mouseDisable == -1 || mouseDisable == 0)
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

        private IEnumerator WaitTillPlayerSpawns(bool state)
        {
            yield return new WaitForSeconds(0.1f);
            while (mouseMoveToggle == null)
                yield return new WaitForSeconds(0.1f);
            mouseMoveToggle(state);
            sfxVolumeChange?.Invoke(sfxVolume);
            musicVolumeChange?.Invoke(musicVolume);
        }
        public void ChangeBGMVolume()
        {
            float sliderValue = musicVSlider.value;
            //mainAM.SetFloat("musicVolume",
            //    Mathf.Log10(sliderValue) * 20);
            musicVolume = sliderValue;
            PlayerPrefs.SetFloat("musicVolume", sliderValue);
            PlayerPrefs.Save();
            musicVolumeChange?.Invoke(sliderValue);
        }
        public void ChangeSFXVolume()
        {
            float sliderValue = sfxVSlider.value;
            //mainAM.SetFloat("sfxVolume",
            //    Mathf.Log10(sliderValue) * 20);
            sfxVolume = sliderValue;
            PlayerPrefs.SetFloat("sfxVolume", sliderValue);
            PlayerPrefs.Save();
            sfxVolumeChange?.Invoke(sliderValue);
        }

        public void ToggleCursorConfined()
        {
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

        public void ToggleMouseDisable()
        {
            bool state = mouseMoveDisable.isOn;
            if (!state)
            {
                if (mouseMoveToggle != null)
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
}