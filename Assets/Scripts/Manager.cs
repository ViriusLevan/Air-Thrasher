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
        healthText, finalScoreText, highScoreText, timeText, medalPoint;
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
    private int score, playerMaxHealth, highScore, pollenKillCount;
    private float playerMaxBoost, pauseTimer=0f, 
        pauseCooldown=0.5f, runtime=0f;
    private bool dead=false; //bruh
    private bool[] medalAlreadyUnlocked;
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
    {   //TODO remove this
        //Cursor.visible = false;

        medalAlreadyUnlocked = new bool[3];
        music.volume = SettingsMenu.musicVolume;
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
        Player.friendlyMissilePop += FriendlyMissilePopped;
        Player.enemyMineExplosion += ExplosionByMine;
        Player.enemyLaserExplosion += ExplosionByLaser;
        Player.enemyRamExplosion += ExplosionByRam;

        Player.playerHasDied += GameOver;
        Pollen_Spine.spineDeath += IncrementSpineKillCount;
        NewgroundsMedals.MedalCalledback += DisplayMedalUnlock;
        SettingsMenu.musicVolumeChange += ChangeMusicVolume;
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
        Player.friendlyMissilePop -= FriendlyMissilePopped;
        Player.enemyMineExplosion -= ExplosionByMine;
        Player.enemyLaserExplosion -= ExplosionByLaser;
        Player.enemyRamExplosion -= ExplosionByRam;

        Player.playerHasDied -= GameOver;
        Player.initializeUI -= InitializeUI;
        Pollen_Spine.spineDeath -= IncrementSpineKillCount;
        NewgroundsMedals.MedalCalledback -= DisplayMedalUnlock;
        SettingsMenu.musicVolumeChange -= ChangeMusicVolume;
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

    private void ChangeMusicVolume(float newVal) {
        music.volume = newVal;
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

    private void FriendlyMissilePopped
        (int scoreIncrease, int updatedFuel) {
        eventText?.DisplayAnEvent("Missile Fratricide", 3);
        IncreaseScore(scoreIncrease);
        ChangeBoostDisplay(updatedFuel);
    }

    private void ExplosionByMine(int scoreIncrease, int updatedFuel)
    {
        eventText?.DisplayAnEvent("Mine Fratricide", 3);
        IncreaseScore(scoreIncrease);
        ChangeBoostDisplay(updatedFuel);
    }

    private void ExplosionByRam(int scoreIncrease, int updatedFuel)
    {
        eventText?.DisplayAnEvent("Ram Fratricide", 3);
        IncreaseScore(scoreIncrease);
        ChangeBoostDisplay(updatedFuel);
    }

    private void ExplosionByLaser(int scoreIncrease, int updatedFuel)
    {
        eventText?.DisplayAnEvent("Laser Fratricide", 3);
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
        player.GetComponent<Player>().ReinitializeScreenCenter();
    }

}
