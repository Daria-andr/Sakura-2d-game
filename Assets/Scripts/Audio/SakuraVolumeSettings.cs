using UnityEngine;
using UnityEngine.UI;
using TMPro;
public sealed class SakuraVolumeSettings : MonoBehaviour
{
    public Slider musicSlider, sfxSlider, masterSlider;
    public TMP_Text musicLabel, sfxLabel, masterLabel;
    void OnEnable() {
        SaveSystem.EnsureLoaded();
        masterSlider.onValueChanged.AddListener(Master); musicSlider.onValueChanged.AddListener(Music); sfxSlider.onValueChanged.AddListener(Sfx);
        SaveSystem.Changed += Refresh; Refresh();
    }
    void OnDisable() {
        masterSlider.onValueChanged.RemoveListener(Master); musicSlider.onValueChanged.RemoveListener(Music); sfxSlider.onValueChanged.RemoveListener(Sfx);
        SaveSystem.Changed -= Refresh;
    }
    void Master(float v) { SaveSystem.SetVolume(v); }
    void Music(float v) { SaveSystem.SetAudioVolume(true,v); }
    void Sfx(float v) { SaveSystem.SetAudioVolume(false,v); }
    void Refresh() {
        masterSlider.SetValueWithoutNotify(SaveSystem.Data.volume);
        masterLabel.text = "Master volume: " + Mathf.RoundToInt(SaveSystem.Data.volume*100) + "%";
        musicSlider.SetValueWithoutNotify(SaveSystem.Data.musicVolume);
        sfxSlider.SetValueWithoutNotify(SaveSystem.Data.sfxVolume);
        musicLabel.text = "Music: " + Mathf.RoundToInt(SaveSystem.Data.musicVolume*100) + "%";
        sfxLabel.text = "SFX: " + Mathf.RoundToInt(SaveSystem.Data.sfxVolume*100) + "%";
    }
}

