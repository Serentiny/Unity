using UnityEngine;
using System;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    static float moveRemain, rotateRemain, dt, dMove, dRotate;
    static SceneOptions.MoveDirection moveDir, cameraDir;
    static SceneOptions.RotationDirection rotateDir;
    static Queue<SceneOptions.MoveDirection> queueMoveDir;
    static Queue<SceneOptions.RotationDirection> queueRotateDir;

    void Start()
    {
        moveRemain   = 0F;
        rotateRemain = 0F;
        queueMoveDir = new Queue<SceneOptions.MoveDirection>();
        queueRotateDir = new Queue<SceneOptions.RotationDirection>();
        cameraDir = SceneOptions.MoveDirection.Forward;
        moveDir = SceneOptions.MoveDirection.Forward;
        rotateDir = SceneOptions.RotationDirection.Right;   // using enum as a sign
    }

    void Update()
    {
        if (rotateRemain == 0F && queueRotateDir.Count != 0)
        {
            rotateDir = queueRotateDir.Dequeue();
            rotateRemain = 90F * (int)rotateDir;
            cameraDir = SceneOptions.RotateEnumMoveDirection(cameraDir, rotateDir);
        }
        if (moveRemain == 0F && queueMoveDir.Count != 0)
        {
            moveDir = SceneOptions.SumEnumMoveDirection(cameraDir, queueMoveDir.Dequeue());
            moveRemain = SceneOptions.GetTileLength();
        }
        
        dt      = Time.deltaTime;
        dMove   = SceneOptions.GetPlayerMoveSpeed()   * dt;
        dRotate = SceneOptions.GetPlayerRotateSpeed() * dt * (int)rotateDir;
        
        if (rotateRemain != 0F || moveRemain != 0F)
        {
            if (moveRemain != 0F)
            {
                if (Math.Abs(moveRemain) < Math.Abs(dMove))
                {
                    dMove = moveRemain;
                    moveRemain = 0F;
                }
                else
                {
                    moveRemain -= dMove;
                }
                transform.position += SceneOptions.EnumMoveDirectionToVector3(moveDir) * dMove;
            }
            if (rotateRemain != 0F)
            {
                if (Math.Abs(rotateRemain) < Math.Abs(dRotate))
                {
                    dRotate = rotateRemain;
                    rotateRemain = 0F;
                }
                else
                {
                    rotateRemain -= dRotate;
                }
                transform.Rotate(0, dRotate, 0);
            }
        }
    }

    public static bool QueueMoving(SceneOptions.MoveDirection dir)
    {
        if (queueMoveDir.Count == 0)
        {
            queueMoveDir.Enqueue(dir);
            return true;
        }
        return false;
    }
    public static bool QueueRotating(SceneOptions.RotationDirection dir)
    {
        if (queueRotateDir.Count == 0)
        {
            queueRotateDir.Enqueue(dir);
            return true;
        }
        return false;
    }

    public static bool IsMoving()
    {
        return moveRemain != 0F;
    }
    public static bool IsRotating()
    {
        return rotateRemain != 0F;
    }
    public static SceneOptions.MoveDirection GetCameraDir()
    {
        return cameraDir;
    }
}