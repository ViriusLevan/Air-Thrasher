using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BargeBullet : MonoBehaviour, IExplodable
{
    [SerializeField] private float force;
    [SerializeField] private float secondsBeforeExplosion;
    private Rigidbody rb;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionForce;
    [SerializeField] private GameObject smokePrefab;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        secondsBeforeExplosion -= Time.deltaTime;
        if (secondsBeforeExplosion <= 0)
        {
            Explode();
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = transform.forward * force;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag.Equals("Player"))
            Explode();
    }

    public void Explode()
    {
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        Instantiate(smokePrefab, transform.position, transform.rotation);

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
}
