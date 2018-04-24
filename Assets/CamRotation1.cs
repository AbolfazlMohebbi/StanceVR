using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using System;
using System.IO;
using System.Linq;


public class CamRotation1 : MonoBehaviour {

    public HeadTrackingInfo trackingInfo;
    private Quaternion startRot;
    private float xEuler, yEuler, zEuler, deltaXori, deltaXori_init, xEulerPrevious, xEulerInit;
    private float Time_0;
    private float toRad, toDeg;
    private int Counter;
    private string prbsRes;

    // Magnified Motion Vars:
    private float MagnifyOri;

    // Step Response Vars:
    private float stepAmpilitude;
    private int stepStartTimeFrame, stepEndTimeFrame, stepTimeFrame;

    // Sinosoidal Vars:
    private float Amplitude, frequency;

    //PRBS Vars:
    private float prbsAmpilitude;
    private int prbsTimeFrame;
    private int iCount;
    private int PrbsElementInt, previousPrbsElementInt;

    private int flagExpNum;  // 0: Normal VR  1: Disabled Head-tracking  2: Magnify Rotation  3: Step-response (5 and 10 deg.) 4: Sinusoidal Perturbations


    // Use this for initialization
    void Start()
    {
        MCCDAQwrap.flashLED();
        MCCDAQwrap.writeVolts(1, 0.0f);  // Set all voltages to zero

        xEuler = 0.0f;
        yEuler = 0.0f;
        zEuler = 0.0f;
        deltaXori = 0.0f;

        stepTimeFrame = 0;
        flagExpNum = 2;  // 0: Normal VR  1: Disabled Head-tracking  2: Magnify Rotation  3: Step-response (5 and 10 deg.) 4: Sinusoidal Perturbations  5: PRBS
        Time_0 = 4.0f;   //Start perturbations after this time seconds

        startRot = transform.rotation;
        
        Counter = 0;

        toRad = (3.14f / 180.0f);
        toDeg = (180.0f / 3.14f);

        prbsRes = PRBS_General();
        iCount = 0;
    }


