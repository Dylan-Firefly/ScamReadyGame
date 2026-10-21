using ScamReady.Scenarios;
using UnityEngine;
using ScamReady.UI;

namespace ScamReady.Email
{
    public sealed class EmailAppView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private GameObject window;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [SerializeField] private UnityEngine.UI.Button inboxButton;
        [SerializeField] private EmailInboxItemView inboxItem;
        [SerializeField] private EmailDetailView detail;

        [Header("Audio")]
        [SerializeField] private UIAudioPlayer audioPlayer;

        [Header("Animation")]
        [SerializeField] private UIWindowAnimator windowAnimator;

        private object lastSession;
        private UnityEngine.UI.ScrollRect bodyScroll;
        private bool resetScroll;

        private void Awake()
        {
            bodyScroll = window.GetComponentInChildren<UnityEngine.UI.ScrollRect>(true);
            closeButton.onClick.AddListener(controller.CloseEmail);
            inboxButton.onClick.AddListener(controller.OpenEmail);
            detail.Connect(controller);

            if (audioPlayer == null)
            {
                audioPlayer = GetComponentInParent<UIAudioPlayer>();
            }

            if (windowAnimator == null)
            {
                windowAnimator = window.GetComponent<UIWindowAnimator>();
            }
        }

        private void OnEnable()
        {
            controller.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => controller.Changed -= Refresh;

        private void Refresh()
        {
            if (!controller.IsEmailScenario || controller.Definition == null)
            {
                SetVisible(false);
                return;
            }

            RefreshNotification();
            RefreshContent();
            SetVisible(controller.Session != null && controller.Session.IsEmailOpen);
            if (resetScroll && window.activeInHierarchy && bodyScroll != null)
            {
                Canvas.ForceUpdateCanvases();
                bodyScroll.StopMovement();
                bodyScroll.verticalNormalizedPosition = 1f;
                resetScroll = false;
            }
        }

        private void RefreshNotification()
        {
            if (controller.Session == null)
                return;

            if (ReferenceEquals(lastSession, controller.Session))
                return;

            lastSession = controller.Session;
            resetScroll = true;
            audioPlayer?.PlayNotification();
        }
        private void RefreshContent()
        {
            inboxItem.Bind(controller.Definition.Email);
            detail.Bind(controller.Definition.Email, controller.Session);
        }

        private void SetVisible(bool visible)
        {
            if (visible) Show();
            else Hide();
        }

        public void Show()
        {
            if (window.activeSelf) return;

            windowAnimator.PlayOpen();
            audioPlayer?.PlayOpen();
        }

        public void Hide()
        {
            if (!window.activeSelf) return;

            audioPlayer?.PlayClose();
            windowAnimator.PlayClose();
        }
    }
}
