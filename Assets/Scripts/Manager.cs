using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    [SerializeField] private TMP_Text speedText, scoreText, boostText, 
        healthText, finalScoreText, highScoreText;
    [SerializeField] private GameObject player;
    [SerializeField] private Image healthBar, boostBar;
    [SerializeField] private GameObject finalPanel;
    [SerializeField] private UI_EventText eventText;
    [SerializeField] private GameObject settingsPanel;
    private int score, playerMaxHealth, highScore;
    private float playerMaxBoost;
    public bool paused;

    private void Awake()
    {
        Player.initializeUI += InitializeUI;
        if (PlayerPrefs.HasKey("highScore")) {
            highScore = PlayerPrefs.GetInt("highScore");
            highScoreText.text = highScore.ToString();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        Time.timeScale = 1;
        Player.playerFiredGun += PlayerFiredGun;
        Player.playerBoosted += PlayerBoosted;
        Player.playerHitByMissile += PlayerHitByMissile;
        Player.playerHitByMine += PlayerHitByMine;
        Player.playerHitByLaser += PlayerHitByLaser;
        Player.playerHitByRam += PlayerHitByRam;
        Player.balloonCrashPop += PlayerDirectlyPopped;
        Player.shotBalloonPop += PlayerShotPopped;
        Player.playerHasDied += GameOver;
    }

    private void OnDestroy()
    {
        Player.playerFiredGun -= PlayerFiredGun;
        Player.playerBoosted -= PlayerBoosted;
        Player.playerHitByMissile -= PlayerHitByMissile;
        Player.playerHitByMine -= PlayerHitByMine;
        Player.playerHitByLaser -= PlayerHitByLaser;
        Player.playerHitByRam -= PlayerHitByRam;
        Player.balloonCrashPop -= PlayerDirectlyPopped;
        Player.shotBalloonPop -= PlayerShotPopped;
        Player.playerHasDied -= GameOver;
        Player.initializeUI -= InitializeUI;
    }
    private void InitializeUI(int boost, int health)
    {
        if (player != null)
        {
            playerMaxHealth = player.GetComponent<Player>().GetMaxHealth();
            playerMaxBoost = player.GetComponent<Player>().GetBoostMax();
        }
        ChangeBoostDisplay(boost);
        ChangeHealthDisplay(health);
        score = 0;
        IncreaseScore(0);
    }

    // Update is called once per frame
    void Update()
    {
        //Doesn't use an event since speed is changed almost constantly
        speedText.text = ((int)player.GetComponent<Player>().GetActiveForwardSpeed()).ToString();
    }

    private void PlayerFiredGun(int updatedFuelValue)
    {
        eventText.DisplayAnEvent("Fired Gun", 0);
        ChangeBoostDisplay(updatedFuelValue);
    }

    private void PlayerBoosted(int updatedFuelValue)
    {
        eventText.DisplayAnEvent("Boosted", 0);
        ChangeBoostDisplay(updatedFuelValue);
    }

    private void ChangeBoostDisplay(int boostFuel) {
        float boostFill = ((float)boostFuel) / playerMaxBoost;
        boostText.text = boostFuel.ToString();
        boostBar.fillAmount = boostFill;
    }

    private void PlayerHitByMine(int healthAmount)
    {
        eventText.DisplayAnEvent("Mine Hit", 1);
        ChangeHealthDisplay(healthAmount);
    }

    private void PlayerHitByMissile(int healthAmount)
    {
        eventText.DisplayAnEvent("Missile Hit", 1);
        ChangeHealthDisplay(healthAmount);
    }
    private void PlayerHitByLaser(int healthAmount)
    {
        eventText.DisplayAnEvent("Laser Hit", 1);
        ChangeHealthDisplay(healthAmount);
    }

    private void PlayerHitByRam(int healthAmount)
    {
        eventText.DisplayAnEvent("Ram Hit", 1);
        ChangeHealthDisplay(healthAmount);
    }

    private void ChangeHealthDisplay(int healthAmount) {
        healthText.text = healthAmount.ToString();
        float healthFill = ((float)healthAmount) / ((float)playerMaxHealth);
        healthBar.fillAmount = healthFill;
    }

    private void PlayerDirectlyPopped
        (int scoreIncrease, int updatedFuel)
    {
        eventText.DisplayAnEvent("Direct Pop", 2);
        IncreaseScore(scoreIncrease);
        ChangeBoostDisplay(updatedFuel);
    }

    private void PlayerShotPopped
        (int scoreIncrease, int updatedFuel)
    {
        eventText.DisplayAnEvent("Shot-Up", 2);
        IncreaseScore(scoreIncrease);
        ChangeBoostDisplay(updatedFuel);
    }

    private void IncreaseScore(int amount) {
        score += amount;
        scoreText.text = score.ToString();
        if (highScore < score) {
            highScore = score;
            PlayerPrefs.SetInt("highScore",highScore);
            highScoreText.text = highScore.ToString();
        }
    }

    public void GameOver() {
        Time.timeScale = 0;
        finalPanel.SetActive(true);
        finalScoreText.text = score.ToString();
    }

    public void PauseORResume ()
    {
        if (Time.timeScale == 0)
        {
            settingsPanel.SetActive(false);
            Time.timeScale = 1;
            paused = false;
        }
        else{
            Time.timeScale = 0;
            settingsPanel.SetActive(true);
            paused = true;
        }
    }

    public void RestartGame() {
        SceneManager.LoadScene(0);
    }

    public void OpenKofi() {
        Application.OpenURL("https://ko-fi.com/viriuslevan");
    }
}
