using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour
{
    public Canvas canvasGame, canvasMap, canvasHelp;
    public GameObject helpList1, helpList2;

    private float moveRemain, rotateRemain, dt, dMove, dRotate;
    private Options.MoveDirection moveDir, cameraDir;
    private Options.RotationDirection rotateDir;
    private Queue<KeyValuePair<Options.DirectionAction, int>> queueAction;
    private Options.KeyAction keyboardAction;
    private Vector3 oldPlayerCoord;

    void Start()
    {
        ResetRemains();
        ResetDirections();
        ResetKeyAction();        
        InitCanvas();

        oldPlayerCoord = Dungeon.player.GetPos().GetVector3();
    }
    private void ResetRemains()
    {
        moveRemain   = 0F;
        rotateRemain = 0F;
    }
    private void ResetDirections()
    {
        queueAction = new Queue<KeyValuePair<Options.DirectionAction, int>>();
        cameraDir = Options.MoveDirection.Forward;
        moveDir = Options.MoveDirection.Forward;
        rotateDir = Options.RotationDirection.Right;   // using enum as a sign
    }
    private void InitCanvas()
    {
        canvasGame.gameObject.SetActive(true);
        canvasMap.gameObject.SetActive(false);
        canvasHelp.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateKeys();
        UpdateQueue();

        CheckQueue();
        MovePlayer();

        CheckNextDungeon();
    }
    private void UpdateKeys()
    {
        // Check [qe] and [wasd] keys
        if (IsAnyKeyAction())
            return;
        else if (Input.GetKeyDown("q"))
            KeyActionRotateLeft();
        else if (Input.GetKeyDown("e"))
            KeyActionRotateRight();
        else if (Input.GetKeyDown("w"))
            KeyActionMoveForward();
        else if (Input.GetKeyDown("a"))
            KeyActionMoveLeft();
        else if (Input.GetKeyDown("s"))
            KeyActionMoveBack();
        else if (Input.GetKeyDown("d"))
            KeyActionMoveRight();

        // Check arrow keys
        if (IsAnyKeyAction())
            return;
        else if (Input.GetKeyDown("up"))
            KeyActionMoveForward();
        else if (Input.GetKeyDown("down"))
            KeyActionMoveBack();
        else if (Input.GetKeyDown("left"))
            KeyActionRotateLeft();
        else if (Input.GetKeyDown("right"))
            KeyActionRotateRight();

        // Check other keys
        if (Input.GetKeyDown("escape"))
            KeyActionCancel();
        else if (Input.GetKeyDown("space"))
            KeyActionUse();
        else if (Input.GetKeyDown("m"))
            KeyActionMap();
    }
    private void UpdateQueue()
    {
        // Проверка на движение в стену
        if (IsMovingKeyAction() && !Dungeon.CanMovePlayer(GetMoveDirection()))
            ResetKeyAction();
        
        if (IsAnyKeyAction())
        {
            // Запоминаем позицию игрока чтобы отрисовать его смещение на большой карте
            oldPlayerCoord = Dungeon.player.GetPos().GetVector3();

            if (IsMovingKeyAction())
            {
                if (QueueAction(Options.DirectionAction.MoveDirection, (int)Options.KeyActionToMoveDirection(keyboardAction)))
                    Dungeon.MovePlayer(GetMoveDirection());
            }
            else if (IsRotationKeyAction())
            {
                QueueAction(Options.DirectionAction.RotationDirection, (int)Options.KeyActionToRotationDirection(keyboardAction));
            }

            UpdateFullMap();
            ResetKeyAction();
        }
    }
    private Options.MoveDirection GetMoveDirection()
    {
        return Options.SumEnumMoveDirection(cameraDir, Options.KeyActionToMoveDirection(keyboardAction));
    }
    private bool QueueAction(Options.DirectionAction type, int dir)
    {
        if (queueAction.Count == 0)
        {
            queueAction.Enqueue(new KeyValuePair<Options.DirectionAction, int>(type, dir));
            return true;
        }
        return false;
    }
    private void UpdateFullMap()
    {
        Vector3 delta = Dungeon.player.GetPos().GetVector3() - oldPlayerCoord;
        GameObject.Find("CameraMapFull").transform.Translate(delta * Options.GetTileSize());
    }

    private void CheckQueue()
    {
        if (rotateRemain == 0F && moveRemain == 0F && queueAction.Count != 0)
        {
            KeyValuePair<Options.DirectionAction, int> newAction = queueAction.Dequeue();
            if (newAction.Key == Options.DirectionAction.MoveDirection)
            {
                moveDir = Options.SumEnumMoveDirection(cameraDir, (Options.MoveDirection) newAction.Value);
                moveRemain = Options.GetTileSize();
            }
            else if (newAction.Key == Options.DirectionAction.RotationDirection)
            {
                rotateDir = (Options.RotationDirection) newAction.Value;
                rotateRemain = 90F * (int)rotateDir;
                cameraDir = Options.RotateEnumMoveDirection(cameraDir, rotateDir);
            }
        }
    }
    private void MovePlayer()
    {
        if (rotateRemain != 0F || moveRemain != 0F)
        {
            dt      = Time.deltaTime;
            dMove   = Options.GetPlayerMoveSpeed()   * dt;
            dRotate = Options.GetPlayerRotateSpeed() * dt * (int)rotateDir;

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
                transform.position += Options.EnumMoveDirectionToVector3(moveDir) * dMove;
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

    private void CheckNextDungeon()
    {
        if (transform.position.x < GameObject.Find("Cell_Exit").transform.position.x + 1 &&
            transform.position.x > GameObject.Find("Cell_Exit").transform.position.x - 1 &&
            transform.position.z < GameObject.Find("Cell_Exit").transform.position.z + 1 &&
            transform.position.z > GameObject.Find("Cell_Exit").transform.position.z - 1)
        {
            // Нужно не перезагружать, а чистить и создавать заново
            LoadScene_Dungeon();
        }
    }
    private void LoadScene_Dungeon()
    {
        SceneManager.LoadScene("Dungeon");
    }
    private void LoadScene_Menu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    // UI buttons
    public void PressRotateLeft()
    {
        KeyActionRotateLeft();
    }
    public void PressRotateRight()
    {
        KeyActionRotateRight();
    }
    public void PressMoveForward()
    {
        KeyActionMoveForward();
    }
    public void PressMoveLeft()
    {
        KeyActionMoveLeft();
    }
    public void PressMoveBack()
    {
        KeyActionMoveBack();
    }
    public void PressMoveRight()
    {
        KeyActionMoveRight();
    }
    public void PressUse()
    {
        KeyActionUse();
    }
    public void PressCancel()
    {
        KeyActionCancel();
    }

    public void MapShow()
    {
        canvasGame.gameObject.SetActive(false);
        canvasMap.gameObject.SetActive(true);
    }
    public void MapHide()
    {
        canvasMap.gameObject.SetActive(false);
        canvasGame.gameObject.SetActive(true);
    }
    public void HelpShow()
    {
        canvasGame.gameObject.SetActive(false);
        ShowHelpList1();
        canvasHelp.gameObject.SetActive(true);
    }
    public void HelpHide()
    {
        canvasHelp.gameObject.SetActive(false);
        canvasGame.gameObject.SetActive(true);
    }
    public void ShowHelpList1()
    {
        helpList2.gameObject.SetActive(false);
        helpList1.gameObject.SetActive(true);
    }
    public void ShowHelpList2()
    {
        helpList1.gameObject.SetActive(false);
        helpList2.gameObject.SetActive(true);
    }

    // Keyboard actions
    private void KeyActionRotateLeft()
    {
        keyboardAction = Options.KeyAction.RotateLeft;
    }
    private void KeyActionRotateRight()
    {
        keyboardAction = Options.KeyAction.RotateRight;
    }
    private void KeyActionMoveForward()
    {
        keyboardAction = Options.KeyAction.MoveForward;
    }
    private void KeyActionMoveLeft()
    {
        keyboardAction = Options.KeyAction.MoveLeft;
    }
    private void KeyActionMoveBack()
    {
        keyboardAction = Options.KeyAction.MoveBack;
    }
    private void KeyActionMoveRight()
    {
        keyboardAction = Options.KeyAction.MoveRight;
    }
    private void ResetKeyAction()
    {
        keyboardAction = Options.KeyAction.NoAction;
    }
    private void KeyActionUse()
    {
        // TODO:
    }
    private void KeyActionMap()
    {
        if (canvasMap.gameObject.activeSelf)
            MapHide();
        else if (canvasGame.gameObject.activeSelf)
            MapShow();
    }
    private void KeyActionCancel()
    {
        if (canvasMap.gameObject.activeSelf)
            MapHide();
        else if (canvasHelp.gameObject.activeSelf)
            HelpHide();
        else
            LoadScene_Menu();
    }

    private bool IsAnyKeyAction()
    {
        return (keyboardAction != Options.KeyAction.NoAction) ? true : false;
    }
    private bool IsMovingKeyAction()
    {
        return (
            keyboardAction == Options.KeyAction.MoveForward ||
            keyboardAction == Options.KeyAction.MoveLeft ||
            keyboardAction == Options.KeyAction.MoveBack ||
            keyboardAction == Options.KeyAction.MoveRight
            ) ? true : false;
    }
    private bool IsRotationKeyAction()
    {
        return (
            keyboardAction == Options.KeyAction.RotateLeft ||
            keyboardAction == Options.KeyAction.RotateRight
            ) ? true : false;
    }
}