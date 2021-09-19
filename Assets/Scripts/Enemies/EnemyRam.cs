using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRam : MonoBehaviour
{
    [SerializeField] private GameObject parent;
    private float cooldown=0f;

    public delegate void OnRamKill();
    public static event OnRamKill ramExplosion;

    private void Update()
    {
        cooldown -= Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && cooldown<=0) {
            collision.gameObject.GetComponent<Player>().EnemyRamHit(2);
            cooldown = 3f;
        } else if(collision.gameObject.TryGetComponent(out IExplodable explosive)
            && !collision.gameObject.TryGetComponent(out Pollen_Spine pSpine)
            && collision.gameObject!=parent)
        {
            explosive.Explode();
            ramExplosion?.Invoke();
        }
    }
}
