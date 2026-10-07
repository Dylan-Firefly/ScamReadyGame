using ScamReady.Scenarios;
using TMPro;
using UnityEngine;

namespace ScamReady.UI
{
    /// <summary>桌面入口与新邮件提示。所有引用在场景 Inspector 中绑定。</summary>
    public sealed class ScenarioDesktopView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private UnityEngine.UI.Button emailButton;
        [SerializeField, Tooltip("绑定 DesktopPanel/BrowserShortcut 的按钮。")]
        private UnityEngine.UI.Button browserButton;
        [SerializeField] private UnityEngine.UI.Button notificationButton;
        [SerializeField] private UnityEngine.UI.Button restartButton;
        [SerializeField] private GameObject unreadBadge;
        [SerializeField] private TMP_Text notificationText;
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text setupText;
        [SerializeField] private GameObject setupPanel;
        private bool? displayedVisibility;

        private void OnEnable()
        {
            // 重新启用时同步全部桌面入口，不假定它们原先的显隐状态一致。
            displayedVisibility = null;
            controller.Changed += Refresh;
            emailButton.onClick.AddListener(controller.OpenEmail);
            browserButton.onClick.AddListener(controller.OpenBrowser);
            notificationButton.onClick.AddListener(controller.OpenEmail);
            restartButton.onClick.AddListener(controller.Restart);
            Refresh();
        }

        private void OnDisable()
        {
            controller.Changed -= Refresh;
            emailButton.onClick.RemoveListener(controller.OpenEmail);
            browserButton.onClick.RemoveListener(controller.OpenBrowser);
            notificationButton.onClick.RemoveListener(controller.OpenEmail);
            restartButton.onClick.RemoveListener(controller.Restart);
        }

        private void Refresh()
        {
            RefreshContent();
            RefreshNotification();
            SetVisible(controller.Session == null || controller.Session.ActiveApp == ScenarioApp.Desktop);
        }

        private void RefreshContent()
        {
            dateText.text = controller.Definition.GameDateText;
            setupText.text = controller.Definition.Setup;
        }

        private void RefreshNotification()
        {
            bool unread = controller.Session != null && controller.Session.HasUnreadEmail;
            unreadBadge.SetActive(unread);
            notificationText.text = unread
                ? "1 new email  -  " + controller.Definition.Email.Sender
                : "Open inbox";
        }

        private void SetVisible(bool visible)
        {
            if (visible) Show();
            else Hide();
        }

        /// <summary>仅显示桌面情境卡和入口，常驻栏保留。切换应用仍通过 Controller。</summary>
        public void Show()
        {
            if (displayedVisibility == true) return;
            displayedVisibility = true;
            SetDesktopElementsActive(true);
        }

        /// <summary>仅隐藏桌面情境卡和入口；切换动效和音效在这里接入。</summary>
        public void Hide()
        {
            if (displayedVisibility == false) return;
            displayedVisibility = false;
            SetDesktopElementsActive(false);
        }

        private void SetDesktopElementsActive(bool active)
        {
            setupPanel.SetActive(active);
            emailButton.gameObject.SetActive(active);
            browserButton.gameObject.SetActive(active);
            notificationButton.gameObject.SetActive(active);
        }
    }
}
