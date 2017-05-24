using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NativeWifi;
using System.IO;

public struct listRouterInfo
{
    double dist;
    int freq;
    int rssi;
    string ssid;

    public listRouterInfo(double _dist, int _freq, int _rssi, string _ssid)
    {
        dist = _dist;
        freq = _freq;
        rssi = _rssi;
        ssid = _ssid;
    }

    public double GetDist()
    {
        return dist;
    }

    public int GetFreq()
    {
        return freq;
    }

    public int GetRssi()
    {
        return rssi;
    }

    public string GetSsid()
    {
        return ssid;
    }
}
public struct cornerRouterInfo
{
    int cornerNo;
    string ssid;

    public cornerRouterInfo(int _cornerNo, string _ssid)
    {
        cornerNo = _cornerNo;
        ssid = _ssid;
    }

    public int GetCornerNo()
    {
        return cornerNo;
    }

    public string GetSsid()
    {
        return ssid;
    }
}

public class Main: MonoBehaviour
{
    public Canvas canvasOption, canvasLogo, canvasError, canvasJoystick;
    public GameObject Sun, Player, wifiSprite;
    public Slider correctionSlider;
    public Toggle Move, JMove, RMove;
    public Text error, fps;
    public Text gpsLatitude, gpsLongtitude, gpsAltitude, gpsHorisAccuracy, gpsTimestamp, gpsError;
    public Text wifiScanText, wifiCalibrateText, rMoveOutput;
    public Button gpsButton, wifiScanButton, wifiCalibrateButton, wifiSetButton, wifiNextButton;

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
    Rigidbody rb;

    // Working with routers
    bool wifiLoopCheck = false;
    Dictionary<string, listRouterInfo> wifiList = new Dictionary<String, listRouterInfo>();
    int cornerNo = 0;
    Dictionary<string, cornerRouterInfo> listCornerRouters = new Dictionary<string, cornerRouterInfo>();
    float kX, kZ;
    List<float> listKX, listKZ;

    // Constant variables
    const float maxTimeToDoubleTap = 0.25f, minSwipeDist = 5.0f, maxNormalRotateSpeed = 5.0f;
    const int doubleTap = 2;
    const double cornerDist = 0.5f;
    const float zDist = 14, xDist = 8;
    string os;
    //==================================================================================================================================================================================

    void Start()
    {
        os = Environment.OSVersion.Platform.ToString().Substring(0, 3);
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
        canvasJoystick.gameObject.SetActive(false);
        JMove.gameObject.SetActive(false);
        JMove.isOn = false;
        RMove.gameObject.SetActive(false);
        RMove.isOn = false;
        wifiCalibrateButton.interactable = false;
        wifiSetButton.gameObject.SetActive(false);
        wifiNextButton.gameObject.SetActive(false);
        wifiSprite.gameObject.SetActive(false);

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
        if (canvasJoystick.gameObject.activeSelf)
            JoystickMoving();
    }
    
