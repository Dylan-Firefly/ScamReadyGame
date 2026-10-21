using ScamReady.Scenarios;
using ScamReady.Phone;
using TMPro;
using UnityEngine;

namespace ScamReady.UI
{
    /// <summary>桌面入口与新邮件提示。所有引用在场景 Inspector 中绑定。</summary>
    public sealed class ScenarioDesktopView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private UnityEngine.UI.Button emailButton;
        [SerializeField] private UnityEngine.UI.Button phoneButton;
        [SerializeField, Tooltip("绑定 DesktopPanel/BrowserShortcut 的按钮。")]
        private UnityEngine.UI.Button browserButton;
        [SerializeField] private UnityEngine.UI.Button notificationButton;
        [SerializeField] private UnityEngine.UI.Button restartButton;
        [SerializeField] private GameObject unreadBadge;
        [SerializeField] private GameObject phoneUnreadBadge;
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
            if (phoneButton != null) phoneButton.onClick.AddListener(controller.OpenPhone);
            browserButton.onClick.AddListener(controller.OpenBrowser);
            notificationButton.onClick.AddListener(OpenCurrentContact);
            restartButton.onClick.AddListener(controller.Restart);
            Refresh();
        }

        private void OnDisable()
        {
            controller.Changed -= Refresh;
            emailButton.onClick.RemoveListener(controller.OpenEmail);
            if (phoneButton != null) phoneButton.onClick.RemoveListener(controller.OpenPhone);
            browserButton.onClick.RemoveListener(controller.OpenBrowser);
            notificationButton.onClick.RemoveListener(OpenCurrentContact);
            restartButton.onClick.RemoveListener(controller.Restart);
        }

        private void Refresh()
        {
            RefreshContent();
            RefreshNotification();
            SetVisible(controller.Session == null || controller.Session.ActiveApp == ScenarioApp.Desktop);
        }

        private void OpenCurrentContact()
        {
            if (controller.IsPhoneScenario) controller.OpenPhone();
            else controller.OpenEmail();
        }

        private void RefreshContent()
        {
            dateText.text = controller.CurrentScenarioDateText ?? string.Empty;
            setupText.text = controller.CurrentScenarioSetup ?? string.Empty;
        }

        private void RefreshNotification()
        {
            bool unread = controller.IsEmailScenario
                && controller.Session != null && controller.Session.HasUnreadEmail;
            unreadBadge.SetActive(unread);
            if (phoneUnreadBadge != null)
                phoneUnreadBadge.SetActive(controller.IsPhoneScenario
                    && controller.Session != null && controller.CallState == PhoneCallState.Incoming);
            notificationText.text = controller.IsPhoneScenario
                ? controller.CallState == PhoneCallState.Incoming
                    ? "Incoming call  -  " + (controller.CurrentContactName ?? string.Empty)
                    : "Open phone"
                : unread
                    ? "1 new email  -  " + (controller.CurrentContactName ?? string.Empty)
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
            emailButton.gameObject.SetActive(active && controller.IsEmailScenario);
            browserButton.gameObject.SetActive(active && (controller.IsEmailScenario || controller.IsPhoneScenario));
            notificationButton.gameObject.SetActive(active);
            if (phoneButton != null)
                phoneButton.gameObject.SetActive(active && controller.IsPhoneScenario);
        }
    }
}
