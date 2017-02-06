using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneOptions : MonoBehaviour {

    static int tile = 10;
    static int frame;
    static int player_mov_speed, player_rot_speed;
    static char c_move;

    void Start ()
    {
        frame = 0;
        c_move = '-';
        player_mov_speed = 25;
        player_rot_speed = 250;
    }

    void Update()
    {
        //подсчет кадров
        frame++;

        //Ловим нажатую клавишу
        if (Input.inputString != "" && c_move == '-')
        {
            c_move = char.ToLower(Input.inputString[0]);
            if ( !IsButtonWASD() && !IsButtonQE(true))
                ReleaseButton();
        }
        if (Input.anyKeyDown && c_move == '-')
            if (!IsArrowsDown(true))
                ReleaseButton();

        //проверяем на попытку движения в стену
        if (!CanMove())
            c_move = '-';

        //если игрок дошел - сбрасываем клавишу
        if ( IsButtonWASD() || IsButtonQE(false) )
        {
            if (Player.GetStep() > tile || Player.GetAngle() > 90f)
            {
                if (IsButtonWASD())
                    Dungeon.map.movePlayer(GetDirection());
                ReleaseButton();
                WayPoint.ReadyToMove();
                RotatePoint.ReadyToRotate();
                Player.ResetTransformCounter();
            }
        }

        if (Input.GetKey("escape"))
            SceneManager.LoadScene("MainMenu");
    }

    public void PressQ()
    {
        if (c_move == '-')
        {
            c_move = 'q';
            Player.rot_dir = (Player.rot_dir - 1) % 4;
        }
    }

    public void PressW()
    {
        if (c_move == '-')
            c_move = 'w';
    }

    public void PressE()
    {
        if (c_move == '-')
        {
            c_move = 'e';
            Player.rot_dir = (Player.rot_dir + 1) % 4;
        }
    }

    public void PressA()
    {
        if (c_move == '-')
            c_move = 'a';
    }

    public void PressS()
    {
        if (c_move == '-')
            c_move = 's';
    }

    public void PressD()
    {
        if (c_move == '-')
            c_move = 'd';
    }

    public static void ReleaseButton()
    {
        c_move = '-';
    }

    public static char GetPressedButton()
    {
        return c_move;
    }

    public static bool IsButtonWASD()
    {
        return (c_move == 'w' || c_move == 'a' || c_move == 's' || c_move == 'd') ? true : false;
    }

    public static bool IsButtonQE(bool set_rot)
    {
        if (c_move == 'q' || c_move == 'e')
        {
            if (c_move == 'q' && set_rot)
                Player.rot_dir = (Player.rot_dir - 1) % 4;
            if (c_move == 'e' && set_rot)
                Player.rot_dir = (Player.rot_dir + 1) % 4;
            return true;
        }
        return false;
    }

    public static bool IsArrowsDown(bool inMove)
    {
        if (inMove)
            if (Input.GetKeyDown("up"))
                c_move = 'w';
            else if (Input.GetKeyDown("down"))
                c_move = 's';
            else if(Input.GetKeyDown("left"))
                c_move = 'q';
            else if(Input.GetKeyDown("right"))
                c_move = 'e';
        return (Input.GetKeyDown("up") || Input.GetKeyDown("down") || Input.GetKeyDown("left") || Input.GetKeyDown("right")) ? true : false;
    }

    public static bool CanMove()
    {
        if (IsButtonWASD())
            return Dungeon.map.canMove(GetDirection());
        return true;
    }

    public static int GetPlayerMoveSpeed()
    {
        return player_mov_speed;
    }

    public static int GetPlayerRotateSpeed()
    {
        return player_rot_speed;
    }

    public static int GetTile()
    {
        return tile;
    }

    public static int GetDirection()
    {
        int dir, rot;
        switch (c_move)
        {
            case 'w':
                dir = 0;
                break;
            case 'd':
                dir = 1;
                break;
            case 's':
                dir = 2;
                break;
            case 'a':
                dir = 3;
                break;
            default:
                dir = 0;
                break;
        }
        rot = Player.rot_dir;
        return (dir + rot) % 4;
    }
}
