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
    private float pi;

    // Magnified Motion Vars:
    private float MagnifyOri;

    // Step Response Vars:
    private float stepAmpilitude;
    private int stepStartTimeMS, stepStartTimeFrame, stepEndTimeFrame, stepStartTimeFramePrevious, stepTimeFrame, stepDurationMS, stepTimeFrameDuration, stepSteadyStateTimeFrame, stepSteadyStateTimeMS;

    // Sinosoidal Vars:
    private float Amplitude, frequency, RotationVelocity;

    //PRBS Vars:
    private float prbsAmpilitude;
    private int prbsTimeFrameDuration, prbsDurationMS;
    private int iCount;
    private int PrbsElementInt, previousPrbsElementInt;

    private int flagExpNum;  // 0: Normal VR  1: Disabled Head-tracking  2: Magnify Rotation  3: Step-response (5 and 10 deg.) 4: Sinusoidal Perturbations

    private static System.Random rndGen;

    float tTime, tTimeMinus1;

    // Use this for initialization
    void Start()
    {
        MCCDAQwrap.flashLED();
        MCCDAQwrap.writeVolts(1, 0.0f);  // Set all voltages to zero

        //Using Random Class
        rndGen = new System.Random();

        tTime = 0.0f;
        xEuler = 0.0f;
        yEuler = 0.0f;
        zEuler = 0.0f;
        deltaXori = 0.0f;
        iCount = 0;
        prbsRes = PRBS_General();
        startRot = transform.rotation;
        Counter = 0;
        toRad = (3.14f / 180.0f);
        toDeg = (180.0f / 3.14f);
        pi = (float)Math.PI;

        // ******************  EXPERIMENTS SETUP  ******************
        flagExpNum = 4;  // 0: Normal VR  1: Disabled Head-tracking  2: Magnify Rotation  3: Step-response (5 and 10 deg.) 4: Sinusoidal Perturbations  5: PRBS  6: Randomized Step
        Time_0 = 4.0f;   //Start perturbations after this time seconds


        // ******************  STEP RESPONSE PARAMETERS ******************
        stepTimeFrame = 0;
        stepStartTimeFramePrevious = 0;

        // each frame = 16ms or 0.016 sec
        stepStartTimeMS = rndGen.Next(0, 4000);   // Step happens at a random time between 0 to 4000 miliseconds
        stepStartTimeFrame = stepStartTimeMS/16; //miliseconds
        print("stepStartTimeFrame is:  " + stepStartTimeFrame + "    and stepStartTimeMS is:  " + stepStartTimeMS);
        stepAmpilitude = 10.0f; //Degrees (5 - 10 - 15 deg.)

        stepDurationMS = 160; //miliseconds
        stepTimeFrameDuration = stepDurationMS/16; //frames
        stepSteadyStateTimeMS = 2000; //miliseconds
        stepSteadyStateTimeFrame = stepSteadyStateTimeMS/16;

        // ******************  SINUSOIDAL PARAMETERS  ********************
        Amplitude = 15.0f;
        RotationVelocity = 2;    // A.Sin(wt) w = degree per second
        frequency = (2 * pi) / RotationVelocity;      //v = 2pi/w   Hz

        // ********************  PRBS PARAMETERS  ************************
        prbsAmpilitude = 2.0f; //Degrees (5 - 10 - 15 deg.)
        prbsDurationMS = 200;  //miliseconds
        prbsTimeFrameDuration = prbsDurationMS/16;

    }


    // Update is called once per frame
    void Update() {
        
        tTime = Time.time;
        if (tTime > Time_0)  //Perturbation wait time
        {
            //MCCDAQwrap.writeVolts(1, 5.0f);   // Send start vision perturbation signal of 5v to channel 1. 

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
                        // each frame = 16ms or 0.016 sec
                        stepTimeFrame = stepTimeFrame + 1;

                        if (stepTimeFrame == stepStartTimeFrame)
                        {
                            transform.Rotate(new Vector3(-stepAmpilitude, yEuler, zEuler));
                            MCCDAQwrap.writeVolts(1, 5.0f);
                        }

                        if (stepTimeFrame == stepStartTimeFrame + stepTimeFrameDuration)
                        {
                            transform.Rotate(new Vector3(stepAmpilitude, yEuler, zEuler));
                            MCCDAQwrap.writeVolts(1, 0.0f);
                        }
                        break;
                    }

                case 4: //Sinusoidal Perturbations
                    {

                        if (Counter == 0)
                        {
                            xEulerInit = trackingInfo.headOriX;
                            xEulerPrevious = xEulerInit;
                        }

                        xEuler = xEulerInit + (Amplitude * (float)Math.Sin(RotationVelocity * (Time.time - Time_0)));
                        MCCDAQwrap.writeVolts(1, 2.5f+ 2.5f * (float)Math.Sin(RotationVelocity * (Time.time - Time_0)));

                        deltaXori = (xEuler - xEulerPrevious);
                        if (deltaXori > 180) deltaXori = deltaXori - 360;
                        if (deltaXori < -180) deltaXori = deltaXori + 360;
                        transform.Rotate(new Vector3( deltaXori,  yEuler, zEuler));
                        xEulerPrevious = xEuler;

                        Counter = Counter + 1;
                        break;
                    }

                case 5: //PRBS
                    {
                        Counter = Counter + 1;
                        
                        if (Counter % prbsTimeFrameDuration == 0)  //done at each prbsTimeFrameDuration
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
                                        MCCDAQwrap.writeVolts(1, 5.0f);
                                    }
                                }

                                if (PrbsElementInt == 0)
                                {
                                    if (previousPrbsElementInt == 1)
                                    {
                                        transform.Rotate(new Vector3(prbsAmpilitude, yEuler, zEuler));
                                        MCCDAQwrap.writeVolts(1, 0.0f);
                                    }
                                }
                                previousPrbsElementInt = PrbsElementInt;
                                iCount = iCount + 1;
                            }
                        }
                        break;
                    }

                case 6: // Randomized Step-response (5 - 10 - 15 deg.)
                    {
                        // each frame = 16ms or 0.016 sec
                        stepTimeFrame = stepTimeFrame + 1;
                        if (stepTimeFrame == stepStartTimeFrame)
                        {
                            transform.Rotate(new Vector3(-stepAmpilitude, yEuler, zEuler));
                            MCCDAQwrap.writeVolts(1, 5.0f);
                            stepStartTimeFrame = stepStartTimeFramePrevious + stepTimeFrameDuration + stepSteadyStateTimeFrame + rndGen.Next(0, stepSteadyStateTimeFrame);
                            print("stepStartTimeFrame" + stepStartTimeFrame);
                        }

                        if (stepTimeFrame == stepStartTimeFramePrevious + stepTimeFrameDuration)
                        {
                            transform.Rotate(new Vector3(stepAmpilitude, yEuler, zEuler));
                            MCCDAQwrap.writeVolts(1, 0.0f);
                            stepStartTimeFramePrevious = stepStartTimeFrame;
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
        var prbs_buf = "";

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

        prbs_buf = prbs;
        for (int i=1; i<10; i++)
        {
            prbs = prbs + prbs_buf;
        }

        print("period = " + period);
        print("prbs = " + prbs);
        return prbs;

    }



}