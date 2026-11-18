using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressMenu : MonoBehaviour
{
    public GameObject levelsPanel, settingsPanel, confirmPanel;
    public Button[] levelButtons;
    public TMP_Text[] levelLabels;
    public Slider volumeSlider;
    public TMP_Text volumeLabel, statusLabel;
    void OnEnable() { SaveSystem.EnsureLoaded(); SaveSystem.Changed += Refresh; Refresh(); }
    void OnDisable() { SaveSystem.Changed -= Refresh; }
    public void Refresh()
    {
        if(levelButtons != null) for(int i=0;i<levelButtons.Length;i++) {
            var colors = levelButtons[i].colors; colors.disabledColor = Color.white; levelButtons[i].colors = colors;
            levelButtons[i].interactable = i <= SaveSystem.Data.highestUnlocked;
            levelLabels[i].text = "Level " + (i+1) + (i == 2 ? " - Boss" : "") +
                (i > SaveSystem.Data.highestUnlocked ? " / Locked" :
                 SaveSystem.Data.completed[i] ? " / Complete / Best: " + SaveSystem.Data.bestScores[i] : " / Ready");
        }
        if(volumeSlider != null) volumeSlider.SetValueWithoutNotify(SaveSystem.Data.volume);
        if(volumeLabel != null) volumeLabel.text = "Master volume: " + Mathf.RoundToInt(SaveSystem.Data.volume*100) + "%";
        if(statusLabel != null) statusLabel.text = SaveSystem.LastError ?? "Progress saved automatically";
    }
    public void OpenLevels() { levelsPanel.SetActive(true); Refresh(); }
    public void CloseLevels() { levelsPanel.SetActive(false); }
    public void OpenSettings() { settingsPanel.SetActive(true); Refresh(); }
    public void CloseSettings() { confirmPanel.SetActive(false); settingsPanel.SetActive(false); }
    public void RequestReset() { confirmPanel.SetActive(true); }
    public void CancelReset() { confirmPanel.SetActive(false); }
    public void ConfirmReset() { SaveSystem.ResetProgress(); confirmPanel.SetActive(false); Refresh(); }
    public void VolumeChanged(float value) { SaveSystem.SetVolume(value); }
    public void Level1() { SaveSystem.PlayLevel(0); }
    public void Level2() { SaveSystem.PlayLevel(1); }
    public void Level3() { SaveSystem.PlayLevel(2); }
}

