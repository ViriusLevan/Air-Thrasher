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
    private MeshRenderer meshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        aSource = this.GetComponent<AudioSource>();
        IEnemy checkMO = this.GetComponentInParent<IEnemy>();
        //use own IEnemy if it is null on parent
        masterObject = (checkMO == null) ? 
            this.GetComponent<IEnemy>() :
            checkMO;
        //TODO, might need to add a conditional?
        meshRenderer = this.GetComponent<MeshRenderer>();
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
        //TODO mesh renderer conditional
        meshRenderer.enabled = false;
        aSource?.Play();
        deflationAnimator.SetBool("deflated", true);
        if (masterObject != null)
        {
            masterObject.ReduceBalloonCount();
        }
        //this.transform.SetParent(null);
    }
}
