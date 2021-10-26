using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour, ICrashable
{
    [SerializeField] private Animator deflationAnimator;
    //[SerializeField] private float deflationSpeed;
    [SerializeField] private GameObject mObjectGO;
    private IEnemy masterObject;
    private bool deflated=false;
    private AudioSource aSource;
    private MeshRenderer meshRenderer;
    private SphereCollider sCollider;
    private CapsuleCollider cCollider;

    public enum PopCause
    {
        RammedByPlayer, ShotByPlayer, Fratricide
    }
    public delegate void BalloonPopped(int boostValue, int score, PopCause cause);
    public static event BalloonPopped balloonPopped;

    // Start is called before the first frame update
    void Start()
    {
        aSource = this.GetComponent<AudioSource>();
        if (mObjectGO == null)
        {
            IEnemy checkMO = this.GetComponentInParent<IEnemy>();
            if (checkMO == null)
            {
                masterObject = this.GetComponent<IEnemy>();
            }
            else
            {
                masterObject = checkMO;
            }
        }
        else {
            masterObject = mObjectGO.GetComponent<IEnemy>();
        }
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
                balloonPopped?.Invoke(7,100, PopCause.RammedByPlayer);
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
        aSource?.PlayOneShot(aSource.clip, SettingsMenu.sfxVolume);
        deflationAnimator.SetBool("deflated", true);
        if (masterObject != null)
        {
            masterObject.ReduceBalloonCount();
        }
        this.transform.SetParent(null);
    }

    public void CrashedByPlayerBullet()
    {
        balloonPopped.Invoke(1,35, PopCause.ShotByPlayer);
        Crash();
    }

    //Missiles no longer pop balloons
    //public void CrashedByFratricide() {
    //    balloonPopped?.Invoke(1,40, PopCause.Fratricide);
    //    Crash();
    //}
    
}
