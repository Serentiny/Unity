using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOption : MonoBehaviour
{
    public GameObject player;
    public Canvas canvasDebug, canvasLogo, canvasAutocalibration;
    public UnityEngine.UI.Text fps;
    public UnityEngine.UI.Text textGyroscopeAvailable, textGyroscopeX, textGyroscopeY, textGyroscopeZ;
    public UnityEngine.UI.Text textCompassAvailable, textCompassRawX, textCompassRawY, textCompassRawZ, textCompassAngle;
    public UnityEngine.UI.Text textAccelerationAvailable, textAccelerationX, textAccelerationY, textAccelerationZ;
    public UnityEngine.UI.Text textHorizontalAngle, textVerticalAngle, textSpinAngle;

    // Flags for working with rotating
    bool isGyroscope = false, isCompass = false, isAcceleration = false;

    // Time counters for FPS and DoubleTap for smartphones
    float fpsDeltaTime = 0.0f, touchLastTime = 0.0f;
    int touchCounter = 0;

    // Current angles of camera
    float curAngleHorizontal = 0.0f, curAngleVertical = 0.0f, curAngleSpin = 0.0f;

    // Variables for calibration gyroscope
    bool isCalibrated = false, isCalm = true, isJerky = false;
    float timeFromEndJerky = 0.0f;

    void Start()
    {
        //Never turn off the screen
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //Turn on sensors
        Input.location.Start();
        Input.compass.enabled = true;
        Input.gyro.enabled = true;

        Init();
    }
    void Init()
    {
        canvasDebug.gameObject.SetActive(false);
        canvasAutocalibration.gameObject.SetActive(false);
        StartCoroutine(ShowLogo());
    }

    void Update()
    {
        //Getting current time for fps lable
        fpsDeltaTime += (Time.deltaTime - fpsDeltaTime) * 0.1f;

        //Get pressed key Escape for return or exit
        if (Input.GetKeyDown(KeyCode.Escape))
            PressEscape();

        //Get pressed key Menu or Double tap for showing or hide menu
        if (Input.GetKeyDown(KeyCode.Menu) || touchCounter >= 2)
            PressMenu();

        //Update the values in debug menu
        if (canvasDebug.gameObject.activeSelf)
            MenuDebugUpdate();

        //Rotating camera with the smartphone
        CameraRotating();

        //Reset touch counter after small delay or after double click
        if (Time.realtimeSinceStartup - touchLastTime > 0.25f || touchCounter >= 2)
            touchCounter = 0;

        //Count touches with interval lesser than small delay
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchLastTime = Time.realtimeSinceStartup;
            if (touch.phase == TouchPhase.Ended)
                touchCounter += 1;
        }
    }

    void PressEscape()
    {
        if (canvasDebug.gameObject.activeSelf)
            canvasDebug.gameObject.SetActive(false);
        else
            Application.Quit();
    }
    void PressMenu()
    {
        if (canvasDebug.gameObject.activeSelf)
            canvasDebug.gameObject.SetActive(false);
        else
            canvasDebug.gameObject.SetActive(true);
    }

    void MenuDebugUpdate()
    {
        fps.text = (1.0f / fpsDeltaTime).ToString("00.0 ");

        textAccelerationX.text = Input.acceleration.x.ToString(" 0.000;-0.000");
        textAccelerationY.text = Input.acceleration.y.ToString(" 0.000;-0.000");
        textAccelerationZ.text = Input.acceleration.z.ToString(" 0.000;-0.000");

        textGyroscopeX.text = Input.gyro.attitude.x.ToString(" 00.000;-00.000");
        textGyroscopeY.text = Input.gyro.attitude.y.ToString(" 00.000;-00.000");
        textGyroscopeZ.text = Input.gyro.attitude.z.ToString(" 00.000;-00.000");

        textCompassRawX.text = Input.compass.rawVector.x.ToString(" 000;-000");
        textCompassRawY.text = Input.compass.rawVector.y.ToString(" 000;-000");
        textCompassRawZ.text = Input.compass.rawVector.z.ToString(" 000;-000");
        textCompassAngle.text = Input.compass.magneticHeading.ToString(" 000.00");

        textVerticalAngle.text = curAngleVertical.ToString(" 000.00;-000.00");
        textHorizontalAngle.text = curAngleHorizontal.ToString(" 000.00;-000.00");
        textSpinAngle.text = curAngleSpin.ToString(" 000.00;-000.00");
    }

    void CameraRotating()
    {
        if (isGyroscope && isCompass && isAcceleration)
        {
            CheckRotateSpeed();

            if (!isCalibrated && isCalm)
                Calibration();

            curAngleHorizontal = -Input.gyro.rotationRateUnbiased.y;
            curAngleVertical   = -Input.gyro.rotationRateUnbiased.x;
            curAngleSpin       =  Input.gyro.rotationRateUnbiased.z;

            player.transform.Rotate(curAngleVertical, curAngleHorizontal, curAngleSpin);
        }
        else if (isGyroscope)
        {
            // Similar to above but with manual calibration
        }
        else if (isCompass && isAcceleration)
        {
            curAngleHorizontal = Input.compass.magneticHeading;
            curAngleVertical   = Input.acceleration.z * -90;
            curAngleSpin       = Input.acceleration.x * -90;

            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, Quaternion.Euler(curAngleVertical, curAngleHorizontal, curAngleSpin), Time.deltaTime);
        }
        else
        {
            // You are looser =)
            // I do not want to check (GC) and (GA) choices
            // I do not want to check ( C) and ( A) choices
        }
    }

    float QueueMedian(ref Queue<float> q)
    {
        float s = 0;
        foreach (float i in q)
            s += i;
        return s / q.Count;
    }
    float QueuePolarMedian(ref Queue<float> q)
    {
        bool more_300 = false, less_60 = false;
        float s = 0;
        foreach (float i in q)
        {
            s += i;
            if (i > 300)
                more_300 = true;
            if (i < 60)
                less_60 = true;
        }

        if (more_300 && less_60)
        {
            s = 0;
            foreach (float i in q)
            {
                s += i;
                if (i < 300)
                    s += 360;
            }
            return s / q.Count % 360;
        }
        else
            return s / q.Count;


        /*
        SLOW but work

        float s = 0, c = 0;
        foreach (float i in q)
        {
            s += Mathf.Sin(i * Mathf.Deg2Rad);
            c += Mathf.Cos(i * Mathf.Deg2Rad);
        }
        if (s > 0)
            return Mathf.Rad2Deg * Mathf.Atan2(s, c);
        else
            return Mathf.Rad2Deg * Mathf.Atan2(s, c) + 360;
        */
    }

    void CheckRotateSpeed()
    {
        const float maxNormalRotateSpeed = 5.0f, timeToCalmDown = 3.0f;

        if (GyroscopeRotateSpeed() > maxNormalRotateSpeed)
        {
            if (canvasAutocalibration.gameObject.activeSelf)
            {
                canvasAutocalibration.GetComponent<Animation>().Stop();
                canvasAutocalibration.gameObject.SetActive(false);
            }

            isCalm = false;
            isCalibrated = false;
            timeFromEndJerky = Time.realtimeSinceStartup;
            isJerky = true;
        }
        else
        {
            if (isJerky && !canvasAutocalibration.gameObject.activeSelf)
            {
                canvasAutocalibration.gameObject.SetActive(true);
                canvasAutocalibration.GetComponent<Animation>().Play();
            }

            isJerky = false;
        }

        if (!isCalm && Time.realtimeSinceStartup - timeFromEndJerky > timeToCalmDown)
        {
            canvasAutocalibration.GetComponent<Animation>().Stop();
            canvasAutocalibration.gameObject.SetActive(false);
            isCalm = true;
        }
    }
    float GyroscopeRotateSpeed()
    {
        return Mathf.Abs(Input.gyro.rotationRateUnbiased.x) + Mathf.Abs(Input.gyro.rotationRateUnbiased.y) + Mathf.Abs(Input.gyro.rotationRateUnbiased.z);
    }
    void Calibration()
    {
        player.transform.localEulerAngles = new Vector3(Input.acceleration.z * -90, Input.compass.magneticHeading, Input.acceleration.x * -90);
        isCalibrated = true;
    }

    IEnumerator ShowLogo()
    {
        const float logoTime = 1.5f;
        canvasLogo.gameObject.SetActive(true);

        yield return new WaitForSeconds(logoTime);

        canvasLogo.gameObject.SetActive(false);
        StopCoroutine(ShowLogo());

        CheckGyroscope();
        CheckCompass();
        CheckAcceleration();

        yield break;
    }
    void CheckGyroscope()
    {
        if (Input.gyro.enabled)
        {
            textGyroscopeAvailable.text = "Gyroscope is available";
            isGyroscope = true;
        }
        else
        {
            textGyroscopeAvailable.text = "Gyroscope is NOT available";
            isGyroscope = false;
        }
    }
    void CheckCompass()
    {
        if (Input.compass.enabled)
        {
            textCompassAvailable.text = "Compass is available";
            isCompass = true;
        }
        else
        {
            textCompassAvailable.text = "Compass is NOT available";
            isCompass = false;
        }
    }
    void CheckAcceleration()
    {
        if (Input.acceleration.x + Input.acceleration.y + Input.acceleration.z != 0)
        {
            textAccelerationAvailable.text = "Acceleration is available";
            isAcceleration = true;
        }
        else
        {
            textAccelerationAvailable.text = "Acceleration is NOT available";
            isAcceleration = false;
        }

    }
}

/*
    float LowPassFilter(float h, float raw)
    {
        float filteredValue = 0;
        float tau = 1;  // filter's time constant - lower = faster reponse + weaker noise suppresion, higher = slower, smoother response
        int iteration = 0;
 
        if (iteration == 0) // if it's the first iteration
            filteredValue = raw; // just initate filteredValue
        else
        {
            float alpha = Mathf.Exp(-h / tau); // calculate alfa value based on time step and filter's time constant
            filteredValue = alpha * filteredValue + (1 - alpha) * raw; // calculate new filteredValue from previous value and new raw value
        }
        iteration++; // increment iteration number
        return filteredValue;
    }
*/
