using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapFollow : MonoBehaviour
{

    [SerializeField] private Transform objFollow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newCoord = objFollow.position;
        newCoord.y = this.transform.position.y;
        this.transform.position = newCoord;
    }
}
