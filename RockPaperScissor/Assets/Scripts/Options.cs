using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public Button buttonMusicOn, buttonMusicOff, buttonSoundsOn, buttonSoundsOff;

    public static bool isMusicOn, isSoundsOn;

    public AudioClip audio1, audio2, audio3;
    List<AudioClip> audioList;
    AudioSource audioSource;
    int lastAudioTrack;

    void Start()
    {
        isMusicOn = true;
        buttonMusicOn.gameObject.SetActive(isMusicOn);
        buttonMusicOff.gameObject.SetActive(!isMusicOn);
        isSoundsOn = true;
        buttonSoundsOn.gameObject.SetActive(isSoundsOn);
        buttonSoundsOff.gameObject.SetActive(!isSoundsOn);

        audioList = new List<AudioClip>();
        audioList.Add(audio1);
        audioList.Add(audio2);
        audioList.Add(audio3);
        lastAudioTrack = 0;

        audioSource = GetComponent<AudioSource>();
        if (!audioSource.isPlaying && isMusicOn)
        {
            audioSource.clip = audioList[lastAudioTrack];
            audioSource.Play();
        }
    }

    void Update()
    {
        if (!audioSource.isPlaying && isMusicOn)
        {
            lastAudioTrack++;
            if (lastAudioTrack >= audioList.Count)
                lastAudioTrack -= audioList.Count;

            audioSource.clip = audioList[lastAudioTrack];
            audioSource.Play();
        }
    }

    public void PressMuteMusic()
    {
        isMusicOn = !isMusicOn;

        //Изменение картинки музыки
        buttonMusicOn.gameObject.SetActive(isMusicOn);
        buttonMusicOff.gameObject.SetActive(!isMusicOn);

        if (isMusicOn)
        {
            lastAudioTrack++;
            if (lastAudioTrack >= audioList.Count)
                lastAudioTrack -= audioList.Count;

            audioSource.clip = audioList[lastAudioTrack];
            audioSource.Play();
        }
        else
            audioSource.Stop();
    }

    public void PressMuteSounds()
    {
        isSoundsOn = !isSoundsOn;

        //Изменение картинки звуков
        buttonSoundsOn.gameObject.SetActive(isSoundsOn);
        buttonSoundsOff.gameObject.SetActive(!isSoundsOn);
    }
}
