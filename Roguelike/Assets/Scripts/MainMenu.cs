using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Canvas canvasLogo, canvasMainMenu, canvasOption, canvasScore;
    public Button resumeOn, resumeOff;

    bool isPlayerAlive;

	void Start()
    {
        isPlayerAlive = false;  //Check saves

        resumeOn.gameObject.SetActive(isPlayerAlive);
        resumeOff.gameObject.SetActive(!isPlayerAlive);

        canvasLogo.gameObject.SetActive(true);
        canvasMainMenu.gameObject.SetActive(false);
        canvasOption.gameObject.SetActive(false);
        canvasScore.gameObject.SetActive(false);
    }

    void Update()
    {
        if (canvasLogo.gameObject.activeSelf && Time.realtimeSinceStartup > 2.0f)
        {
            canvasLogo.gameObject.SetActive(false);
            canvasMainMenu.gameObject.SetActive(true);
        }
    }

    public void GameNew()
    {
        SceneManager.LoadScene("Dungeon");
    }
    public void GameResume()
    { }
    public void OptionsShow()
    {
        canvasMainMenu.gameObject.SetActive(false);
        canvasOption.gameObject.SetActive(true);
    }
    public void OptionsHide()
    {
        canvasOption.gameObject.SetActive(false);
        canvasMainMenu.gameObject.SetActive(true);
    }
    public void ScoresShow()
    {
        canvasMainMenu.gameObject.SetActive(false);
        canvasScore.gameObject.SetActive(true);
    }
    public void ScoresHide()
    {
        canvasScore.gameObject.SetActive(false);
        canvasMainMenu.gameObject.SetActive(true);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
