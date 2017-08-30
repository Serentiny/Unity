using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public Button buttonMusicOn, buttonMusicOff, buttonSoundsOn, buttonSoundsOff;

    static bool isMusicOn, isSoundsOn;

    void Start()
    {
        isMusicOn = true;
        buttonMusicOn.gameObject.SetActive(isMusicOn);
        buttonMusicOff.gameObject.SetActive(!isMusicOn);
        isSoundsOn = true;
        buttonSoundsOn.gameObject.SetActive(isSoundsOn);
        buttonSoundsOff.gameObject.SetActive(!isSoundsOn);
    }

    public void PressMuteMusic()
    {
        isMusicOn = !isMusicOn;

        //Изменение картинки музыки
        buttonMusicOn.gameObject.SetActive(isMusicOn);
        buttonMusicOff.gameObject.SetActive(!isMusicOn);

//      buttonMusic.transform.Find("Text").GetComponent<Text>().text = "Music is " + (isMusicOn ? "on" : "off");
    }

    public void PressMuteSounds()
    {
        isSoundsOn = !isSoundsOn;

        //Изменение картинки звуков
        buttonSoundsOn.gameObject.SetActive(isSoundsOn);
        buttonSoundsOff.gameObject.SetActive(!isSoundsOn);

//      buttonSounds.transform.Find("Text").GetComponent<Text>().text = "Sounds are " + (isSoundsOn ? "on" : "off");
    }
}
