using UnityEngine;
using UnityEngine.UI;

namespace ScamReady.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        [SerializeField] private UIAudioPlayer audioPlayer;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();

            if (audioPlayer == null)
            {
                audioPlayer = GetComponentInParent<UIAudioPlayer>();
            }

            button.onClick.AddListener(PlayClickSound);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(PlayClickSound);
            }
        }

        private void PlayClickSound()
        {
            Debug.Log("UIButtonSound clicked");
            if (audioPlayer != null)
            {
                audioPlayer.PlayClick();
            }
        }
    }
}