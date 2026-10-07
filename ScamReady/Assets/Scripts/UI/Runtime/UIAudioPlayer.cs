using UnityEngine;

namespace ScamReady.UI
{
    public class UIAudioPlayer : MonoBehaviour
    {
        [Header("Audio Source")]
        [SerializeField] private AudioSource audioSource;

        [Header("UI Sounds")]
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;
        [SerializeField] private AudioClip notificationClip;
        [SerializeField] private AudioClip evidenceCollectedClip;
        [SerializeField] private AudioClip successClip;
        [SerializeField] private AudioClip warningClip;
        

        [Range(0f, 1f)]
        [SerializeField] private float volume = 1f;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
        }

        public void PlayClick()
        {
            Play(clickClip);
        }

        public void PlayOpen()
        {
            Play(openClip);
        }

        public void PlayClose()
        {
            Play(closeClip);
        }

        public void PlayNotification()
        {
            Play(notificationClip);
        }

        public void PlayEvidenceCollected()
        {
            Play(evidenceCollectedClip);
        }

        public void PlaySuccess()
        {
            Play(successClip);
        }

        public void PlayWarning()
        {
            Play(warningClip);
        }

        

        private void Play(AudioClip clip)
        {
            if (audioSource == null || clip == null)
            {
                return;
            }

            audioSource.PlayOneShot(clip, volume);
        }
    }
}

