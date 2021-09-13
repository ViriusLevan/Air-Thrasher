using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingProjectile : MonoBehaviour, IExplodable
{
    [SerializeField] private Transform target;
    [SerializeField] private float force;
    [SerializeField] private float rotationForce;
    [SerializeField] private float secondsBeforeExplosion;
    [SerializeField] private float secondsBeforeHoming;
    private Rigidbody rb;
    private bool shouldFollow = false;
    [SerializeField] private GameObject explosionPrefab;

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        StartCoroutine(WaitBeforeHoming());

        if (target == null) {
            GameObject nCheck = GameObject.FindGameObjectWithTag("Player");
            if (nCheck == null)
            {
                Explode();
            }
            else {
                target = nCheck.transform;
            }
        }
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
        if (shouldFollow)
        {
            if (target != null)
            {
                Vector3 direction = target.position - rb.position; //getting direction
                direction.Normalize(); //erasing magnitude
                Vector3 rotationAmount = Vector3.Cross(transform.forward, direction);//calculating angle
                rb.angularVelocity = rotationAmount * rotationForce;
                rb.velocity = transform.forward * force;
            }
            else {
                rb.velocity = new Vector3(0,0,0);
                rb.angularVelocity = new Vector3(0, 0, 0);
                Explode();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<Player>().EnemyMissileHit(1);
            Explode();
        }
    }

    private IEnumerator WaitBeforeHoming() {
        rb.AddForce(transform.forward * force, ForceMode.Impulse);
        yield return new WaitForSeconds(secondsBeforeHoming);
        shouldFollow = true;
    }

    public void Explode()
    {
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        Destroy(this.gameObject);
    }
}
