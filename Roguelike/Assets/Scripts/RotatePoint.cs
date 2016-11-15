using UnityEngine;
using System.Collections;

public class RotatePoint : MonoBehaviour
{
    static Transform _transform;
    static bool waiting;

    void Start()
    {
        _transform = transform;
        waiting = false;
    }

    void Update()
    {
        if (( SceneOptions.IsButtonWASD() || SceneOptions.IsButtonQE(false) ) && !waiting)
        {
            //назначаем место/угол передвижения
            switch (SceneOptions.GetPressedButton())
            {
                case 'q':
                    transform.Translate(Vector3.left * SceneOptions.GetTile());
                    transform.Translate(Vector3.back * SceneOptions.GetTile());
                    transform.rotation *= Quaternion.Euler(0f, -90, 0f);
                    break;
                case 'e':
                    transform.Translate(Vector3.right * SceneOptions.GetTile());
                    transform.Translate(Vector3.back  * SceneOptions.GetTile());
                    transform.rotation *= Quaternion.Euler(0f, 90, 0f);
                    break;
                case 'w':
                    transform.Translate(Vector3.forward * SceneOptions.GetTile());
                    break;
                case 'a':
                    transform.Translate(Vector3.left * SceneOptions.GetTile());
                    break;
                case 's':
                    transform.Translate(Vector3.back * SceneOptions.GetTile());
                    break;
                case 'd':
                    transform.Translate(Vector3.right * SceneOptions.GetTile());
                    break;
            }

            waiting = true;
        }
    }

    public static void ReadyToRotate()
    {
        waiting = false;
    }

    public static bool IsWaiting()
    {
        return waiting;
    }

    public static Transform GetTransform()
    {
        return _transform;
    }
}
