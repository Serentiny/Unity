using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class MenuScripts : MonoBehaviour
{
    public Canvas canvasMainMenu, canvasNewAccount, canvasEditAccount, canvasLoadAccount, canvasGame, canvasOptions;

    // Main Menu
    public Text mainMenuPlayerName;
    public Button mainMenuAvatar;

    // Load Account
    public Image imAcc1, imAcc2, imAcc3;
    public Text textRate1, textRate2, textRate3;
    public Text textName1, textName2, textName3;

    // New Account
    public InputField inputName;
    public Text textError;

    // Edit Account
    public Image editAccountAvatar;
    public Text editAccountPlayerName;
    public Text editAccountRating;

    // Game
    public Text gamePlayerName;
    public Text gameCounter;

    // ImageList
    public Sprite spriteNew, sprite1, sprite2, sprite3, sprite4, sprite5;
    public const int spriteListSize = 5;
    List<Sprite> arrSprites;

//=====================================================================================================================

    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        arrSprites = new List<Sprite>();
        arrSprites.Add(spriteNew);
        arrSprites.Add(sprite1);
        arrSprites.Add(sprite2);
        arrSprites.Add(sprite3);
        arrSprites.Add(sprite4);
        arrSprites.Add(sprite5);

        InitVision();
        InitAccounts();
    }
    void InitVision()
    {
        canvasOptions.gameObject.SetActive(true);
        canvasLoadAccount.gameObject.SetActive(true);

        canvasMainMenu.gameObject.SetActive(false);
        canvasNewAccount.gameObject.SetActive(false);
        canvasEditAccount.gameObject.SetActive(false);
        canvasGame.gameObject.SetActive(false);
    }
    void InitAccounts()
    {
        // При запуске приложения проверяется, первый ли раз заходим
        // Если аккаунт создавался, то загружаем данные, и даем выбор аккаунта
        // Иначе создаем три стандартных
        string path = Application.persistentDataPath;

        if (!(File.Exists(path + "/player1.txt") && File.Exists(path + "/player2.txt") && File.Exists(path + "/player3.txt")))
            for (int i = 1; i <= 3; i++)
                if (!(File.Exists(path + "/player" + i + ".txt")))
                {
                    StreamWriter sw = new StreamWriter(path + "/player" + i + ".txt", false);
                    sw.WriteLine("");
                    sw.Close();
                }

        string accName;
        int spWin, spLoose, imageNum;

        StreamReader sr1 = new StreamReader(path + "/player1.txt", true);
        accName = sr1.ReadLine();
        if (accName == "")
        {
            imAcc1.sprite = spriteNew;
            textRate1.text = "";
            textName1.text = "New player";
        }
        else
        {
            Int32.TryParse(sr1.ReadLine(), out spWin);
            Int32.TryParse(sr1.ReadLine(), out spLoose);
            sr1.ReadLine();
            Int32.TryParse(sr1.ReadLine(), out imageNum);

            imAcc1.sprite = arrSprites[imageNum];
            textRate1.text = spWin + " : " + spLoose;
            textName1.text = accName;
        }
        sr1.Close();

        StreamReader sr2 = new StreamReader(path + "/player2.txt", true);
        accName = sr2.ReadLine();
        if (accName == "")
        {
            imAcc2.sprite = spriteNew;
            textRate2.text = "";
            textName2.text = "New player";
        }
        else
        {
            Int32.TryParse(sr2.ReadLine(), out spWin);
            Int32.TryParse(sr2.ReadLine(), out spLoose);
            sr2.ReadLine();
            Int32.TryParse(sr2.ReadLine(), out imageNum);

            imAcc2.sprite = arrSprites[imageNum];
            textRate2.text = spWin + " : " + spLoose;
            textName2.text = accName;
        }
        sr2.Close();

        StreamReader sr3 = new StreamReader(path + "/player3.txt", true);
        accName = sr3.ReadLine();
        if (accName == "")
        {
            imAcc3.sprite = spriteNew;
            textRate3.text = "";
            textName3.text = "New player";
        }
        else
        {
            Int32.TryParse(sr3.ReadLine(), out spWin);
            Int32.TryParse(sr3.ReadLine(), out spLoose);
            sr3.ReadLine();
            Int32.TryParse(sr3.ReadLine(), out imageNum);

            imAcc3.sprite = arrSprites[imageNum];
            textRate3.text = spWin + " : " + spLoose;
            textName3.text = accName;
        }
        sr2.Close();
    }

    void Update()
    {
        //Get pressed key Escape for return or exit
        if (Input.GetKeyDown(KeyCode.Escape))
            PressEscape();
    }

    void ShowGame()
    {
        gamePlayerName.text = Account.GetPlayerName();

        canvasGame.gameObject.SetActive(true);
    }
    void ShowEditAccount()
    {
        editAccountAvatar.sprite = arrSprites[Account.GetImageNum()];
        editAccountPlayerName.text = Account.GetPlayerName();
        editAccountRating.text = Account.GetWinScores().ToString() + '\n' + Account.GetLooseScores().ToString();

        canvasEditAccount.gameObject.SetActive(true);
    }
    void ShowMainMenu()
    {
        mainMenuAvatar.image.sprite = arrSprites[Account.GetImageNum()];
        mainMenuPlayerName.text = Account.GetPlayerName();

        canvasMainMenu.gameObject.SetActive(true);
    }

