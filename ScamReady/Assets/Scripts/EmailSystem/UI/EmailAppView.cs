using ScamReady.Scenarios;
using UnityEngine;
using ScamReady.UI;

namespace ScamReady.Email
{
    /// <summary>绑定收件箱列表与详情。挂在常驻 EmailUI 上，关闭 EmailWindow 时保留订阅。</summary>
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

        // 用于避免同一 ScenarioSession 在多次 Refresh 时重复播放邮件提示音。
        private object lastSession;

        private void Awake()
        {
            closeButton.onClick.AddListener(controller.CloseEmail);
            inboxButton.onClick.AddListener(controller.OpenEmail);
            detail.Connect(controller);

            if (audioPlayer == null)
            {
                audioPlayer = GetComponentInParent<UIAudioPlayer>();
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
            RefreshNotification();
            RefreshContent();
            SetVisible(controller.Session != null && controller.Session.IsEmailOpen);
        }

        /// <summary>新会话首次出现邮件时播放一次提示音，后续状态刷新不重复播放。</summary>
        private void RefreshNotification()
        {
            if (controller.Session == null)
                return;

            if (ReferenceEquals(lastSession, controller.Session))
                return;

            lastSession = controller.Session;
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

        /// <summary>仅显示邮件窗口；开窗动效和音效在这里接入。玩法操作调用 Controller。</summary>
        public void Show()
        {
            if (window.activeSelf) return;

            window.SetActive(true);
            audioPlayer?.PlayOpen();
            Debug.Log($"Email Show audioPlayer found: {audioPlayer != null}");
        }

        /// <summary>仅隐藏邮件窗口；关闭动效和音效在这里接入。</summary>
        public void Hide()
        {
            if (!window.activeSelf) return;

            audioPlayer?.PlayClose();
            window.SetActive(false);
            Debug.Log($"Email Hide audioPlayer found: {audioPlayer != null}");
        }
    }
}
