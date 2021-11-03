using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRam : MonoBehaviour
{
    [SerializeField] private GameObject parent;
    [SerializeField] private AudioSource crashSound;
    private float cooldown=0f;

    public delegate void OnRamKill(Player.ScoreIncrementCause cause);
    public static event OnRamKill fratricide;

    private void Update()
    {
        if (Time.timeScale == 0) return;
        cooldown -= Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && cooldown<=0) {
            collision.gameObject.GetComponent<Player>()
                .HitByEnemy(Player.HealthChangedCause.EnemyRam);
            cooldown = 3f;
            crashSound.volume = SettingsMenu.sfxVolume;
            crashSound.Play();
        } 
        //else if(collision.gameObject.TryGetComponent(out BalloonEnemy explosive)
        //    && !collision.gameObject.TryGetComponent(out Pollen_Spine pSpine)
        //    && collision.gameObject!=parent)
        //{
        //    explosive.Explode();
        //    fratricide?.Invoke(Player.ScoreIncrementCause.FratricideRam);
        //}
    }
}
