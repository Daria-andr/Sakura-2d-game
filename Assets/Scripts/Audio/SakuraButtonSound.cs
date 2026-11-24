using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
public sealed class SakuraButtonSound : MonoBehaviour
{
    void OnEnable() { GetComponent<Button>().onClick.AddListener(Click); }
    void OnDisable() { GetComponent<Button>().onClick.RemoveListener(Click); }
    void Click() { SakuraAudio.Play(SakuraAudio.Cue.Click); }
}
