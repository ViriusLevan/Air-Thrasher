using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    private Transform tr;

    // Start is called before the first frame update
    void Start()
    {
        tr = this.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        tr.Rotate(0,Time.deltaTime,0,Space.World);
    }
}
