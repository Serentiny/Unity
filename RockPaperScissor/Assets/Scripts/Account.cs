using System;
using System.IO;
using UnityEngine;

public enum Moves
{
    Rock = 1,
    Paper = 2,
    Scissors = 3
}

public enum LoadMessage
{
    Continue = 0,
    New = 1
}

public class Account : MonoBehaviour
{
    public Canvas canvasNewAccount, canvasMainMenu;

    static string path;
    static string playerName;
    static int winScores, looseScores;
    static int lastMoves;
    static int imageNum, playerNum;

    const int windowMovesSize = 5;

    //=====================================================================================================================

    void Start()
    {
        path = Application.persistentDataPath;
    }

    void Update()
    {
    }

    //=====================================================================================================================

    public static LoadMessage LoadAccount(int num)
    {
        playerNum = num;
        StreamReader sr = new StreamReader(path + "/player" + playerNum + ".txt", true);
        playerName = sr.ReadLine();                         //"Serentiny"
        if (playerName == "")
        {
            sr.Close();
            return LoadMessage.New;   //Создаем нового пользователя
        }

        Int32.TryParse(sr.ReadLine(), out winScores);       //15
        Int32.TryParse(sr.ReadLine(), out looseScores);     //10
        Int32.TryParse(sr.ReadLine(), out lastMoves);       //00123 == [_ _ К Н Б]
        Int32.TryParse(sr.ReadLine(), out imageNum);        //2 == Номер картинки
        sr.Close();
        return LoadMessage.Continue;   //Выбрали пользователя - продолжаем игру
    }
    public static void SaveAccount()
    {
        StreamWriter sw = new StreamWriter(path + "/player" + playerNum + ".txt", false);
        sw.WriteLine(playerName);

        sw.WriteLine(winScores.ToString());
        sw.WriteLine(looseScores.ToString());
        sw.WriteLine(lastMoves.ToString());
        sw.WriteLine(imageNum.ToString());
        sw.Close();
    }

    public static string GetPlayerName()
    {
        return playerName;
    }
    public static int GetWindowMovesSize()
    {
        return windowMovesSize;
    }
    public static int GetWinScores()
    {
        return winScores;
    }
    public static int GetLooseScores()
    {
        return looseScores;
    }
    public static int GetLastMoves()
    {
        return lastMoves;
    }
    public static int GetImageNum()
    {
        return imageNum;
    }

    public static void SetPlayerName(string name)
    {
        playerName = name;
    }
    public static void SetDefaultValues()
    {
        winScores = 0;
        looseScores = 0;
        lastMoves = 123;
        imageNum = 1;
    }
    public static void AddWinScore()
    {
        winScores++;
    }
    public static void AddLooseScore()
    {
        looseScores++;
    }
    public static void AddNewMove(int move)
    {
        lastMoves *= 10;
        switch (move)
        {
            case 1:
            case 2:
            case 3:
                lastMoves += move;
                break;
            default:
                lastMoves += 0;
                break;
        }

        int cut = (int)Math.Pow(10, windowMovesSize);
        lastMoves %= cut;
    }
    public static void NextAvatarImage()
    {
        imageNum++;
        if (imageNum > MenuScripts.spriteListSize)
            imageNum = 1;
        SaveAccount();
    }
    public static void PrevAvatarImage()
    {
        imageNum--;
        if (imageNum == 0)
            imageNum = MenuScripts.spriteListSize;
        SaveAccount();
    }
    
}
