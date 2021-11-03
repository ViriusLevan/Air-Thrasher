using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BalloonEnemy : MonoBehaviour
{
    [Header("Float Height Settings")]
    [SerializeField] protected float upwardFloatForce, restingFloatForce;
    protected ConstantForce floatForce;

    [Header("Self Explosion and Death")]
    [SerializeField] protected GameObject explosionPrefab;
    [SerializeField] protected float explosionRadius
        ,explosionForce, explosionTimer,numberOfBalloons;
    protected bool dead = false;

    [Header("Follow")]
    [SerializeField] protected Transform target;
    [SerializeField] protected float rotationForce, force;
    protected Rigidbody rb;

    public enum BalloonEnemyType { 
        HotAirBalloon, Blimp, Powell, Barge, PollenSpine, PollenChild
    }
    public delegate void OnBalloonEnemyDeath(BalloonEnemyType type);
    public static event OnBalloonEnemyDeath enemyDeath;

    protected void Move()
    {
        rb.AddForce(transform.forward * force);
        //Debug.Log(transform.forward+"->"+rb.velocity);
    }

    protected void Turn()
    {
        //Rotating forward transform to direction of the player
        Vector3 direction = target.position - rb.position;
        direction.Normalize();
        Vector3 rotateToPlayer = Vector3.Cross(transform.forward, direction);
        rotateToPlayer = Vector3.Project(rotateToPlayer, transform.up);
        rb.AddRelativeTorque(rotateToPlayer * rotationForce * 5);

        Balance();
    }

    protected void Balance() {
        //Local up to World Up
        Vector3 predictedUp = Quaternion.AngleAxis(
             rb.angularVelocity.magnitude * Mathf.Rad2Deg * rotationForce / force,
             rb.angularVelocity
         ) * transform.up;
        Vector3 uprightBalance = Vector3.Cross(predictedUp, Vector3.up);
        rb.AddTorque(uprightBalance * rotationForce * 3);
    }

    protected void AdjustFloat()
    {
        //Float height target
        if (this.transform.position.y >= target.position.y)
        {
            floatForce.force = new Vector3(0, restingFloatForce, 0);
        }
        else
        {
            floatForce.force = new Vector3(0, upwardFloatForce, 0);
        }
    }

    public void Explode()
    {
        //instantiate effect
        Instantiate(explosionPrefab, transform.position, transform.rotation);

        //apply force to nearby rigidbodies with colliders
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(
                    explosionForce, transform.position, explosionRadius);
            }
        }
        Destroy(this.gameObject);
    }

    public void ReduceBalloonCount()
    {
        numberOfBalloons -= 1;
        if (numberOfBalloons <= 0)
        {
            floatForce.force = new Vector3(0, 0, 0);
            dead = true;
        }
    }

    protected void InvokeDeathEvent(BalloonEnemyType type) {
        enemyDeath?.Invoke(type);
        EnemySpawner.enemyNumber -= 1;
    }
}
