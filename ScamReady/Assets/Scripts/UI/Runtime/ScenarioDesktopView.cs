using ScamReady.Scenarios;
using ScamReady.Phone;
using TMPro;
using UnityEngine;

namespace ScamReady.UI
{
    public sealed class ScenarioDesktopView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private UnityEngine.UI.Button emailButton;
        [SerializeField] private UnityEngine.UI.Button phoneButton;
        [SerializeField, Tooltip("Button on DesktopPanel/BrowserShortcut.")]
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

        public void Show()
        {
            displayedVisibility = true;
            SetDesktopElementsActive(true);
        }

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
