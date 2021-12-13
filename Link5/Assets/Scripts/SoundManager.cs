using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource audioSource;
    private AudioSource bgAudioSource;

    public AudioClip ChipSFX;
    public AudioClip WrongMoveSFX;
    public AudioClip ClickSFX;
    //public Slider volumeSlider;
    //public Slider soundEffectVolumeSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Instance = null;
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //bgAudioSource = GameObject.Find("Main Camera").GetComponent<AudioSource>();
        //volumeSlider.value = PlayerPrefs.GetFloat("volume");
        //soundEffectVolumeSlider.value = PlayerPrefs.GetFloat("soundEffectVolume");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleAudio()
    {

    }

    public void PlaySFX(AudioClip audioClip)
    {
        audioSource.PlayOneShot(audioClip);
    }

    //mahin
    public void SetVolume()
    {

        //bgAudioSource.volume = volumeSlider.value;
        PlayerPrefs.SetFloat("volume", bgAudioSource.volume);
    }

    public void SetSoundEffectVolume()
    {
        //audioSource.volume = soundEffectVolumeSlider.value;
        PlayerPrefs.SetFloat("soundEffectVolume", audioSource.volume);
    }
}
