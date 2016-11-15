using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    static float f_step, f_angle;
    public static int rot_dir;

    void Start()
    {
        f_angle = 0F;
        f_step = 0F;
        rot_dir = 0;
    }

    void Update()
    {
        //Движение и повороты по клеткам
        if ( SceneOptions.IsButtonWASD() || SceneOptions.IsButtonQE(false) )
        {
            //поворачиваемся в нужную сторону
            if ( SceneOptions.IsButtonQE(false) && RotatePoint.IsWaiting() )
            {
                f_angle += Time.deltaTime * SceneOptions.GetPlayerRotateSpeed();
                Transform rotateTarget  = RotatePoint.GetTransform();           //TODO Можно уменьшить количество действий на одно за весь поворот
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateTarget.rotation, Time.deltaTime * SceneOptions.GetPlayerRotateSpeed());
            }

            //двигаемся по указанной позиции
            if ( SceneOptions.IsButtonWASD() && WayPoint.IsWaiting())
            {
                f_step += Time.deltaTime * SceneOptions.GetPlayerMoveSpeed();
                Transform moveTarget = WayPoint.GetTransform();                 //TODO Можно уменьшить количество действий на одно за весь поворот
                transform.position = Vector3.MoveTowards(transform.position, moveTarget.position, Time.deltaTime * SceneOptions.GetPlayerMoveSpeed());
            }
        }
    }

    public static float GetStep()
    {
        return f_step;
    }

    public static float GetAngle()
    {
        return f_angle;
    }

    public static void ResetTransformCounter()
    {
        f_angle = 0F;
        f_step  = 0F;
    }
}
