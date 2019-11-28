using System;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneOptions : MonoBehaviour
{
    public enum MoveDirection
    {
        Forward = 0,    // [w] or ( 0, 0,  1)
        Right = 1,      // [d] or ( 1, 0,  0)
        Back = 2,       // [s] or ( 0, 0, -1)
        Left = 3,       // [a] or (-1, 0,  0)
    }
    static int sizeMoveDirections = Enum.GetNames(typeof(MoveDirection)).Length;
    public enum RotationDirection
    {
        Left = -1,  // [q]
        Right = 1,  // [e]
    }
    enum KeyAction
    {
        NoAction,

        MoveForward,    // [w]
        MoveRight,      // [d]
        MoveBack,       // [s]
        MoveLeft,       // [a]

        RotateRight,    // [e]
        RotateLeft,     // [q]
    }

    static int tile = 10;
    static int player_mov_speed, player_rot_speed;
    static KeyAction keyboardAction;

    void Start ()
    {
        keyboardAction = KeyAction.NoAction;
        player_mov_speed = 25;
        player_rot_speed = 250;
    }

    void Update()
    {
        UpdateKeys();
        if (!CanMove())
            ResetKeyAction();
        
        if (IsAnyKeyAction())
        {
            if (IsMovingKeyAction())
            {
                if (Player.QueueMoving(KeyActionToMoveDirection(keyboardAction)))
                    Dungeon.Map.MovePlayer(GetMoveDirection());
            }
            else if (IsRotationKeyAction())
            {
                Player.QueueRotating(KeyActionToRotationDirection(keyboardAction));
            }
            ResetKeyAction();
        }

        if (Input.GetKey("escape"))
            KeyActionEscape();
    }

    static void UpdateKeys()
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
    }
    static bool CanMove()
    {
        if (IsMovingKeyAction())
        {
            return Dungeon.Map.CanMove(GetMoveDirection());
        }
        return true;
    }

    public static int GetMoveDirection()
    {
        return (int)SumEnumMoveDirection(Player.GetCameraDir(), KeyActionToMoveDirection(keyboardAction));
    }

    // Enum methods
    public static MoveDirection SumEnumMoveDirection(MoveDirection mDir1, MoveDirection mDir2)
    {
        int result = (int)mDir1 + (int)mDir2;
        return (MoveDirection)(result % sizeMoveDirections);
    }
    public static MoveDirection RotateEnumMoveDirection(MoveDirection mDir, RotationDirection rDir)
    {
        int result = (int)mDir + (int)rDir;
        if (result < 0)
            result += sizeMoveDirections;
        return (MoveDirection)(result % sizeMoveDirections);
    }
    public static Vector3 EnumMoveDirectionToVector3(MoveDirection mDir)
    {
        switch (mDir)
        {
            case MoveDirection.Forward:
                return Vector3.forward;
            case MoveDirection.Right:
                return Vector3.right;
            case MoveDirection.Back:
                return Vector3.back;
            case MoveDirection.Left:
                return Vector3.left;
            default:
                return Vector3.zero;
        }
    }
    static MoveDirection KeyActionToMoveDirection(KeyAction action)
    {
        switch (action)
        {
            case KeyAction.MoveForward:
                return MoveDirection.Forward;
            case KeyAction.MoveRight:
                return MoveDirection.Right;
            case KeyAction.MoveBack:
                return MoveDirection.Back;
            case KeyAction.MoveLeft:
                return MoveDirection.Left;
            default:
                return MoveDirection.Forward;
        }
    }
    static RotationDirection KeyActionToRotationDirection(KeyAction action)
    {
        switch (action)
        {
            case KeyAction.RotateLeft:
                return RotationDirection.Left;
            case KeyAction.RotateRight:
                return RotationDirection.Right;
            default:
                return RotationDirection.Right;
        }
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
    public void PressEscape()
    {
        KeyActionEscape();
    }

    // Keyboard actions
    static void KeyActionRotateLeft()
    {
        keyboardAction = KeyAction.RotateLeft;
    }
    static void KeyActionRotateRight()
    {
        keyboardAction = KeyAction.RotateRight;
    }
    static void KeyActionMoveForward()
    {
        keyboardAction = KeyAction.MoveForward;
    }
    static void KeyActionMoveLeft()
    {
        keyboardAction = KeyAction.MoveLeft;
    }
    static void KeyActionMoveBack()
    {
        keyboardAction = KeyAction.MoveBack;
    }
    static void KeyActionMoveRight()
    {
        keyboardAction = KeyAction.MoveRight;
    }
    static void ResetKeyAction()
    {
        keyboardAction = KeyAction.NoAction;
    }
    static void KeyActionEscape()
    {
        SceneManager.LoadScene("MainMenu");
    }

    static bool IsAnyKeyAction()
    {
        return (keyboardAction != KeyAction.NoAction) ? true : false;
    }
    static bool IsMovingKeyAction()
    {
        return (
            keyboardAction == KeyAction.MoveForward ||
            keyboardAction == KeyAction.MoveLeft ||
            keyboardAction == KeyAction.MoveBack ||
            keyboardAction == KeyAction.MoveRight
            ) ? true : false;
    }
    static bool IsRotationKeyAction()
    {
        return (
            keyboardAction == KeyAction.RotateLeft ||
            keyboardAction == KeyAction.RotateRight
            ) ? true : false;
    }

    // Setting getters
    public static int GetPlayerMoveSpeed()
    {
        return player_mov_speed;
    }
    public static int GetPlayerRotateSpeed()
    {
        return player_rot_speed;
    }
    public static int GetTileLength()
    {
        return tile;
    }
}
