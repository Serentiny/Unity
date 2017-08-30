using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MenuScripts : MonoBehaviour
{
    public Canvas canvasMainMenu, canvasNewAccount, canvasEditAccount, canvasSingle, canvasNetwork, canvasOptions;
    public Text avatarPlayerName;

    static string playerName;

    //=====================================================================================================================

    void Start()
    {
        InitVision();
        InitAccount();        
    }

    void InitVision()
    {
        canvasOptions.gameObject.SetActive(true);
        canvasMainMenu.gameObject.SetActive(true);
        canvasNewAccount.gameObject.SetActive(false);
        canvasEditAccount.gameObject.SetActive(false);
        canvasSingle.gameObject.SetActive(false);
        canvasNetwork.gameObject.SetActive(false);
    }
    void InitAccount()
    {
        // При запуске приложения проверяется, создан ли аккаунт
            // Если аккаунт создан, то загружаем данные
            // Иначе открываем страницу создания
        string fileName = Application.persistentDataPath + "/account.txt";

        if (File.Exists(fileName))
            Account.LoadAccount(ref fileName);
        else
        {
            canvasMainMenu.gameObject.SetActive(false);
            canvasNewAccount.gameObject.SetActive(true);
            Account.NewAccount(ref fileName);
        }
    }

    void Update()
    {
        //Get pressed key Escape for return or exit
        if (Input.GetKeyDown(KeyCode.Escape))
            PressEscape();

        avatarPlayerName.text = playerName;
    }

    //=====================================================================================================================

    public static void SetPlayerName(ref string str)
    {
        playerName = str;
    }

    //=====================================================================================================================

    public void PressSingleplayer()
    {
        canvasMainMenu.gameObject.SetActive(false);
        canvasSingle.gameObject.SetActive(true);
        SinglePlayer.StartNewGame();
    }

    public void PressMultiplayer()
    {
        canvasMainMenu.gameObject.SetActive(false);
        canvasNetwork.gameObject.SetActive(true);
    }

    public void PressEditAccount()
    {
        canvasMainMenu.gameObject.SetActive(false);
        canvasEditAccount.gameObject.SetActive(true);
    }

    public void PressExit()
    {
        Application.Quit();
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
        if (canvasNetwork.gameObject.activeSelf)
        {
            canvasNetwork.gameObject.SetActive(false);
            canvasMainMenu.gameObject.SetActive(true);
        }
    }
}
