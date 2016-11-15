using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public Button startButton;
    public Button quitButton;

	void Start ()
    {
        startButton = startButton.GetComponent<Button> ();
        quitButton  =  quitButton.GetComponent<Button> ();
    }

    public void StartPress()
    {
        SceneManager.LoadScene("Dungeon");
    }

    public void QuitPress()
    {
        Application.Quit();
    }
}
