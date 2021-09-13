using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField]private float forwardSpeed;
    private Rigidbody rb;
    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionForce;
    [SerializeField] private float timeBeforeExplosion;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        rb.velocity = (transform.forward * forwardSpeed);
    }

    private void Update()
    {
        timeBeforeExplosion -= Time.deltaTime;
        if (timeBeforeExplosion <= 0) { Explode(); }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Balloon ie)) {
            ie.CrashedByPlayerBullet();
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    public void Explode()
    {
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

            if (TryGetComponent(out Balloon insBalloon)) {
                insBalloon.CrashedByPlayerBullet();
            }
        }
        Destroy(this.gameObject);
    }
}
