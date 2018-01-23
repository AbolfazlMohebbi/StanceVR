using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadTrackingInfo : MonoBehaviour {
    public float headOriX, headOriY, headOriZ;

    // Use this for initialization
    void Start () {
        headOriX = 0.0f;
        headOriY = 0.0f;
        headOriZ = 0.0f;


    }
	
	// Update is called once per frame
	void Update () {
        
        headOriX = transform.eulerAngles.x;
        headOriY = transform.eulerAngles.y;
        headOriZ = transform.eulerAngles.z;

        /*        
        headOriX = transform.rotation.eulerAngles.x;        
        headOriY = transform.rotation.eulerAngles.y;
        headOriZ = transform.rotation.eulerAngles.z;      

        print("HeadOriX: " + headOriX);
        print("HeadOriY: " + headOriY);
        print("HeadOriZ: " + headOriZ);
        */

    }
}
