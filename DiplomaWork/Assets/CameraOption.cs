using System.Collections.Generic;
using UnityEngine;

public class CameraOption : MonoBehaviour
{
    public GameObject player;
    public Canvas canvasDebug, canvasLogo;
    public UnityEngine.UI.Text fps;
    public UnityEngine.UI.Text textAccelerateAvailable, textAccelerationX, textAccelerationY, textAccelerationZ;
    public UnityEngine.UI.Text textCompassAvailable,    textCompassRawX,   textCompassRawY,   textCompassRawZ,   textCompassAngle;
    public UnityEngine.UI.Text textHorizontalAngle,     textVerticalAngle, textSpinAngle;

    const float logoTime = 2.0f;
    const int   smoothSize = 8;

    //time counters for FPS and DoubleTap for smartphones
    float fpsDeltaTime = 0.0f, touchLastTime = 0.0f;   
    int touchCounter = 0;  

    //current angle of camera
    float curHorizontalAngle = 0.0f, curVerticalAngle = 0.0f, curSpinAngle = 0.0f;   
    Queue<float> firstHorizontalAngleQueue, secondHorizontalAngleQueue;
    Queue<float> firstVerticalAngleQueue, secondVerticalAngleQueue;
    Queue<float> firstSpinAngleQueue, secondSpinAngleQueue;

    void Start()
    {
        //Never turn off the screen
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //Turn on sensors
        Input.location.Start();
        Input.compass.enabled = true;

        Init();
    }
    void Init()
    {
        canvasDebug.gameObject.SetActive(false);
        canvasLogo.gameObject.SetActive(true);
        
        CheckAcceleration();
        CheckCompass();

        firstHorizontalAngleQueue = new Queue<float>();
        secondHorizontalAngleQueue = new Queue<float>();
        firstVerticalAngleQueue = new Queue<float>();
        secondVerticalAngleQueue = new Queue<float>();
        firstSpinAngleQueue = new Queue<float>();
        secondSpinAngleQueue = new Queue<float>();
    }

    void Update()
    {
        //Drawing the Logo for logoTime second while camera is spining
        if (canvasLogo.gameObject.activeSelf && Time.realtimeSinceStartup > logoTime)
            canvasLogo.gameObject.SetActive(false);

        //Getting current time for fps lable
        fpsDeltaTime += (Time.deltaTime - fpsDeltaTime) * 0.1f;

        //Get pressed key Escape for return or exit
        if (Input.GetKeyDown(KeyCode.Escape))
            PressEscape();

        //Get pressed key Menu or Double click for showing or hide menu
        if (Input.GetKeyDown(KeyCode.Menu) || touchCounter >= 2)
            PressMenu();

        //Update the values in debug menu
        if (canvasDebug.gameObject.activeSelf)
            MenuDebugUpdate();

        //Rotating camera with the smartphone
        if (CheckCompass() && CheckAcceleration())
            TwoStepSmoothRotating();

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

    bool CheckCompass()
    {
        if (Input.compass.enabled)
        {
            textCompassAvailable.text = "Compass is available";
            return true;
        }
        else
        {
            textCompassAvailable.text = "Compass is NOT available";
            return false;
        }
    }
    bool CheckAcceleration()
    {
        if (Input.acceleration.x + Input.acceleration.y + Input.acceleration.z != 0)
        {
            textAccelerateAvailable.text = "Acceleration is available";
            return true;
        }
        else
        {
            textAccelerateAvailable.text = "Acceleration is NOT available";
            return false;
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

        textAccelerationX.text = Input.acceleration.x.ToString(" 0.000");
        textAccelerationY.text = Input.acceleration.y.ToString(" 0.000");
        textAccelerationZ.text = Input.acceleration.z.ToString(" 0.000");

        textCompassRawX.text = Input.compass.rawVector.x.ToString(" 000");
        textCompassRawY.text = Input.compass.rawVector.y.ToString(" 000");
        textCompassRawZ.text = Input.compass.rawVector.z.ToString(" 000");
        textCompassAngle.text = Input.compass.magneticHeading.ToString(" 000.00");

        textVerticalAngle.text   = curVerticalAngle.ToString(" 000.00");
        textHorizontalAngle.text = curHorizontalAngle.ToString(" 000.00");
        textSpinAngle.text       = curSpinAngle.ToString(" 000.00");
    }

    void TwoStepSmoothRotating()
    {
        if (firstHorizontalAngleQueue.Count >= smoothSize)
        {
            firstHorizontalAngleQueue.Dequeue();
            secondHorizontalAngleQueue.Dequeue();
            firstVerticalAngleQueue.Dequeue();
            secondVerticalAngleQueue.Dequeue();
            firstSpinAngleQueue.Dequeue();
            secondSpinAngleQueue.Dequeue();
        }

        firstHorizontalAngleQueue.Enqueue(Input.compass.magneticHeading);
        secondHorizontalAngleQueue.Enqueue(QueuePolarMedian(firstHorizontalAngleQueue));
        float horizontalMedian = QueuePolarMedian(secondHorizontalAngleQueue);

        firstVerticalAngleQueue.Enqueue(Input.acceleration.z * -90);
        secondVerticalAngleQueue.Enqueue(QueueMedian(firstVerticalAngleQueue));
        float verticalMedian = QueueMedian(secondVerticalAngleQueue);

        firstSpinAngleQueue.Enqueue(Input.acceleration.x * -90);
        secondSpinAngleQueue.Enqueue(QueueMedian(firstSpinAngleQueue));
        float spinMedian = QueueMedian(secondSpinAngleQueue);

        player.transform.Rotate(verticalMedian - player.transform.rotation.eulerAngles.x, horizontalMedian - player.transform.rotation.eulerAngles.y, spinMedian - player.transform.rotation.eulerAngles.z);
        curHorizontalAngle = horizontalMedian;
        curVerticalAngle   = verticalMedian;
        curSpinAngle       = spinMedian;
    }

    float QueueMedian(Queue<float> q)
    {
        float s = 0;
        foreach (float i in q)
            s += i;
        return s / q.Count;
    }
    float QueuePolarMedian(Queue<float> q)
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
}
