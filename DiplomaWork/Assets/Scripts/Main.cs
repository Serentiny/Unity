using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Main: MonoBehaviour
{
    public Canvas canvasOption, canvasLogo, canvasError, canvasMoving;
    public GameObject Sun, Player;
    public Slider correctionSlider;
    public Text error, fps;
    public Text accelerationAvailable, compassAvailable, gyroscopeAvailable;
    public Text accelerationX, accelerationY, accelerationZ;
    public Text compassRawX, compassRawY, compassRawZ, compassAngle;
    public Text gyroAttitudeX, gyroAttitudeY, gyroAttitudeZ, gyroAttitudeW, gyroRotUnbiasX, gyroRotUnbiasY, gyroRotUnbiasZ;
    public Text verticalAngle, horizontAngle, spinAngle;
    public Text gpsLatitude, gpsLongtitude, gpsAltitude, gpsHorisAccuracy, gpsTimestamp, gpsError;
    public Text wifiError;
    public Button gpsButton, wifiButton;

    // Flags for working with rotating
    bool isGyroscope = false, isCompass = false, isAcceleration = false, areSensorsChecked = false;

    // Time counters for FPS
    float fpsDeltaTime = 0.0f;

    // Variablis for detecting taps/swipes
    float tapLastTime = 0.0f;
    int tapCounter = 0;
    Vector2 fromFirstPos, toFirstPos, fromSecondPos, toSecondPos;
    float fromDist, toDist;
    enum Swipe
        { None, Up, Down, Left, Right};
    Swipe swipeDirection;
    bool isOptionAnimated = false;

    // Current angles of camera
    float curAngleHorizont = 0.0f, curAngleVertical = 0.0f, curAngleSpin = 0.0f;

    bool wifiLoopCheck = false;
    
    // Constant variables
    const float maxTimeToDoubleTap = 0.25f, minSwipeDist = 5.0f, maxNormalRotateSpeed = 5.0f;
    const int doubleTap = 2;
    Rigidbody rb;
    //==================================================================================================================================================================================

    void Start()
    {
        //Never turn off the screen
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //Turn on sensors
        Input.compass.enabled = true;
        Input.gyro.enabled = true;
        
        //add listener to correction slider
        correctionSlider.onValueChanged.AddListener(delegate { HorizontCorrectionChange(); });

        Init();
    }
    void Init()
    {
        canvasOption.gameObject.SetActive(false);
        canvasError.gameObject.SetActive(false);
        canvasMoving.gameObject.SetActive(false);

        Sun.SetActive(false);
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
        if (Input.GetKeyDown(KeyCode.Menu) || tapCounter == doubleTap)
            PressMenu();

        //Update the values in Options
        if (canvasOption.gameObject.activeSelf)
            OptionUpdate();

        //Rotating camera with the smartphone
        CameraRotating();

        //Reset touch counter after small delay or after double click
        if (Time.realtimeSinceStartup - tapLastTime > maxTimeToDoubleTap || tapCounter == doubleTap)
            tapCounter = 0;
        swipeDirection = Swipe.None;

        //Detecting any touches at screen
        if (Input.touchCount > 0)
        {
            if (Input.touchCount == 1)
                CheckSwipeAndTap();
            if (Input.touchCount == 2)
                TwoFingersZoom();
        }

        //Moving by joystick
        if (canvasMoving.gameObject.activeSelf)
            JoystickMoving();
    }
    
    void OptionUpdate()
    {
        if (!isOptionAnimated)
        {
            switch (swipeDirection)
            {
                case Swipe.Up:  //move to the bottom
                    if (canvasOption.transform.FindChild("Central").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionToOptions());
                    else
                    if (canvasOption.transform.FindChild("GPS").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionFromGPS());
                    break;
                case Swipe.Down:  //move to upper
                    if (canvasOption.transform.FindChild("Central").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionToGPS());
                    else
                    if (canvasOption.transform.FindChild("Options").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionFromOptions());
                    break;
                case Swipe.Left:  //move to the right
                    if (canvasOption.transform.FindChild("Central").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionToSensors());
                    else
                    if (canvasOption.transform.FindChild("Routers").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionFromRouters());
                    break;
                case Swipe.Right:  //move to the left
                    if (canvasOption.transform.FindChild("Central").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionToRouters());
                    else
                    if (canvasOption.transform.FindChild("Sensors").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionFromSensors());
                    break;
            }
        }

        if (canvasOption.transform.FindChild("Central").gameObject.activeSelf)
            fps.text = (1.0f / fpsDeltaTime).ToString("00.0") + " FPS";
        if (canvasOption.transform.FindChild("Sensors").gameObject.activeSelf)
            OptionSensorsUpdate();
    }
    void OptionSensorsUpdate()
    {
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
        spinAngle.text     = "Spin = "     + curAngleSpin.ToString(" 000.00;-000.00");
    }

    void CheckSwipeAndTap()
    {
        Touch t0 = Input.GetTouch(0);

        switch (t0.phase)
        {
            case TouchPhase.Began:
                fromFirstPos = new Vector2(t0.position.x, t0.position.y);
                break;
            case TouchPhase.Ended:
                toFirstPos = new Vector2(t0.position.x, t0.position.y);
                if (CalculateDist(ref fromFirstPos, ref toFirstPos) > minSwipeDist)
                {
                    float xDist = toFirstPos.x - fromFirstPos.x, yDist = toFirstPos.y - fromFirstPos.y;
                    if (Mathf.Abs(xDist) > Mathf.Abs(yDist))
                    {
                        if (xDist > 0)
                            swipeDirection = Swipe.Right;
                        if (xDist < 0)
                            swipeDirection = Swipe.Left;
                    }
                    else
                    {
                        if (yDist > 0)
                            swipeDirection = Swipe.Up;
                        if (yDist < 0)
                            swipeDirection = Swipe.Down;
                    }
                }
                else
                {
                    tapLastTime = Time.realtimeSinceStartup;
                    tapCounter++;
                }
                break;
        }
    }
    void TwoFingersZoom()
    {
        Touch t0 = Input.GetTouch(0);
        Touch t1 = Input.GetTouch(1);

        if (fromDist == 0)
        {
            fromFirstPos = new Vector2(t0.position.x, t0.position.y);
            fromSecondPos = new Vector2(t1.position.x, t1.position.y);
            fromDist = CalculateDist(ref fromFirstPos, ref fromSecondPos);
        }
        else if (t0.phase == TouchPhase.Moved || t1.phase == TouchPhase.Moved)
        {
            toFirstPos = new Vector2(t0.position.x, t0.position.y);
            toSecondPos = new Vector2(t1.position.x, t1.position.y);
            toDist = CalculateDist(ref toFirstPos, ref toSecondPos);

            Camera cam = Camera.main;
            if (toDist - fromDist > 0) // zoom in
            {
                if (cam.fieldOfView > 20)
                    cam.fieldOfView--;
            }
            if (toDist - fromDist < 0) // zoom out
            {
                if (cam.fieldOfView < 50)
                    cam.fieldOfView++;
            }

            fromFirstPos = new Vector2(t0.position.x, t0.position.y);
            fromSecondPos = new Vector2(t1.position.x, t1.position.y);
            fromDist = CalculateDist(ref fromFirstPos, ref fromSecondPos);
        }
        else if (t0.phase == TouchPhase.Ended || t1.phase == TouchPhase.Ended)
        {
            fromDist = 0;
            toDist = 0;
        }
    }
    void JoystickMoving()
    {
        var inputVector = new Vector3(CnControls.CnInputManager.GetAxis("Horizontal"), 0f, CnControls.CnInputManager.GetAxis("Vertical"));

        // If we have some input
        if (inputVector.sqrMagnitude > 0.001f)
        {
            rb.AddForce(inputVector.z * Camera.main.transform.forward, ForceMode.Impulse);
            rb.AddForce(inputVector.x * Camera.main.transform.right  , ForceMode.Impulse);
        }
    }

    void CameraRotating()
    {
        if (!areSensorsChecked)
            return;

        if (isGyroscope)
        {
            Camera.main.transform.localRotation = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, -Input.gyro.attitude.z, -Input.gyro.attitude.w);

            curAngleVertical = Camera.main.transform.eulerAngles.x;
            curAngleHorizont = Camera.main.transform.eulerAngles.y;
            curAngleSpin = Camera.main.transform.eulerAngles.z;
        }
        else if (isCompass && isAcceleration)
        {
            curAngleVertical = Input.acceleration.z * -90;
            curAngleHorizont = Input.compass.magneticHeading;
            curAngleSpin = Input.acceleration.x * -90;
            
            Camera.main.transform.localRotation = Quaternion.Slerp(Camera.main.transform.rotation, Quaternion.Euler(curAngleVertical, curAngleHorizont, curAngleSpin), Time.deltaTime);
        }
        else
        {
            if (!canvasError.gameObject.activeSelf)
            {
                error.text = "Sorry, there is no gyroscope or acceleration with compass on that device.";
                canvasError.gameObject.SetActive(true);
            }
        }
    }

    //distance for swipe identification
    float CalculateDist(ref Vector2 a, ref Vector2 b)
    {
        return Mathf.Sqrt((a.x - b.x)*(a.x - b.x) + (a.y - b.y)*(a.y - b.y));
    } 

    void PressEscape()
    {
        if (canvasOption.gameObject.activeSelf)
            canvasOption.gameObject.SetActive(false);
        else
            Application.Quit();
    }
    void PressMenu()
    {
        if (canvasOption.gameObject.activeSelf)
            canvasOption.gameObject.SetActive(false);
        else
        {
            canvasOption.gameObject.SetActive(true);
            canvasOption.transform.FindChild("Central").gameObject.SetActive(true);
            canvasOption.transform.FindChild("Central").localScale = new Vector3(1, 1, 1);
            canvasOption.transform.FindChild("Sensors").gameObject.SetActive(false);
            canvasOption.transform.FindChild("Routers").gameObject.SetActive(false);
            canvasOption.transform.FindChild("GPS").gameObject.SetActive(false);
            canvasOption.transform.FindChild("Options").gameObject.SetActive(false);
        }
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

        if (isGyroscope)
            Player.transform.Rotate(90, 0, 0);
        areSensorsChecked = true;
        rb = Player.GetComponent<Rigidbody>();

        yield return new WaitForSeconds(logoTime);
        canvasLogo.gameObject.SetActive(false);

        StopCoroutine(ShowLogo());
        yield break;
    }
    IEnumerator CheckGPSLocation()
    {
        //if (!Input.location.isEnabledByUser)
        //{
        //    gpsError.text = "Please, turn on GPS manualy";
        //    gpsCheckButton.enabled = true;
        //    yield break;
        //}

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
            gpsError.text += ".";
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
            gpsError.text = "Timed out";
        else if (Input.location.status == LocationServiceStatus.Failed)
        {
            gpsError.text = "Unable to determine device location";
        }
        else
        {
            // Access granted and location value could be retrieved
            gpsLatitude.text = "Latitude = " + Input.location.lastData.latitude;
            gpsLongtitude.text = "Longtitude = " + Input.location.lastData.longitude;
            gpsAltitude.text = "Altitude = " + Input.location.lastData.altitude;
            gpsHorisAccuracy.text = "HorizontalAccuracy = " + Input.location.lastData.horizontalAccuracy;
            gpsTimestamp.text = "Timestamp = " + Input.location.lastData.timestamp;
            gpsError.text = "";
        }

        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
        gpsButton.enabled = true;
        yield break;
    }
    IEnumerator CheckWiFiRouters()
    {
        while (wifiLoopCheck)
        {
            using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
                {
                    try
                    {
                        var enabled = wifiManager.Call<Boolean>("isWifiEnabled");
                        if (!enabled)
                        {
                            wifiError.text = "WiFi is Off; \n";
                            wifiButton.transform.FindChild("Text").GetComponent<Text>().text = "Start WiFi scaning";
                            wifiLoopCheck = false;
                        }
                        else
                        {
                            wifiError.text = "";
                            var scanlist = wifiManager.Call<AndroidJavaObject>("getScanResults");
                            var size = scanlist.Call<int>("size");
                            for (int i = 0; i < size; i++)
                            {
                                var scanResult = scanlist.Call<AndroidJavaObject>("get", i);
                                var SSID = scanResult.Get<String>("SSID");
                                var rssi = scanResult.Get<int>("level");
                                var freq = scanResult.Get<int>("frequency");

                                double exp = (27.55 - (20 * Math.Log10(freq)) + Math.Abs(rssi)) / 20.0;
                                double dist = Math.Pow(10.0, exp);

                                wifiError.text += "'" + SSID + "' " + dist.ToString("F3") + "m.\n";
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        wifiError.color = Color.red;
                        wifiError.text = e.ToString();
                    }
                }
            }
            if (!wifiButton.interactable)
                wifiButton.interactable = true;
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(2);
        wifiButton.interactable = true;
        wifiError.text = "";
    }

    IEnumerator SwipeOptionToOptions()
    {
        isOptionAnimated = true;
        canvasOption.transform.FindChild("Options").gameObject.SetActive(true);
        canvasOption.transform.FindChild("Options").localScale = new Vector3(1, 0, 1);
        canvasOption.transform.FindChild("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.FindChild("Central").localScale = new Vector3(1, 1 - i, 1);
            canvasOption.transform.FindChild("Options").localScale = new Vector3(1, i, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.FindChild("Central").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionToOptions());
        yield break;
    }
    IEnumerator SwipeOptionFromGPS()
    {
        isOptionAnimated = true;
        canvasOption.transform.FindChild("Central").gameObject.SetActive(true);
        canvasOption.transform.FindChild("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.FindChild("Central").localScale = new Vector3(1, i, 1);
            canvasOption.transform.FindChild("GPS").localScale = new Vector3(1, 1 - i, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.FindChild("GPS").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionFromGPS());
        yield break;
    }
    IEnumerator SwipeOptionToGPS()
    {
        isOptionAnimated = true;
        canvasOption.transform.FindChild("GPS").gameObject.SetActive(true);
        canvasOption.transform.FindChild("GPS").localScale = new Vector3(1, 0, 1);
        canvasOption.transform.FindChild("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.FindChild("Central").localScale = new Vector3(1, 1 - i, 1);
            canvasOption.transform.FindChild("GPS").localScale = new Vector3(1, i, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.FindChild("Central").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionToGPS());
        yield break;
    }
    IEnumerator SwipeOptionFromOptions()
    {
        isOptionAnimated = true;
        canvasOption.transform.FindChild("Central").gameObject.SetActive(true);
        canvasOption.transform.FindChild("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.FindChild("Central").localScale = new Vector3(1,     i, 1);
            canvasOption.transform.FindChild("Options").localScale = new Vector3(1, 1 - i, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.FindChild("Options").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionFromOptions());
        yield break;
    }
    IEnumerator SwipeOptionToSensors()
    {
        isOptionAnimated = true;
        canvasOption.transform.FindChild("Sensors").gameObject.SetActive(true);
        canvasOption.transform.FindChild("Sensors").localScale = new Vector3(0, 1, 1);
        canvasOption.transform.FindChild("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.FindChild("Central").localScale = new Vector3(1 - i, 1, 1);
            canvasOption.transform.FindChild("Sensors").localScale = new Vector3(    i, 1, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.FindChild("Central").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionToSensors());
        yield break;
    }
    IEnumerator SwipeOptionFromRouters()
    {
        isOptionAnimated = true;
        canvasOption.transform.FindChild("Central").gameObject.SetActive(true);
        canvasOption.transform.FindChild("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.FindChild("Central").localScale = new Vector3(    i, 1, 1);
            canvasOption.transform.FindChild("Routers").localScale = new Vector3(1 - i, 1, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.FindChild("Routers").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionFromRouters());
        yield break;
    }
    IEnumerator SwipeOptionToRouters()
    {
        isOptionAnimated = true;
        canvasOption.transform.FindChild("Routers").gameObject.SetActive(true);
        canvasOption.transform.FindChild("Routers").localScale = new Vector3(0, 1, 1);
        canvasOption.transform.FindChild("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.FindChild("Central").localScale = new Vector3(1 - i, 1, 1);
            canvasOption.transform.FindChild("Routers").localScale = new Vector3(    i, 1, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.FindChild("Central").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionToRouters());
        yield break;
    }
    IEnumerator SwipeOptionFromSensors()
    {
        isOptionAnimated = true;
        canvasOption.transform.FindChild("Central").gameObject.SetActive(true);
        canvasOption.transform.FindChild("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.FindChild("Central").localScale = new Vector3(    i, 1, 1);
            canvasOption.transform.FindChild("Sensors").localScale = new Vector3(1 - i, 1, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.FindChild("Sensors").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionFromSensors());
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
    
    void HorizontCorrectionChange()
    {
        Player.transform.rotation = Quaternion.Euler(Player.transform.rotation.eulerAngles.x, correctionSlider.value, Player.transform.rotation.eulerAngles.z);
    }

    public void CheckGPS()
    {
        gpsButton.enabled = false;
        gpsError.text = "Please wait a moment";
        StartCoroutine(CheckGPSLocation());
    }
    public void CheckWiFi()
    {
        wifiButton.interactable = false;
        if (wifiLoopCheck)
        {
            wifiButton.transform.FindChild("Text").GetComponent<Text>().text = "Start WiFi scaning";
            wifiLoopCheck = false;
        }
        else
        {
            wifiButton.transform.FindChild("Text").GetComponent<Text>().text = "Stop WiFi scaninig";
            wifiLoopCheck = true;
            StartCoroutine(CheckWiFiRouters());
        }
    }
    public void ToggleSun(bool _this)
    {
        Sun.SetActive(!Sun.activeSelf);
    }
    public void ToggleMoving(bool _this)
    {
        canvasMoving.gameObject.SetActive(!canvasMoving.gameObject.activeSelf);
    }
}