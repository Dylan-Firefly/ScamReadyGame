using ScamReady.Scenarios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScamReady.UI
{
    /// <summary>桌面入口与新邮件提示。所有引用在场景 Inspector 中绑定。</summary>
    public sealed class ScenarioDesktopView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private Button emailButton;
        [SerializeField, Tooltip("绑定 DesktopPanel/BrowserShortcut 的按钮。")]
        private Button browserButton;
        [SerializeField] private Button notificationButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private GameObject unreadBadge;
        [SerializeField] private TMP_Text notificationText;
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text setupText;
        [SerializeField] private GameObject setupPanel;

        private void OnEnable()
        {
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
            dateText.text = controller.Definition.GameDateText;
            setupText.text = controller.Definition.Setup;
            bool onDesktop = controller.Session == null || controller.Session.ActiveApp == ScenarioApp.Desktop;
            setupPanel.SetActive(onDesktop);
            emailButton.gameObject.SetActive(onDesktop);
            browserButton.gameObject.SetActive(onDesktop);
            notificationButton.gameObject.SetActive(onDesktop);
            bool unread = controller.Session != null && controller.Session.HasUnreadEmail;
            unreadBadge.SetActive(unread);
            notificationText.text = unread
                ? "1 new email  -  " + controller.Definition.Email.Sender
                : "Open inbox";
        }
    }
}
