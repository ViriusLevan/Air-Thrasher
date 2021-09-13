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
    private SphereCollider sCollider;
    private CapsuleCollider cCollider;

    public delegate void BalloonPopped(int boostValue, int score);
    public static event BalloonPopped balloonPoppedByPCrash;
    public static event BalloonPopped balloonPoppedByPShot;

    // Start is called before the first frame update
    void Start()
    {
        aSource = this.GetComponent<AudioSource>();
        IEnemy checkMO = this.GetComponentInParent<IEnemy>();
        masterObject = (checkMO == null) ? 
            this.GetComponent<IEnemy>() :
            checkMO;
        meshRenderer = this.GetComponent<MeshRenderer>();
        sCollider = this.GetComponent<SphereCollider>();
        cCollider = this.GetComponent<CapsuleCollider>();
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (!deflated)
        {
            if (collision.gameObject.tag == "Player")
            {
                balloonPoppedByPCrash.Invoke(10,100);
                Crash();
            }
        }
    }

    public void Crash()
    {
        deflated = true;
        //Debug.Log("Deflated");
        if (meshRenderer!=null)
            meshRenderer.enabled = false;
        if(sCollider!=null)
            sCollider.enabled = false;
        if(cCollider!=null)
            cCollider.enabled = false;
        aSource?.Play();
        deflationAnimator.SetBool("deflated", true);
        if (masterObject != null)
        {
            masterObject.ReduceBalloonCount();
        }
        this.transform.SetParent(null);
    }

    public void CrashedByPlayerBullet()
    {
        balloonPoppedByPShot(3,20);
        Crash();
    }
}