    // Update is called once per frame
    void Update() {

        if (Time.time > Time_0)  //Perturbation wait time
        {
            MCCDAQwrap.writeVolts(1, 5.0f);   // Send start vision perturbation signal of 5v. 

            switch (flagExpNum)
            {
                case 0: // No purturbation
                    {
                        break;
                    }

                case 1:   //head tracking disabled
                    {
                        transform.rotation = startRot * Quaternion.Inverse(InputTracking.GetLocalRotation(VRNode.CenterEye));
                        break;
                    }

                case 2:  // Magnify Motion
                    {
                        MagnifyOri = 1.5f;     // Magnifying Factor. When MagnifyOri = 1.0f, No Magnification. 

                        xEuler = trackingInfo.headOriX;
                        if (Counter == 0)
                        {
                            xEulerPrevious = trackingInfo.headOriX;
                            deltaXori_init = xEuler - xEulerPrevious;
                            if (deltaXori_init > 180)  deltaXori_init = deltaXori_init - 360;
                            if (deltaXori_init < -180) deltaXori_init = deltaXori_init + 360;
                        }

                        deltaXori = xEuler - xEulerPrevious;
                        if (deltaXori > 180) deltaXori = deltaXori - 360;
                        if (deltaXori <-180) deltaXori = deltaXori + 360;
                        transform.Rotate(new Vector3( (MagnifyOri - 1.0f) * deltaXori, (MagnifyOri - 1.0f) * yEuler, (MagnifyOri - 1.0f) * zEuler));
                        xEulerPrevious = xEuler;

                        //print("   xEuler: " + xEuler + "   yEuler: " + yEuler + "   zEuler: " + zEuler);
                        //print("   MagnifyOri * deltaXori: " + (MagnifyOri * deltaXori));
                        //print("   deltaXori_init:  " + deltaXori_init);

                        Counter = Counter + 1;
                        break;
                    }

                case 3: // Step-response (5 - 10 - 15 deg.)
                    {
                        stepTimeFrame = stepTimeFrame + 1;
                        stepAmpilitude = 10.0f; //Degrees
                        stepStartTimeFrame = 5;
                        stepEndTimeFrame = 25;

                        if (stepTimeFrame == stepStartTimeFrame)
                        {
                            transform.Rotate(new Vector3(-stepAmpilitude, yEuler, zEuler));
                        }

                        if (stepTimeFrame == stepEndTimeFrame)
                        {
                            transform.Rotate(new Vector3(stepAmpilitude, yEuler, zEuler));
                        }
                        break;
                    }

                case 4: //Sinusoidal Perturbations
                    {
                        Amplitude = 15.0f;
                        frequency = 2;    // degree per second
                        
                        if (Counter == 0)
                        {
                            xEulerInit = trackingInfo.headOriX;
                            xEulerPrevious = xEulerInit;
                        }

                        xEuler = xEulerInit + (Amplitude * (float)Math.Sin(frequency * (Time.time - Time_0)));

                        deltaXori = (xEuler - xEulerPrevious);
                        if (deltaXori > 180) deltaXori = deltaXori - 360;
                        if (deltaXori < -180) deltaXori = deltaXori + 360;
                        transform.Rotate(new Vector3( deltaXori,  yEuler, zEuler));
                        xEulerPrevious = xEuler;

                        ////MCCDAQ.writeWaveForm(4, 1, 5.0f, 0.1f, Time.time - Time_0);

                        Counter = Counter + 1;
                        break;
                    }

                case 5: //PRBS
                    {
                        Counter = Counter + 1;
                        prbsAmpilitude = 10.0f; //Degrees
                        prbsTimeFrame = 10;

                        if (Counter % prbsTimeFrame == 0)  //done at each prbsTimeFrame
                        {
                            if (iCount < prbsRes.Length)
                            {
                                char prbsElement = prbsRes[iCount];
                                PrbsElementInt = Convert.ToInt32(new string(prbsElement, 1));
                                print("PrbsElementInt:  " + PrbsElementInt);

                                if (PrbsElementInt == 1)
                                {
                                    if (previousPrbsElementInt == 0)
                                    {
                                        transform.Rotate(new Vector3(-prbsAmpilitude, yEuler, zEuler));
                                    }
                                }

                                if (PrbsElementInt == 0)
                                {
                                    if (previousPrbsElementInt == 1)
                                    {
                                        transform.Rotate(new Vector3(prbsAmpilitude, yEuler, zEuler));
                                    }
                                }
                                previousPrbsElementInt = PrbsElementInt;
                                iCount = iCount + 1;
                            }
                        }
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

    public string PRBS_General()
    {
        var polynomial = "1100000";
        //"1100000" = x^7+x^6+1
        //"10100" = x^5+x^3+1
        //"110" = x^3+x^2+1
        var start_state = 0x1;  /* Any nonzero start state will work. */

        var taps = Convert.ToInt32(polynomial, 2);
        var lfsr = start_state;
        var period = 0;
        var prbs = "";

        do
        {
            var lsb = lfsr & 1;  /* Get LSB (i.e., the output bit). */
            prbs = prbs + lsb;
            lfsr >>= 1;          /* Shift register */
            if (lsb == 1)
            {      /* Only apply toggle mask if output bit is 1. */
                lfsr ^= taps;      /* Apply toggle mask, value has 1 at bits corresponding to taps, 0 elsewhere. */
            }
            ++period;
        } while (lfsr != start_state);
        print("period = " + period);
        print("prbs = " + prbs);

        //var prbsInt = Convert.ToInt32(prbs, 2);
        //print("prbsInt =   " + prbsInt);

        if (period == Math.Pow(2, polynomial.Length) - 1)
        {
            print("polynomial is maximal length");
        }
        else
        {
            print("polynomial is not maximal length");
        }

        return prbs;

    }



}