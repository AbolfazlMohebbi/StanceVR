using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.VR;

public class CamRotation1 : MonoBehaviour {

    public HeadTrackingInfo trackingInfo;
    Camera camVR;
    private float x_cam, y_cam, z_cam, xEuler, yEuler, zEuler, OriX;
    
    private int flagExpNum;  // 0: Normal VR  1: Disabled Head-tracking  2: Magnify Rotation  3: Step-response (5 and 10 deg.) 4: Sinusoidal Perturbations
    private int timeFrame, Stepflag;
    //SteamVR_TestTrackedCamera VRcam;
    //SteamVR_Camera VRcam;

    // Use this for initialization
    void Start() {
        x_cam = 0.0f;
        y_cam = 0.0f;
        z_cam = 0.0f;
        xEuler = 0.0f;
        yEuler = 0.0f;
        zEuler = 0.0f;
        //OriX = 0.0f;
        OriX = trackingInfo.headOriX;
        timeFrame = 0;
        Stepflag = 0;
        //print("headOriX: " + transform.rotation.eulerAngles.x);
        //VRcam = gameObject.GetComponent<SteamVR_Camera>();
        //transform.rotation = Quaternion.Inverse(InputTracking.GetLocalRotation(VRNode.CenterEye));
        //transform.Rotate(new Vector3(0, -90, 0));
        //Quaternion Q1;
        //Q1 = InputTracking.GetLocalRotation(VRNode.CenterEye);
        //print("Q1" + Q1);
    }

    // Update is called once per frame
    void Update() {

        timeFrame = timeFrame + 1;

        if (Time.time > 10.0f)  //Perturbation wait time
        {
            flagExpNum = 4;  // 0: Normal VR  1: Disabled Head-tracking  2: Magnify Rotation  3: Step-response (5 and 10 deg.) 4: Sinusoidal Perturbations     

            switch (flagExpNum)
            {
                case 0: // No purturbation
                    {
                        break;
                    }

                case 1:   //head tracking disabled
                    {
                        transform.rotation = Quaternion.Inverse(InputTracking.GetLocalRotation(VRNode.CenterEye));
                        //transform.position = -InputTracking.GetLocalPosition(VRNode.CenterEye);
                        //InputTracking.disablePositionalTracking = true;
                        //transform.rotation = Quaternion.Inverse(InputTracking.GetLocalRotation(VRNode.));
                        //transform.Rotate(new Vector3(0, -90, 0));        
                        


                        break;
                    }

                case 2:  // Magnify Motion
                    {
                        xEuler = OriX;
                        OriX = trackingInfo.headOriX;
                        xEuler = OriX - xEuler;
                        //yEuler = trackingInfo.headOriY;
                        //zEuler = trackingInfo.headOriZ;
                        if (xEuler > 180) xEuler = xEuler - 360;
                        if (xEuler < -180) xEuler = xEuler + 360; 
                        print("xEuler: " + xEuler);
                        print("yEuler: " + yEuler);
                        print("zEuler: " + zEuler);
                        transform.Rotate(new Vector3(0.6f*xEuler, yEuler, zEuler));

                        break;
                    }

                case 3: // Step-response (5 and 10 deg.)
                    {                                                
                        float stepAmp = ToRadian(15.0f);
                        Stepflag = Stepflag + 1;
                        print("Step: " + Stepflag);
                        if (Stepflag>5 && Stepflag<15)                        
                            { 
                            xEuler = -stepAmp;
                            transform.Rotate(new Vector3(xEuler, yEuler, zEuler));
                            }
                        if (Stepflag>25 && Stepflag<35)
                            {
                            xEuler = stepAmp;
                            transform.Rotate(new Vector3(xEuler, yEuler, zEuler));
                            }
                        
                        break;
                        
                    }

                case 4: //Sinusoidal Perturbations
                    {
                        float degAmplitude = ToRadian(15.0f); //In degrees  
                        float degSpeed = 2 * (3.14f / 10.0f); //full cycle in 20 sec
                        //xEuler = degAmplitude * (float)Math.Sin(ToRadian(degSpeed * Time.time));
                        xEuler = degAmplitude * (3.14f / 10.0f) * (float)Math.Sin(degSpeed * Time.time);
                        print("Angle: " + xEuler);                        
                        print("Time:" + Time.time);
                        transform.Rotate(new Vector3(xEuler, yEuler, zEuler));
                        break;
                    }
            }

        }
    }	
	        public float ToDegree(float theta)
        {
            float x = theta * (180.0f / 3.14f);
            return x;
        }

        public float ToRadian(float xx)
        {
            float theta = xx * (3.14f / 180.0f);
            return theta;
        }
	
}