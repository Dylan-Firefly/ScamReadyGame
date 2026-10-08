using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScamReady.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour, ISubmitHandler
    {
        [SerializeField] private UIAudioPlayer audioPlayer;

        private void Awake()
        {
            if (audioPlayer == null)
            {
                audioPlayer = GetComponentInParent<UIAudioPlayer>();
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (audioPlayer != null)
            {
                audioPlayer.PlayClick();
            }
        }
    }
}