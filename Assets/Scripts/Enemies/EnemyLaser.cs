using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
    private float countUp=0f;
    [SerializeField]private GameObject parent;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject != parent 
            && TryGetComponent(out IExplodable explosive) 
            && !TryGetComponent(out Pollen_Spine pSpine)) {
            explosive.Explode();//explodes every explodable but a fellow pollenSpine
        }else if (other.gameObject.tag == "Player") {
            countUp += Time.deltaTime;
            if (countUp >= 0.5f)
            {
                countUp -= 0.5f;
                other.gameObject.GetComponent<Player>().EnemyLaserHit(1);
            }
        }
    }
}