//=====================================================================================================================
    
    public void PressSaveAccount()
    {
        textError.gameObject.SetActive(false);

        if (!IsPlayerNameGood(inputName.text))
            return;

        Account.SetPlayerName(inputName.text);
        Account.SetDefaultValues();

        Account.SaveAccount();
        canvasNewAccount.gameObject.SetActive(false);
        ShowMainMenu();
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

    public void PressGame()
    {
        canvasMainMenu.gameObject.SetActive(false);
        ShowGame();
    }
    public void PressEditAccount()
    {
        canvasMainMenu.gameObject.SetActive(false);
        ShowEditAccount();
    }
    public void PressEditAccountBack()
    {
        canvasEditAccount.gameObject.SetActive(false);
        ShowMainMenu();
    }

    public void PressExit()
    {
        Application.Quit();
    }

    public void PressFirstAccount()
    {
        ChooseAcoount(Account.LoadAccount(1));
    }
    public void PressSecondAccount()
    {
        ChooseAcoount(Account.LoadAccount(2));
    }
    public void PressThirdAccount()
    {
        ChooseAcoount(Account.LoadAccount(3));
    }
    void ChooseAcoount(LoadMessage ret)
    {
        if (ret == LoadMessage.Continue)
        {
            canvasLoadAccount.gameObject.SetActive(false);
            ShowMainMenu();
        }
        else if (ret == LoadMessage.New)
        {
            canvasLoadAccount.gameObject.SetActive(false);

            textError.gameObject.SetActive(false);
            canvasNewAccount.gameObject.SetActive(true);
        }
    }

    public void PressNextAvatarImage()
    {
        Account.NextAvatarImage();
        editAccountAvatar.sprite = arrSprites[Account.GetImageNum()];
    }
    public void PressPrevAvatarImage()
    {
        Account.PrevAvatarImage();
        editAccountAvatar.sprite = arrSprites[Account.GetImageNum()];
    }
    
    //=====================================================================================================================

    void PressEscape()
    {
        if (canvasMainMenu.gameObject.activeSelf)
            Application.Quit();
        if (canvasNewAccount.gameObject.activeSelf)
            Application.Quit();
        if (canvasEditAccount.gameObject.activeSelf)
        {
            canvasEditAccount.gameObject.SetActive(false);
            canvasMainMenu.gameObject.SetActive(true);
        }
        if (canvasGame.gameObject.activeSelf)
        {
            canvasGame.gameObject.SetActive(false);
            ShowMainMenu();
        }
    }
}
