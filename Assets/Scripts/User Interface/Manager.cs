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
        healthText, finalScoreText, highScoreText, timeText, medalPoint, cruiseControlText;
    [SerializeField] private GameObject player;
    [SerializeField] private Image healthBar, boostBar;
    [SerializeField] private GameObject finalPanel;
    [SerializeField] private UI_EventText eventText;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button pauseButton, restartButton;
    [SerializeField] private NewgroundsMedals ngMedalsHandler;
    [SerializeField] private Animator achievementAnimator;
    [SerializeField] private Image achievementSprite;
    [SerializeField] private Sprite[] achievementSprites;
    [SerializeField] private AudioSource music;
    [SerializeField] private CircleSpeedIndicator circleSpeed;
    private Player currentPlayer;
    private int score, playerMaxHealth, highScore, pollenKillCount;
    private float playerMaxBoost, pauseTimer=0f, 
        pauseCooldown=0.5f, runtime=0f;
    private bool dead=false; //bruh
    private bool[] medalAlreadyUnlocked;
    public bool paused;


    public void InputPause() {
        if (pauseTimer <= 0)
        {
            if (!dead)
            {
                PauseORResume();
                pauseTimer = pauseCooldown;
            }
            else
            {
                RestartGame();
            }
        }
    }

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
    {   //TODO remove these
        //Cursor.visible = false;
        //Time.timeScale = 0;

        medalAlreadyUnlocked = new bool[3];
        music.volume = SettingsMenu.musicVolume;
        pollenKillCount = 0;
        if (player == null) { 
            player = GameObject.FindGameObjectWithTag("Player"); 
        }
        Time.timeScale = 1;
        Player.onFuelUsed += OnFuelUsed;
        Player.onHealthChanged += PlayerHealthChanged;
        Balloon.balloonPopped += BalloonPopped;
        Player.fratricide += FratricideExplosion;

        Player.playerHasDied += GameOver;
        Pollen_Spine.spineDeath += IncrementSpineKillCount;
        SettingsMenu.musicVolumeChange += ChangeMusicVolume;
        //
        NewgroundsMedals.MedalCalledback += DisplayMedalUnlock;
    }

    private void OnDestroy()
    {
        Player.onFuelUsed -= OnFuelUsed;
        Player.onHealthChanged -= PlayerHealthChanged;
        Balloon.balloonPopped -= BalloonPopped;
        Player.fratricide -= FratricideExplosion;
        Player.playerHasDied -= GameOver;
        Player.initializeUI -= InitializeUI;
        Pollen_Spine.spineDeath -= IncrementSpineKillCount;
        SettingsMenu.musicVolumeChange -= ChangeMusicVolume;
        //
        NewgroundsMedals.MedalCalledback -= DisplayMedalUnlock;
    }
    private void InitializeUI(int boost, int health)
    {
        if (player != null)
        {
            currentPlayer = player.GetComponent<Player>();
            playerMaxHealth = currentPlayer.GetMaxHealth();
            playerMaxBoost = currentPlayer.GetBoostMax();
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
        float playerSpeed = currentPlayer.GetRB().velocity.magnitude;
        speedText.text = (playerSpeed).ToString("0");
        circleSpeed.SpeedChange(playerSpeed);
        cruiseControlText.text = currentPlayer.cruiseControl.ToString();
        if (!dead) {
            runtime += Time.deltaTime;
            timeText.text = runtime.ToString("0.00");
        }
        if (pauseTimer > 0)
        {
            pauseTimer -= Time.unscaledDeltaTime;
        }
    }

    private void ChangeMusicVolume(float newVal) {
        music.volume = newVal;
    }

    private void OnFuelUsed(int updatedFuelValue, Player.FuelUsedCause cause) {
        switch (cause) {
            case Player.FuelUsedCause.Boosted:
                eventText?.DisplayAnEvent("Boosted", 0);
                ChangeBoostDisplay(updatedFuelValue);
                break;
            case Player.FuelUsedCause.FiredGun:
                eventText?.DisplayAnEvent("Fired Gun", 0);
                ChangeBoostDisplay(updatedFuelValue);
                break;
        }
    }

    private void ChangeBoostDisplay(int boostFuel) {
        float boostFill = ((float)boostFuel) / playerMaxBoost;
        boostText.text = boostFuel.ToString();
        boostBar.fillAmount = boostFill;
    }

    private void PlayerHealthChanged(int currentHealth, Player.HealthChangedCause cause) {
        switch (cause)
        {
            case Player.HealthChangedCause.EnemyMine:
                eventText?.DisplayAnEvent("Mine Hit", 1);
                break;
            case Player.HealthChangedCause.EnemyMissile:
                eventText?.DisplayAnEvent("Missile Hit", 1);
                break;
            case Player.HealthChangedCause.EnemyLaser:
                eventText?.DisplayAnEvent("Laser Hit", 1);
                break;
            case Player.HealthChangedCause.EnemyRam:
                eventText?.DisplayAnEvent("Ram Hit", 1);
                break;
        }
        ChangeHealthDisplay(currentHealth);
    }

    private void ChangeHealthDisplay(int healthAmount) {
        if (healthAmount < 0) healthAmount = 0;
        healthText.text = healthAmount.ToString();
        float healthFill = ((float)healthAmount) / ((float)playerMaxHealth);
        healthBar.fillAmount = healthFill;
    }

    private void BalloonPopped(int addedFuel, int scoreIncrease
        , Balloon.PopCause cause)
    {
        switch (cause) {
            case Balloon.PopCause.RammedByPlayer:
                eventText?.DisplayAnEvent("Direct Pop", 2);
                break;
            case Balloon.PopCause.ShotByPlayer:
                eventText?.DisplayAnEvent("Shot-Up", 2);
                break;
            //case Balloon.PopCause.Fratricide:
            //    eventText?.DisplayAnEvent("Missile Pop", 3);
            //    break;
        }
        currentPlayer.IncrementBoostFuel(addedFuel);
        IncreaseScore(scoreIncrease);
        ChangeBoostDisplay(currentPlayer.GetBoostFuel());
    }

    private void FratricideExplosion(int scoreIncrease, int updatedFuel, Player.ScoreIncrementCause cause) {
        switch (cause) {
            case Player.ScoreIncrementCause.FratricideMine:
                eventText?.DisplayAnEvent("Mine Fratricide", 3);
                IncreaseScore(scoreIncrease);
                ChangeBoostDisplay(updatedFuel);
                break;
            case Player.ScoreIncrementCause.FratricideLaser:
                eventText?.DisplayAnEvent("Laser Fratricide", 3);
                IncreaseScore(scoreIncrease);
                ChangeBoostDisplay(updatedFuel);
                break;
            case Player.ScoreIncrementCause.FratricideRam:
                eventText?.DisplayAnEvent("Ram Fratricide", 3);
                IncreaseScore(scoreIncrease);
                ChangeBoostDisplay(updatedFuel);
                break;
        }
    }

    private void IncreaseScore(int amount) {
        score += amount;
        scoreText.text = score.ToString();
        if (highScore < score) {
            highScore = score;
            PlayerPrefs.SetInt("highScore",highScore);
            highScoreText.text = highScore.ToString();
        }

        if (score >= 2000 && runtime < 60
            && !medalAlreadyUnlocked[0]) {
            if (ngMedalsHandler != null) {
                ngMedalsHandler?.unlockMedal(65275);
                medalAlreadyUnlocked[0] = true;
            }
        }
        if (score >= 10000
            && !medalAlreadyUnlocked[1]) {
            if (ngMedalsHandler != null)
            {
                ngMedalsHandler?.unlockMedal(65274);
                medalAlreadyUnlocked[1] = true;
            }
        }
    }

    private void IncrementSpineKillCount() {
        pollenKillCount += 1;
        if (pollenKillCount >= 10 
            && !!medalAlreadyUnlocked[2]) {
            if (ngMedalsHandler != null)
            {
                ngMedalsHandler?.unlockMedal(65273);
                medalAlreadyUnlocked[2] = true;
            }
        }
    }

    private void DisplayMedalUnlock(string index, int points) {
        switch (index) {
            case "Thrasher"://Thrasher
                achievementSprite.sprite = achievementSprites[0];
                break;
            case "Endurance"://Endurance
                achievementSprite.sprite = achievementSprites[1];
                break;
            default://Laser Allergy
                achievementSprite.sprite = achievementSprites[2];
                break;
        }
        medalPoint.text = points + " Points";
        StartCoroutine(DisplayCheevoMomentarily());
    }

    private IEnumerator DisplayCheevoMomentarily()
    {
        achievementAnimator.SetBool("disappear",false);
        yield return new WaitForSeconds(5f);
        achievementAnimator.SetBool("disappear", true);
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
            AudioListener.pause = false;
            EventSystem.current.SetSelectedGameObject(null);
        }
        else{
            Time.timeScale = 0;
            settingsPanel.SetActive(true);
            paused = true;
            pauseButton.Select();
            AudioListener.pause=true;
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
        currentPlayer.ReinitializeScreenCenter();
    }

}
