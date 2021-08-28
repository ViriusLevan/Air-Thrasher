using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour, ICrashable
{
    [SerializeField] private Animator deflationAnimator;
    //[SerializeField] private float deflationSpeed;
    private IEnemy masterObject;
    private bool deflated=false;
    private AudioSource aSource;

    // Start is called before the first frame update
    void Start()
    {
        aSource = this.GetComponent<AudioSource>();
        masterObject = this.GetComponentInParent<IEnemy>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!deflated)
        {
            if (collision.gameObject.tag == "Player")
            {
                deflated = true;
                collision.gameObject.GetComponent<Player>().BalloonPoppedByPlane();
                Debug.Log("Deflated");
                Crash();
            }
        }
    }

    public void Crash()
    {
        aSource?.Play();
        deflationAnimator.SetBool("deflated", true);
        if (masterObject != null)
        {
            masterObject.ReduceBalloonCount();
        }
    }
}
