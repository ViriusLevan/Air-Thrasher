using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    [SerializeField] private TMP_Text speedText, scoreText, boostText, healthText, finalScoreText;
    [SerializeField] private GameObject player;
    [SerializeField] private Image healthBar, boostBar;
    [SerializeField] private GameObject finalPanel;
    private int score;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        Player.balloonPopped += PlayerPoppedBalloon;
        Player.playerHasDied += GameOver;
    }

    // Update is called once per frame
    void Update()
    {
        speedText.text = ((int)player.GetComponent<Player>().activeForwardSpeed).ToString();

        float pBoostFuel = player.GetComponent<Player>().boostFuel;
        boostText.text = ((int)pBoostFuel).ToString();

        float boostFill = pBoostFuel / player.GetComponent<Player>().boostMax;
        boostBar.fillAmount = boostFill;

        int pHealth = player.GetComponent<Player>().GetHealth();
        healthText.text = pHealth.ToString();
        float healthFill = ((float)pHealth) / ((float)player.GetComponent<Player>().GetMaxHealth());
        healthBar.fillAmount = healthFill;
    }

    private void PlayerPoppedBalloon() {
        IncreaseScore(100);
    }

    public void IncreaseScore(int amount) {
        score += amount;
        scoreText.text = score.ToString();
    }

    public void GameOver() {
        Time.timeScale = 0;
        finalPanel.SetActive(true);
        finalScoreText.text = score.ToString();
    }

    public void PauseORResume ()
    {
        if (Time.timeScale == 0) {
            Time.timeScale = 1;
        }
        else{
            Time.timeScale = 0;
        }
    }


    public void RestartGame() {
        SceneManager.LoadScene("SampleScene");
    }
}
