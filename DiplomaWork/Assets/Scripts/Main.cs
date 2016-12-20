using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main: MonoBehaviour
{
    public GameObject player;
    public Canvas canvasDebug, canvasLogo, canvasAutocalibration, canvasError;
    public UnityEngine.UI.Text error, fps;
    public UnityEngine.UI.Text accelerationAvailable, compassAvailable, gyroscopeAvailable;
    public UnityEngine.UI.Text accelerationX, accelerationY, accelerationZ;
    public UnityEngine.UI.Text compassRawX, compassRawY, compassRawZ, compassAngle;
    public UnityEngine.UI.Text gyroAttitudeX, gyroAttitudeY, gyroAttitudeZ, gyroAttitudeW, gyroRotUnbiasX, gyroRotUnbiasY, gyroRotUnbiasZ;
    public UnityEngine.UI.Text verticalAngle, horizontAngle, spinAngle;

    // Flags for working with rotating
    bool isGyroscope = false, isCompass = false, isAcceleration = false, areSensorsChecked = false;

    // Time counters for FPS and DoubleTap for smartphones
    float fpsDeltaTime = 0.0f, touchLastTime = 0.0f;
    int touchCounter = 0;

    // Current angles of camera
    float curAngleHorizont = 0.0f, curAngleVertical = 0.0f, curAngleSpin = 0.0f;

    // Variables for calibration gyroscope
    bool isCalibrated = false, isDeviceCalm = true, isDeviceJerky = false;
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
        canvasError.gameObject.SetActive(false);
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
            OptionSensorsUpdate();

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

    void OptionSensorsUpdate()
    {
        fps.text = (1.0f / fpsDeltaTime).ToString("00.0") + " FPS";

        accelerationX.text = "Acceleration.X = " + Input.acceleration.x.ToString(" 0.000;-0.000");
        accelerationY.text = "Acceleration.Y = " + Input.acceleration.y.ToString(" 0.000;-0.000");
        accelerationZ.text = "Acceleration.Z = " + Input.acceleration.z.ToString(" 0.000;-0.000");

        compassRawX.text  = "RawVector.X = " + Input.compass.rawVector.x.ToString(" 000;-000");
        compassRawY.text  = "RawVector.Y = " + Input.compass.rawVector.y.ToString(" 000;-000");
        compassRawZ.text  = "RawVector.Z = " + Input.compass.rawVector.z.ToString(" 000;-000");
        compassAngle.text = "MagnetHeading = " + Input.compass.magneticHeading.ToString(" 000.00");

        gyroAttitudeX.text  = "Attitude.X = " + Input.gyro.attitude.x.ToString(" 00.000;-00.000");
        gyroAttitudeY.text  = "Attitude.Y = " + Input.gyro.attitude.y.ToString(" 00.000;-00.000");
        gyroAttitudeZ.text  = "Attitude.Z = " + Input.gyro.attitude.z.ToString(" 00.000;-00.000");
        gyroAttitudeW.text  = "Attitude.W = " + Input.gyro.attitude.w.ToString(" 00.000;-00.000");
        gyroRotUnbiasX.text = "RotationRateUnbiased.X = " + Input.gyro.rotationRateUnbiased.x.ToString(" 00.000;-00.000");
        gyroRotUnbiasY.text = "RotationRateUnbiased.Y = " + Input.gyro.rotationRateUnbiased.y.ToString(" 00.000;-00.000");
        gyroRotUnbiasZ.text = "RotationRateUnbiased.Z = " + Input.gyro.rotationRateUnbiased.z.ToString(" 00.000;-00.000");

        verticalAngle.text = "Vertical = " + curAngleVertical.ToString(" 000.00;-000.00");
        horizontAngle.text = "Horizont = " + curAngleHorizont.ToString(" 000.00;-000.00");
        spinAngle.text     = "Spin = " + curAngleSpin.ToString(" 000.00;-000.00");
    }

    void CameraRotating()
    {
        if (!areSensorsChecked)
            return;

        if (isGyroscope && isCompass && isAcceleration)
        {
            CheckRotateSpeed();

            if (!isCalibrated && isDeviceCalm)
                Calibration();

            curAngleVertical = -Input.gyro.rotationRateUnbiased.x;
            curAngleHorizont = -Input.gyro.rotationRateUnbiased.y;
            curAngleSpin = Input.gyro.rotationRateUnbiased.z;

            player.transform.Rotate(curAngleVertical, curAngleHorizont, curAngleSpin);

            curAngleVertical = player.transform.eulerAngles.x;
            curAngleHorizont = player.transform.eulerAngles.y;
            curAngleSpin = player.transform.eulerAngles.z;
        }
        else if (isGyroscope)
        {
            // Similar to above but with manual calibration

            if (!canvasError.gameObject.activeSelf)
            {
                if (!isAcceleration || !isCompass)
                    error.text = "Sorry, you have gyroscope but there is no acceleration or compass on that device.";
                canvasError.gameObject.SetActive(true);
            }
        }
        else if (isCompass && isAcceleration)
        {
            curAngleVertical = Input.acceleration.z * -90;
            curAngleHorizont = Input.compass.magneticHeading;
            curAngleSpin = Input.acceleration.x * -90;

            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, Quaternion.Euler(curAngleVertical, curAngleHorizont, curAngleSpin), Time.deltaTime);
        }
        else
        {
            if (!canvasError.gameObject.activeSelf)
            {
                if (!isAcceleration || !isCompass)
                    error.text = "Sorry, there is no gyroscope, acceleration or compass on that device.";
                canvasError.gameObject.SetActive(true);
            }
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

            isDeviceCalm = false;
            isCalibrated = false;
            timeFromEndJerky = Time.realtimeSinceStartup;
            isDeviceJerky = true;
        }
        else
        {
            if (isDeviceJerky && !canvasAutocalibration.gameObject.activeSelf)
            {
                canvasAutocalibration.gameObject.SetActive(true);
                canvasAutocalibration.GetComponent<Animation>().Play();
            }

            isDeviceJerky = false;
        }

        if (!isDeviceCalm && Time.realtimeSinceStartup - timeFromEndJerky > timeToCalmDown)
        {
            canvasAutocalibration.GetComponent<Animation>().Stop();
            canvasAutocalibration.gameObject.SetActive(false);
            isDeviceCalm = true;
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
        const float sensorUp = 0.1f;
        const float logoTime = 1.5f;
        canvasLogo.gameObject.SetActive(true);

        yield return new WaitForSeconds(sensorUp);
        CheckGyroscope();
        CheckCompass();
        CheckAcceleration();
        areSensorsChecked = true;

        yield return new WaitForSeconds(logoTime);
        canvasLogo.gameObject.SetActive(false);
        StopCoroutine(ShowLogo());

        yield break;
    }
	
    void CheckGyroscope()
    {
        //(Input.gyro.enabled)
		if (Input.gyro.attitude.x + Input.gyro.attitude.y + Input.gyro.attitude.z != 0)
        {
            gyroscopeAvailable.text = "Gyroscope is available";
            isGyroscope = true;
        }
        else
        {
            gyroscopeAvailable.text = "Gyroscope is NOT available";
            isGyroscope = false;
        }
    }
    void CheckCompass()
    {
        //if (Input.compass.enabled)
		if (Input.compass.rawVector.x + Input.compass.rawVector.y + Input.compass.rawVector.z != 0)
        {
            compassAvailable.text = "Compass is available";
            isCompass = true;
        }
        else
        {
            compassAvailable.text = "Compass is NOT available";
            isCompass = false;
        }
    }
    void CheckAcceleration()
    {
        if (Input.acceleration.x + Input.acceleration.y + Input.acceleration.z != 0)
        {
            accelerationAvailable.text = "Acceleration is available";
            isAcceleration = true;
        }
        else
        {
            accelerationAvailable.text = "Acceleration is NOT available";
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
