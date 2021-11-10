using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AirThrasher.Assets.Scripts.Player
{
    public class EnemyLaser : MonoBehaviour
    {
        private float countUp = 0f;
        [SerializeField] private GameObject parent;

        public delegate void OnLaserKill(Player.ScoreIncrementCause cause);
        public static event OnLaserKill fratricide;

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                countUp += Time.deltaTime;
                if (countUp >= 0.35f)
                {
                    countUp -= 0.35f;
                    other.gameObject.
                        GetComponent<Player>().HitByEnemy(Player.HealthChangedCause.EnemyLaser);
                }
            }
            else if (other.gameObject.TryGetComponent(out HomingProjectile homing))
            {
                homing?.Explode();
            }
            //else if (other.gameObject != parent
            //    && other.gameObject.TryGetComponent(out BalloonEnemy explosive)
            //    && !other.gameObject.TryGetComponent(out Pollen_Spine pSpine))
            //{

            //    explosive?.Explode();//explodes every BalloonEnemy but a fellow pollenSpine
            //    fratricide?.Invoke(Player.ScoreIncrementCause.FratricideLaser);
            //}
        }
    }
}