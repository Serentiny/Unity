using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public enum Moves
{
    Rock = 1,
    Paper = 2,
    Scissors = 3
}

public class Account : MonoBehaviour
{
    public InputField inputName;
    public Text textError;
    public Canvas canvasNewAccount, canvasEditAccount, canvasMainMenu;
    public Image avatar;
    public Text avatarPlayerName;

    static string path;
    static string playerName;
    static int spWin, spLoose;
    static int lastMoves;

    const int windowMovesSize = 5;

    //=====================================================================================================================

    void Start()
    {
        textError.gameObject.SetActive(false);
    }

    void Update()
    {
        avatarPlayerName.text = playerName;
    }

    //=====================================================================================================================

    public static void NewAccount(ref string fileName)
    {
        path = fileName;
    }
    public static void LoadAccount(ref string fileName)
    {
        path = fileName;
        StreamReader sr = new StreamReader(path, true);
        playerName = sr.ReadLine();                         //"Serentiny"
                                                                //TODO: Номер картинки
        Int32.TryParse(sr.ReadLine(), out spWin);           //15
        Int32.TryParse(sr.ReadLine(), out spLoose);         //10
        Int32.TryParse(sr.ReadLine(), out lastMoves);       //"00123" == [_ _ К Н Б]

        MenuScripts.SetPlayerName(ref playerName);
    }

    public static string GetName()
    {
        return playerName;
    }
    public static int GetSpWin()
    {
        return spWin;
    }
    public static int GetSpLoose()
    {
        return spLoose;
    }
    public static int GetLastMoves()
    {
        return lastMoves;
    }
    public static int GetWindowMovesSize()
    {
        return windowMovesSize;
    }
    public static void AddSpWin()
    {
        spWin++;
    }
    public static void AddSpLoose()
    {
        spLoose++;
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

    public static void QuickSave()
    {
        StreamWriter sw = new StreamWriter(path, false);
        sw.WriteLine(playerName);
                                                                //TODO: Номер картинки
        sw.WriteLine(spWin.ToString());
        sw.WriteLine(spLoose.ToString());
        sw.WriteLine(lastMoves.ToString());
        sw.Close();
    }

    //=====================================================================================================================

    public void PressSaveAccount()
    {
        textError.gameObject.SetActive(false);

        if (!IsPlayerNameGood(inputName.text))
            return;

        playerName = inputName.text;
        spWin = 0;
        spLoose = 0;
        lastMoves = 123;

        QuickSave();

        MenuScripts.SetPlayerName(ref playerName);
        canvasNewAccount.gameObject.SetActive(false);
        canvasMainMenu.gameObject.SetActive(true);
    }
    public void PressBack()
    {
        canvasEditAccount.gameObject.SetActive(false);
        canvasMainMenu.gameObject.SetActive(true);
    }

    bool IsPlayerNameGood(string name)
    {
        if (name.Length < 3)
        {
            textError.text = "Too short name." + "\n" + "Need more than 3 letters.";
            textError.gameObject.SetActive(true);
            return false;
        }
        if (name.Length > 16)
        {
            textError.text = "Too long name." + "\n" + "Need less than 16 letters.";
            textError.gameObject.SetActive(true);
            return false;
        }
        if (!Regex.IsMatch(name, "[a-zA-Z]"))
        {
            textError.text = "Wrong language." + "\n" + "Type Name in English.";
            textError.gameObject.SetActive(true);
            return false;
        }

        return true;
    }
}
