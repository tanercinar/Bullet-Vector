using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    //Sesleri yöneten mixer
    public AudioMixer mainMixer;
    //Müzik sesini ayarlayan slider
    public Slider musicSlider; 
    //Ses efektlerini ayarlayan slider
    public Slider sfxSlider;

    void Start()
    {
        //Daha önceden ayarlanan bir ayar varsa onu al yoksa 0.5 olarak ayarla
        float savedMusicVal = PlayerPrefs.GetFloat("MusicSetting", 0.5f);
        float savedSFXVal = PlayerPrefs.GetFloat("SFXSetting", 0.5f);
        //Sliderları ayarla
        if (musicSlider != null)
        {
            musicSlider.value = savedMusicVal;
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFXVal;
        }
        //Mikserin ses seviyesini ayarla
        SetMusicVolume(savedMusicVal);
        SetSFXVolume(savedSFXVal);
    }

    //START butonuna basınca test sahnesini yükle
    public void StartGame()
    {
        SceneManager.LoadScene("TestLevel");
    }
    //Müzik sliderı değiştiğinde çalışır
    public void SetMusicVolume(float volume)
    {
        //Logaritmik hatayı önlemek için slider 0 yerine çok küçük bir değere ayarlıyoruz
        if (volume <= 0) volume = 0.0000001f;
        //Slider 0-1 arasında ses ise -80Db ile 0Db arasında değiştiği için logaritmik dönüşüm yapıyoruz
        mainMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        //Yeni ayarı kaydet
        PlayerPrefs.SetFloat("MusicSetting", volume);
    }

    public void SetSFXVolume(float volume)
    {
        //Logaritmik hatayı önlemek için slider 0 yerine çok küçük bir değere ayarlıyoruz
        if (volume <= 0) volume = 0.0000001f;
        //Slider 0-1 arasında ses ise -80Db ile 0Db arasında değiştiği için logaritmik dönüşüm yapıyoruz
        mainMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        //Yeni ayarı kaydet
        PlayerPrefs.SetFloat("SFXSetting", volume);
    }
}