    void OptionUpdate()
    {
        if (!isOptionAnimated)
        {
            switch (swipeDirection)
            {
                case Swipe.Up:  //move to the bottom
                    if (canvasOption.transform.Find("Central").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionToOptions());
                    else
                    if (canvasOption.transform.Find("GPS").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionFromGPS());
                    break;
                case Swipe.Down:  //move to upper
                    if (canvasOption.transform.Find("Central").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionToGPS());
                    else
                    if (canvasOption.transform.Find("Options").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionFromOptions());
                    break;
                case Swipe.Left:  //move to the right
                    if (canvasOption.transform.Find("Central").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionToSensors());
                    else
                    if (canvasOption.transform.Find("Routers").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionFromRouters());
                    break;
                case Swipe.Right:  //move to the left
                    if (canvasOption.transform.Find("Central").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionToRouters());
                    else
                    if (canvasOption.transform.Find("Sensors").gameObject.activeSelf)
                        StartCoroutine(SwipeOptionFromSensors());
                    break;
            }
        }

        if (canvasOption.transform.Find("Central").gameObject.activeSelf)
            fps.text = (1.0f / fpsDeltaTime).ToString("00.0") + " FPS";
        if (canvasOption.transform.Find("Sensors").gameObject.activeSelf)
            OptionSensorsUpdate();
    }
    void OptionSensorsUpdate()
    {
        canvasOption.transform.Find("Sensors/Acceleration/X").GetComponent<Text>().text = "Acceleration.X = " + Input.acceleration.x.ToString(" 0.000;-0.000");
        canvasOption.transform.Find("Sensors/Acceleration/Y").GetComponent<Text>().text = "Acceleration.Y = " + Input.acceleration.y.ToString(" 0.000;-0.000");
        canvasOption.transform.Find("Sensors/Acceleration/Z").GetComponent<Text>().text = "Acceleration.Z = " + Input.acceleration.z.ToString(" 0.000;-0.000");

        canvasOption.transform.Find("Sensors/Compass/Raw.X").GetComponent<Text>().text         = "RawVector.X = "   + Input.compass.rawVector.x.ToString(" 000;-000");
        canvasOption.transform.Find("Sensors/Compass/Raw.Y").GetComponent<Text>().text         = "RawVector.Y = "   + Input.compass.rawVector.y.ToString(" 000;-000");
        canvasOption.transform.Find("Sensors/Compass/Raw.Z").GetComponent<Text>().text         = "RawVector.Z = "   + Input.compass.rawVector.z.ToString(" 000;-000");
        canvasOption.transform.Find("Sensors/Compass/MagnetHeading").GetComponent<Text>().text = "MagnetHeading = " + Input.compass.magneticHeading.ToString(" 000.00");

        canvasOption.transform.Find("Sensors/Gyroscope/Attitude.X").GetComponent<Text>().text  = "Attitude.X = " + Input.gyro.attitude.x.ToString(" 00.000;-00.000");
        canvasOption.transform.Find("Sensors/Gyroscope/Attitude.Y").GetComponent<Text>().text  = "Attitude.Y = " + Input.gyro.attitude.y.ToString(" 00.000;-00.000");
        canvasOption.transform.Find("Sensors/Gyroscope/Attitude.Z").GetComponent<Text>().text  = "Attitude.Z = " + Input.gyro.attitude.z.ToString(" 00.000;-00.000");
        canvasOption.transform.Find("Sensors/Gyroscope/Attitude.W").GetComponent<Text>().text  = "Attitude.W = " + Input.gyro.attitude.w.ToString(" 00.000;-00.000");
        canvasOption.transform.Find("Sensors/Gyroscope/RotationRateUnbiased.X").GetComponent<Text>().text = "RotationRateUnbiased.X = " + Input.gyro.rotationRateUnbiased.x.ToString(" 00.000;-00.000");
        canvasOption.transform.Find("Sensors/Gyroscope/RotationRateUnbiased.Y").GetComponent<Text>().text = "RotationRateUnbiased.Y = " + Input.gyro.rotationRateUnbiased.y.ToString(" 00.000;-00.000");
        canvasOption.transform.Find("Sensors/Gyroscope/RotationRateUnbiased.Z").GetComponent<Text>().text = "RotationRateUnbiased.Z = " + Input.gyro.rotationRateUnbiased.z.ToString(" 00.000;-00.000");

        canvasOption.transform.Find("Sensors/RealCoord/CurrentAngle.X").GetComponent<Text>().text = "Vertical = " + curAngleVertical.ToString(" 000.00;-000.00");
        canvasOption.transform.Find("Sensors/RealCoord/CurrentAngle.Y").GetComponent<Text>().text = "Horizont = " + curAngleHorizont.ToString(" 000.00;-000.00");
        canvasOption.transform.Find("Sensors/RealCoord/CurrentAngle.Z").GetComponent<Text>().text     = "Spin = "     + curAngleSpin.ToString(" 000.00;-000.00");
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
            canvasOption.transform.Find("Central").gameObject.SetActive(true);
            canvasOption.transform.Find("Central").localScale = new Vector3(1, 1, 1);
            canvasOption.transform.Find("Sensors").gameObject.SetActive(false);
            canvasOption.transform.Find("Routers").gameObject.SetActive(false);
            canvasOption.transform.Find("GPS").gameObject.SetActive(false);
            canvasOption.transform.Find("Options").gameObject.SetActive(false);
        }
    }

    void CheckGyroscope()
    {
        //(Input.gyro.enabled)
		if (Input.gyro.attitude.x + Input.gyro.attitude.y + Input.gyro.attitude.z != 0)
        {
            canvasOption.transform.Find("Sensors/Availability/Gyroscope").GetComponent<Text>().text = "Gyroscope is available";
            isGyroscope = true;
        }
        else
        {
            canvasOption.transform.Find("Sensors/Availability/Gyroscope").GetComponent<Text>().text = "Gyroscope is NOT available";
            isGyroscope = false;
        }
    }
    void CheckCompass()
    {
        //if (Input.compass.enabled)
		if (Input.compass.rawVector.x + Input.compass.rawVector.y + Input.compass.rawVector.z != 0)
        {
            canvasOption.transform.Find("Sensors/Availability/Compass").GetComponent<Text>().text = "Compass is available";
            isCompass = true;
        }
        else
        {
            canvasOption.transform.Find("Sensors/Availability/Compass").GetComponent<Text>().text = "Compass is NOT available";
            isCompass = false;
        }
    }
    void CheckAcceleration()
    {
        if (Input.acceleration.x + Input.acceleration.y + Input.acceleration.z != 0)
        {
            canvasOption.transform.Find("Sensors/Availability/Acceleration").GetComponent<Text>().text = "Acceleration is available";
            isAcceleration = true;
        }
        else
        {
            canvasOption.transform.Find("Sensors/Availability/Acceleration").GetComponent<Text>().text = "Acceleration is NOT available";
            isAcceleration = false;
        }

    }
    
    void HorizontCorrectionChange()
    {
        Player.transform.rotation = Quaternion.Euler(Player.transform.rotation.eulerAngles.x, correctionSlider.value, Player.transform.rotation.eulerAngles.z);
    }

    bool TryGetSSIDByCornerNo(ref Dictionary<string, cornerRouterInfo> dict, int value, out List<string> ssids)
    {
        ssids = new List<string>();
        ssids.Clear();
        foreach (var record in dict)
        {
            if (record.Value.GetCornerNo().Equals(value))
                ssids.Add(record.Value.GetSsid());
        }
        if (ssids.Count != 0)
            return true;
        return false;
    }
    bool TryGetBSSIDByCornerNo(ref Dictionary<string, cornerRouterInfo> dict, int value, out List<string> bssids)
    {
        bssids = new List<string>();
        bssids.Clear();
        foreach (var record in dict)
        {
            if (record.Value.GetCornerNo().Equals(value))
                bssids.Add(record.Key);
        }
        if (bssids.Count != 0)
            return true;
        return false;
    }

    public void CheckGPS()
    {
        gpsButton.enabled = false;
        gpsError.text = "Please wait a moment";
        StartCoroutine(CheckGPSLocation());
    }
    public void CheckWiFi()
    {
        wifiScanButton.interactable = false;
        if (wifiLoopCheck)
        {
            wifiScanButton.transform.Find("Text").GetComponent<Text>().text = "Start WiFi scaning";
            wifiLoopCheck = false;
            wifiCalibrateButton.interactable = false;
        }
        else
        {
            wifiScanButton.transform.Find("Text").GetComponent<Text>().text = "Stop WiFi scaninig";
            wifiLoopCheck = true;
            wifiCalibrateButton.interactable = true;
            StartCoroutine(CheckWiFiRouters());
        }
    }
    public void CalibrateWiFi()
    {
        wifiCalibrateButton.gameObject.SetActive(false);
        wifiScanButton.interactable = false;
        wifiSetButton.gameObject.SetActive(true);
        wifiNextButton.gameObject.SetActive(true);
        StartCoroutine(CalibrateWiFiRouters());
    }
    public void AddWiFiRouter()
    {                        
        foreach (var router in wifiList)
        {
            if (router.Value.GetDist() < cornerDist)
            {
                if (listCornerRouters.ContainsKey(router.Key))
                    listCornerRouters[router.Key] = new cornerRouterInfo(cornerNo, router.Value.GetSsid());
                else
                    listCornerRouters.Add(router.Key, new cornerRouterInfo(cornerNo, router.Value.GetSsid()));
            }
        }

        List<string> list;
        wifiCalibrateText.text = "Routers in corner #" + (cornerNo + 1).ToString() + ":\n";
        if (TryGetSSIDByCornerNo(ref listCornerRouters, cornerNo, out list))
            foreach (var router in list)
                wifiCalibrateText.text += "  '" + router + "'\n";

        List<string> listBssid;
        switch (cornerNo)           //подсчет коэффициентов пропорций реальной и виртуальной комнат
        {
            case 1:
                if (TryGetBSSIDByCornerNo(ref listCornerRouters, 0, out listBssid))
                    foreach (var bssid in listBssid)
                        if (wifiList.ContainsKey(bssid))
                            listKZ.Add((float)wifiList[bssid].GetDist());
                break;
            case 2:
                if (TryGetBSSIDByCornerNo(ref listCornerRouters, 1, out listBssid))
                    foreach (var bssid in listBssid)
                        if (wifiList.ContainsKey(bssid))
                            listKX.Add((float)wifiList[bssid].GetDist());
                break;
            case 3:
                if (TryGetBSSIDByCornerNo(ref listCornerRouters, 2, out listBssid))
                    foreach (var bssid in listBssid)
                        if (wifiList.ContainsKey(bssid))
                            listKZ.Add((float)wifiList[bssid].GetDist());
                if (TryGetBSSIDByCornerNo(ref listCornerRouters, 0, out listBssid))
                    foreach (var bssid in listBssid)
                        if (wifiList.ContainsKey(bssid))
                            listKX.Add((float)wifiList[bssid].GetDist());
                break;
            default:
                break;
        }
    }
    public void NextWiFiRouter()
    {
        cornerNo++;
        if (cornerNo < 4)
            wifiCalibrateText.text = "Routers in corner #" + (cornerNo + 1).ToString() + ":\n";

        switch (cornerNo)
        {
            case 1:
                wifiSprite.transform.position = new Vector3(-3.5f, 1.5f, 6.5f);
                break;
            case 2:
                wifiSprite.transform.position = new Vector3(3.5f, 1.5f, 6.5f);
                break;
            case 3:
                wifiSprite.transform.position = new Vector3(3.5f, 1.5f, -6.5f);
                break;
            default:
                wifiSprite.gameObject.SetActive(false);
                break; 
        }
    }
    public void SaveWiFiData()
    {
        if (!wifiLoopCheck)
            return;
        StartCoroutine(WriteWiFiData());
                                                                    //Можно добавить затухание кнопки для убирания левых нажатий
    }

    public void ToggleSun(bool _this)
    {
        Sun.SetActive(!Sun.activeSelf);
    }
    public void ToggleMoving(bool _this)
    {
        JMove.gameObject.SetActive(Move.isOn);
        RMove.gameObject.SetActive(Move.isOn);
        JMove.isOn = JMove.gameObject.activeSelf ? true : false;
        RMove.isOn = false;
    }
    public void ToggleJMoving(bool _this)
    {
        if (Move.isOn)
            RMove.isOn = !JMove.isOn;
        canvasJoystick.gameObject.SetActive(JMove.isOn);
    }
    public void ToggleRMoving(bool _this)
    {
        if (Move.isOn)
            JMove.isOn = !RMove.isOn;
        if (RMove.isOn)
            StartCoroutine(RMoving());
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
        wifiScanText.color = Color.black;
        while (wifiLoopCheck)
        {
            if (!os.Equals("Win"))
            {
                // ANDROID
                using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
                    {
                        try
                        {
                            var enabled = wifiManager.Call<Boolean>("isWifiEnabled");
                            if (!enabled)
                            {
                                wifiScanText.text = "WiFi is Off; \n";
                                wifiScanButton.transform.Find("Text").GetComponent<Text>().text = "Start WiFi scaning";
                                wifiLoopCheck = false;
                                wifiCalibrateButton.interactable = false;
                            }
                            else
                            {
                                wifiScanText.text = "";
                                if (!wifiManager.Call<bool>("startScan"))
                                    continue;

                                var scanlist = wifiManager.Call<AndroidJavaObject>("getScanResults");
                                var size = scanlist.Call<int>("size");
                                for (int i = 0; i < size; i++)
                                {
                                    var scanResult = scanlist.Call<AndroidJavaObject>("get", i);
                                    var BSSID = scanResult.Get<String>("BSSID");
                                    var SSID = scanResult.Get<String>("SSID");
                                    var rssi = scanResult.Get<int>("level");
                                    var freq = scanResult.Get<int>("frequency");

                                    double exp = (27.55 - (20 * Math.Log10(freq)) + Math.Abs(rssi)) / 20.0;
                                    double dist = Math.Pow(10.0, exp);

                                    if (wifiList.ContainsKey(BSSID))
                                        wifiList[BSSID] = new listRouterInfo(dist, freq, rssi, SSID);
                                    else
                                        wifiList.Add(BSSID, new listRouterInfo(dist, freq, rssi, SSID));

                                    wifiScanText.text += "'" + SSID + "' " + dist.ToString("F3") + "m.\n";
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            wifiScanText.color = Color.red;
                            wifiScanText.text = e.ToString();
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
                // ANDROID
            }
            else
            {
                // WINDOWS
                WlanClient client;
                try
                {
                    client = new WlanClient();
                }
                catch (Exception)
                {
                    continue;
                }

                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {                    
                    try
                    {
                        wlanIface.Scan();
                    }
                    catch (Exception)
                    {
                        break;
                    }


                    yield return new WaitForSeconds(1);
                    
                    bool needToCleanOutput = true;
                    Wlan.WlanBssEntry[] wlanBssEntries1 = wlanIface.GetNetworkBssList();
                    foreach (Wlan.WlanBssEntry network in wlanBssEntries1)
                    {
                        if (needToCleanOutput)
                        {
                            wifiScanText.text = "";
                            needToCleanOutput = false;
                        }
                        var BSSID = System.Text.Encoding.ASCII.GetString(network.dot11Bssid).Trim((char)0);
                        var SSID = System.Text.Encoding.ASCII.GetString(network.dot11Ssid.SSID).Trim((char)0);
                        var rssi = network.rssi;
                        var freq = (int)network.chCenterFrequency / 1000;

                        double exp = (27.55 - (20 * Math.Log10(freq)) + Math.Abs(rssi)) / 20.0;
                        double dist = Math.Pow(10.0, exp);

                        if (wifiList.ContainsKey(BSSID))
                            wifiList[BSSID] = new listRouterInfo(dist, freq, rssi, SSID);
                        else
                            wifiList.Add(BSSID, new listRouterInfo(dist, freq, rssi, SSID));

                        wifiScanText.text += "'" + SSID + "' " + dist.ToString("F3") + "m.\n";
                    }
                }
                // WINDOWS
            }

            if (!wifiScanButton.interactable)
                wifiScanButton.interactable = true;
        }
        yield return new WaitForSeconds(2);
        wifiScanButton.interactable = true;
        wifiScanText.text = "";
    }
    IEnumerator CalibrateWiFiRouters()
    {
        listKX = new List<float>();
        listKZ = new List<float>();
        kX = 0;
        kZ = 0;
        listCornerRouters.Clear();
        cornerNo = 0;
        wifiSprite.gameObject.SetActive(true);
        wifiSprite.transform.position = new Vector3(-3.5f, 1.5f, -6.5f);
        wifiCalibrateText.text = "Routers in corner #" + (cornerNo + 1).ToString() + ":\n";

        while (cornerNo < 4)
        {
            wifiSprite.transform.LookAt(Player.transform);
            yield return new WaitForEndOfFrame();
        }

        List<string> listSsid;
        wifiCalibrateText.text = "";
        for (int i = 0; i < 4; i++)
        {
            wifiCalibrateText.text += "Routers in corner #" + (i + 1).ToString() + ":\n";
            if (TryGetSSIDByCornerNo(ref listCornerRouters, i, out listSsid))
                foreach (var router in listSsid)
                    wifiCalibrateText.text += "  '" + router + "'\n";
        }

        foreach (var f in listKX)
            kX += f;
        if (listKX.Count > 0)
            kX = kX / listKX.Count / xDist;
        foreach (var f in listKZ)
            kZ += f;
        if (listKZ.Count > 0)
            kZ = kZ / listKZ.Count / zDist;
        wifiCalibrateText.text += "kX = " + kX + "; kZ = " + kZ + ";\n";

        wifiCalibrateButton.gameObject.SetActive(true);
        wifiScanButton.interactable = true;
        wifiSetButton.gameObject.SetActive(false);
        wifiNextButton.gameObject.SetActive(false);
        StopCoroutine(CalibrateWiFiRouters());
        yield break;
    }
    IEnumerator WriteWiFiData()
    {
        string cur_time = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss");
        Directory.CreateDirectory(Application.persistentDataPath + "/" + cur_time);

        while (wifiLoopCheck)
        {
            yield return new WaitForSeconds(1);

            foreach (KeyValuePair<string, listRouterInfo> _pair in wifiList)
            {
                string l1 = _pair.Key;
                string l2 = _pair.Value.GetSsid().Normalize();
                l2 = l2.Substring(0, Math.Min(25, l2.Length));
                double dist = _pair.Value.GetDist();
                int freq = _pair.Value.GetFreq();
                int rssi = _pair.Value.GetRssi();

                try
                {
                    string fileName = Application.persistentDataPath + "/" + cur_time + "/wifidata_" + l2 + ".txt";
                    StreamWriter sr = new StreamWriter(fileName, true);
                    sr.WriteLine(dist.ToString() + " " + freq.ToString() + " " + rssi.ToString());
                    sr.Close();
                }
                catch (Exception)
                {}
            }
        }
                                                                                //Можно добавить появление кнопки для убирания левых нажатий
        yield break;
    }

    IEnumerator SwipeOptionToOptions()
    {
        isOptionAnimated = true;
        canvasOption.transform.Find("Options").gameObject.SetActive(true);
        canvasOption.transform.Find("Options").localScale = new Vector3(1, 0, 1);
        canvasOption.transform.Find("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.Find("Central").localScale = new Vector3(1, 1 - i, 1);
            canvasOption.transform.Find("Options").localScale = new Vector3(1, i, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.Find("Central").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionToOptions());
        yield break;
    }
    IEnumerator SwipeOptionFromGPS()
    {
        isOptionAnimated = true;
        canvasOption.transform.Find("Central").gameObject.SetActive(true);
        canvasOption.transform.Find("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.Find("Central").localScale = new Vector3(1, i, 1);
            canvasOption.transform.Find("GPS").localScale = new Vector3(1, 1 - i, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.Find("GPS").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionFromGPS());
        yield break;
    }
    IEnumerator SwipeOptionToGPS()
    {
        isOptionAnimated = true;
        canvasOption.transform.Find("GPS").gameObject.SetActive(true);
        canvasOption.transform.Find("GPS").localScale = new Vector3(1, 0, 1);
        canvasOption.transform.Find("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.Find("Central").localScale = new Vector3(1, 1 - i, 1);
            canvasOption.transform.Find("GPS").localScale = new Vector3(1, i, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.Find("Central").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionToGPS());
        yield break;
    }
    IEnumerator SwipeOptionFromOptions()
    {
        isOptionAnimated = true;
        canvasOption.transform.Find("Central").gameObject.SetActive(true);
        canvasOption.transform.Find("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.Find("Central").localScale = new Vector3(1,     i, 1);
            canvasOption.transform.Find("Options").localScale = new Vector3(1, 1 - i, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.Find("Options").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionFromOptions());
        yield break;
    }
    IEnumerator SwipeOptionToSensors()
    {
        isOptionAnimated = true;
        canvasOption.transform.Find("Sensors").gameObject.SetActive(true);
        canvasOption.transform.Find("Sensors").localScale = new Vector3(0, 1, 1);
        canvasOption.transform.Find("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.Find("Central").localScale = new Vector3(1 - i, 1, 1);
            canvasOption.transform.Find("Sensors").localScale = new Vector3(    i, 1, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.Find("Central").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionToSensors());
        yield break;
    }
    IEnumerator SwipeOptionFromRouters()
    {
        isOptionAnimated = true;
        canvasOption.transform.Find("Central").gameObject.SetActive(true);
        canvasOption.transform.Find("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.Find("Central").localScale = new Vector3(    i, 1, 1);
            canvasOption.transform.Find("Routers").localScale = new Vector3(1 - i, 1, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.Find("Routers").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionFromRouters());
        yield break;
    }
    IEnumerator SwipeOptionToRouters()
    {
        isOptionAnimated = true;
        canvasOption.transform.Find("Routers").gameObject.SetActive(true);
        canvasOption.transform.Find("Routers").localScale = new Vector3(0, 1, 1);
        canvasOption.transform.Find("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.Find("Central").localScale = new Vector3(1 - i, 1, 1);
            canvasOption.transform.Find("Routers").localScale = new Vector3(    i, 1, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.Find("Central").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionToRouters());
        yield break;
    }
    IEnumerator SwipeOptionFromSensors()
    {
        isOptionAnimated = true;
        canvasOption.transform.Find("Central").gameObject.SetActive(true);
        canvasOption.transform.Find("Central").gameObject.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);

        float i = 0.0f, step = Time.deltaTime * 5;
        while (i < 1.0f)
        {
            i = Mathf.Min(i + step, 1.0f);
            canvasOption.transform.Find("Central").localScale = new Vector3(    i, 1, 1);
            canvasOption.transform.Find("Sensors").localScale = new Vector3(1 - i, 1, 1);
            yield return new WaitForEndOfFrame();
        }
        canvasOption.transform.Find("Sensors").gameObject.SetActive(false);
        isOptionAnimated = false;

        StopCoroutine(SwipeOptionFromSensors());
        yield break;
    }
    IEnumerator RMoving()
    {
        List<float> wfDist = new List<float>();
        for (int i = 0; i < 4; i++)
            wfDist.Add(0);
        float newXPos = 0, newZPos = 0;

        while (RMove.isOn)
        {
            for (int i = 0; i < 4; i++)
            {
                List<string> routers;                                                       // Для каждого угла
                if (TryGetBSSIDByCornerNo(ref listCornerRouters, i, out routers))           // Достаем все его роутеры
                {
                    int count = 0;
                    float dist = 0;
                    foreach (var bssid in routers)                                          // И для каждого роутера
                        if (wifiList.ContainsKey(bssid))
                        {
                            dist += (float)wifiList[bssid].GetDist();                       // Достаем расстояние от него
                            count++;
                        }
                    if (count > 0)                                                          // После чего усредняем по всем роутерам в сети
                        wfDist[i] = dist / count;
                }
                else
                {
                    JMove.isOn = true;
                    break;                                                                  // А если хотя бы одного роутера в углу нет - это косяк
                }
            }

            if (JMove.isOn)
            {
                ToggleJMoving(true);
                break;
            }

            // Делаем четыре трилатерации а итог усредняем
            rMoveOutput.text = "kX = " + kX + "; kZ = " + kZ + ";\n";
            rMoveOutput.text += "wfDist[0] = " + wfDist[0] + ";\n wfDist[1] = " + wfDist[1] + ";\n";
            rMoveOutput.text += "wfDist[2] = " + wfDist[2] + ";\n wfDist[3] = " + wfDist[3] + ";\n";
            // 1
            float x0 =  (wfDist[0] * wfDist[0] - wfDist[3] * wfDist[3]) / (2 * xDist * kX * kX);
            float z0 =  (wfDist[0] * wfDist[0] - wfDist[1] * wfDist[1]) / (2 * zDist * kZ * kZ);
            // 2
            float x1 =  (wfDist[1] * wfDist[1] - wfDist[2] * wfDist[2]) / (2 * xDist * kX * kX);
            float z1 = -(wfDist[1] * wfDist[1] - wfDist[0] * wfDist[0]) / (2 * zDist * kZ * kZ);
            // 3
            float x2 = -(wfDist[2] * wfDist[2] - wfDist[1] * wfDist[1]) / (2 * xDist * kX * kX);
            float z2 = -(wfDist[2] * wfDist[2] - wfDist[3] * wfDist[3]) / (2 * zDist * kZ * kZ);
            // 4
            float x3 = -(wfDist[3] * wfDist[3] - wfDist[0] * wfDist[0]) / (2 * xDist * kX * kX);
            float z3 =  (wfDist[3] * wfDist[3] - wfDist[2] * wfDist[2]) / (2 * zDist * kZ * kZ);

            newXPos = Math.Min(Math.Max((x0 + x1 + x2 + x3) / 4, -4), 4);
            newZPos = Math.Min(Math.Max((z0 + z1 + z2 + z3) / 4, -8), 8);

            rMoveOutput.text += "x0 = " + x0 + ";\n x1 = " + x1 + ";\n x2 = " + x2 + ";\n x3 = " + x3 + ";\n";
            rMoveOutput.text += "z0 = " + z0 + ";\n z1 = " + z1 + ";\n z2 = " + z2 + ";\n z3 = " + z3 + ";\n";
            rMoveOutput.text += "newXPos = " + newXPos + ";\n newZPos = " + newZPos + ";\n";



            Player.transform.position = Vector3.Lerp(Player.transform.position, new Vector3(newXPos, Player.transform.position.y, newZPos), Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        StopCoroutine(RMoving());
        yield break;
    }
}