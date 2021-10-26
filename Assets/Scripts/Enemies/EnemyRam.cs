using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRam : MonoBehaviour
{
    [SerializeField] private GameObject parent;
    private float cooldown=0f;

    public delegate void OnRamKill(Player.ScoreIncrementCause cause);
    public static event OnRamKill fratricide;

    private void Update()
    {
        cooldown -= Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && cooldown<=0) {
            collision.gameObject.GetComponent<Player>()
                .HitByEnemy(Player.HealthChangedCause.EnemyRam);
            cooldown = 3f;
        } else if(collision.gameObject.TryGetComponent(out IExplodable explosive)
            && !collision.gameObject.TryGetComponent(out Pollen_Spine pSpine)
            && collision.gameObject!=parent)
        {
            explosive.Explode();
            fratricide?.Invoke(Player.ScoreIncrementCause.FratricideRam);
        }
    }
}
