using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BargeBullet : MonoBehaviour, IExplodable
{
    [SerializeField] private Transform target;
    [SerializeField] private float force;
    [SerializeField] private float rotationForce;
    [SerializeField] private float secondsBeforeExplosion;
    [SerializeField] private float secondsBeforeHoming;
    private Rigidbody rb;
    private bool shouldFollow = false;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionForce;
    [SerializeField] private GameObject smokePrefab;
    private Vector3 randomPointModifier;
    
    // Start is called before the first frame update
    void Start()
    {
        randomPointModifier = 
            new Vector3(Random.Range(0,2f), Random.Range(0, 2f), Random.Range(0, 2f));
        rb = this.GetComponent<Rigidbody>();
        StartCoroutine(WaitBeforeHoming());

        if (target == null)
        {
            GameObject nCheck = GameObject.FindGameObjectWithTag("Player");
            if (nCheck == null)
            {
                Explode();
            }
            else
            {
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
                Vector3 direction = target.position + randomPointModifier - rb.position; //getting direction
                direction.Normalize(); //erasing magnitude
                Vector3 rotationAmount = Vector3.Cross(transform.forward, direction);//calculating angle
                rb.angularVelocity = rotationAmount * rotationForce;
                rb.velocity = transform.forward * force;
            }
            else
            {
                rb.velocity = new Vector3(0, 0, 0);
                rb.angularVelocity = new Vector3(0, 0, 0);
                Explode();
            }
        }
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
        Instantiate(smokePrefab, transform.position, transform.rotation);
        Destroy(this.gameObject);
    }
    private IEnumerator WaitBeforeHoming()
    {
        rb.AddForce(transform.forward * force, ForceMode.Impulse);
        yield return new WaitForSeconds(secondsBeforeHoming);
        shouldFollow = true;
    }
}
