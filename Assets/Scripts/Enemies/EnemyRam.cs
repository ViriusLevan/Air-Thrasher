using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRam : MonoBehaviour
{
    [SerializeField] private GameObject parent;
    private float cooldown=0f;
    private void Update()
    {
        cooldown -= Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" && cooldown<=0) {
            collision.gameObject.GetComponent<Player>().EnemyRamHit(2);
            cooldown = 3f;
        } else if(TryGetComponent(out IExplodable explosive)
            && !TryGetComponent(out Pollen_Spine pSpine)
            && collision.gameObject!=parent)
        {
            explosive.Explode();
        }
    }
}
