using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
    private float countUp=0f;
    [SerializeField]private GameObject parent;

    public delegate void OnLaserKill();
    public static event OnLaserKill laserExplosion;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject != parent
            && other.gameObject.TryGetComponent(out IExplodable explosive)
            && !other.gameObject.TryGetComponent(out Pollen_Spine pSpine)) {
            explosive.Explode();//explodes every explodable but a fellow pollenSpine
            laserExplosion?.Invoke();
        }else if (other.gameObject.tag == "Player") {
            countUp += Time.deltaTime;
            if (countUp >= 0.35f)
            {
                countUp -= 0.35f;
                other.gameObject.
                    GetComponent<Player>().EnemyLaserHit(1);
            }
        }
    }
}
