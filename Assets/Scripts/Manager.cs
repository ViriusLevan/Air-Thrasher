using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Manager : MonoBehaviour
{

    [SerializeField] private TMP_Text speedText, scoreText, boostText, 
        healthText, finalScoreText, highScoreText, timeText;
    [SerializeField] private GameObject player;
    [SerializeField] private Image healthBar, boostBar;
    [SerializeField] private GameObject finalPanel;
    [SerializeField] private UI_EventText eventText;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button pauseButton, restartButton;
    private int score, playerMaxHealth, highScore, pollenKillCount;
    private float playerMaxBoost, pauseTimer=0f, 
        pauseCooldown=0.5f, runtime=0f;
    private bool dead=false; //bruh
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
    {//TODO remove this
        Cursor.visible = false;

        pollenKillCount = 0;
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
        Pollen_Spine.spineDeath += IncrementSpineKillCount;
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
        Pollen_Spine.spineDeath -= IncrementSpineKillCount;
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
        if (Input.GetAxisRaw("Cancel") == 1
                && pauseTimer <= 0)
        {
            if (!dead)
            {
                PauseORResume();
                pauseTimer = pauseCooldown;
            }
            else {
                RestartGame();
            }
        }
        else if (pauseTimer > 0)
        {
            pauseTimer -= Time.unscaledDeltaTime;
        }

        if (!dead) {
            runtime += Time.deltaTime;
            timeText.text = runtime.ToString("0.00");
        }
    }

    private void PlayerFiredGun(int updatedFuelValue)
    {
        eventText?.DisplayAnEvent("Fired Gun", 0);
        ChangeBoostDisplay(updatedFuelValue);
    }

    private void PlayerBoosted(int updatedFuelValue)
    {
        eventText?.DisplayAnEvent("Boosted", 0);
        ChangeBoostDisplay(updatedFuelValue);
    }

    private void ChangeBoostDisplay(int boostFuel) {
        float boostFill = ((float)boostFuel) / playerMaxBoost;
        boostText.text = boostFuel.ToString();
        boostBar.fillAmount = boostFill;
    }

    private void PlayerHitByMine(int healthAmount)
    {
        eventText?.DisplayAnEvent("Mine Hit", 1);
        ChangeHealthDisplay(healthAmount);
    }

    private void PlayerHitByMissile(int healthAmount)
    {
        eventText?.DisplayAnEvent("Missile Hit", 1);
        ChangeHealthDisplay(healthAmount);
    }
    private void PlayerHitByLaser(int healthAmount)
    {
        eventText?.DisplayAnEvent("Laser Hit", 1);
        ChangeHealthDisplay(healthAmount);
    }

    private void PlayerHitByRam(int healthAmount)
    {
        eventText?.DisplayAnEvent("Ram Hit", 1);
        ChangeHealthDisplay(healthAmount);
    }

    private void ChangeHealthDisplay(int healthAmount) {
        if (healthAmount < 0) healthAmount = 0;
        healthText.text = healthAmount.ToString();
        float healthFill = ((float)healthAmount) / ((float)playerMaxHealth);
        healthBar.fillAmount = healthFill;
    }

    private void PlayerDirectlyPopped
        (int scoreIncrease, int updatedFuel)
    {
        eventText?.DisplayAnEvent("Direct Pop", 2);
        IncreaseScore(scoreIncrease);
        ChangeBoostDisplay(updatedFuel);
    }

    private void PlayerShotPopped
        (int scoreIncrease, int updatedFuel)
    {
        eventText?.DisplayAnEvent("Shot-Up", 2);
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

        //TODO medal unlock : Thrasher
        if (score >= 2000 && runtime < 60) { 

        }
        //TODO medal unlock : Endurance
        if (score >= 10000) {
            
        }
    }

    private void IncrementSpineKillCount() {
        pollenKillCount += 1;
        //TODO medal unlock : Allergic to Lasers
        if (pollenKillCount >= 10) { 
        
        }
    }

    public void GameOver() {
        Time.timeScale = 0;
        finalPanel.SetActive(true);
        finalScoreText.text = score.ToString();
        dead = true;
        restartButton.Select();
    }

    public void PauseORResume ()
    {
        if (Time.timeScale == 0)
        {
            settingsPanel.SetActive(false);
            Time.timeScale = 1;
            paused = false;
            EventSystem.current.SetSelectedGameObject(null);
        }
        else{
            Time.timeScale = 0;
            settingsPanel.SetActive(true);
            paused = true;
            pauseButton.Select();
        }
    }

    public void RestartGame() {
        SceneManager.LoadScene(0);
    }

    public void OpenKofi() {
        Application.OpenURL("https://ko-fi.com/viriuslevan");
    }

    public void ReinitPlayerScreenVariable()
    {
        player.GetComponent<Player>().ReinitializeScreenCenter();
    }

}
