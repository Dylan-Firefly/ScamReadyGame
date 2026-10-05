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
            notificationButton.onClick.AddListener(controller.OpenEmail);
            restartButton.onClick.AddListener(controller.Restart);
            Refresh();
        }

        private void OnDisable()
        {
            controller.Changed -= Refresh;
            emailButton.onClick.RemoveListener(controller.OpenEmail);
            notificationButton.onClick.RemoveListener(controller.OpenEmail);
            restartButton.onClick.RemoveListener(controller.Restart);
        }

        private void Refresh()
        {
            dateText.text = controller.Definition.GameDateText;
            setupText.text = controller.Definition.Setup;
            setupPanel.SetActive(controller.Session == null || !controller.Session.IsEmailOpen);
            bool unread = controller.Session != null && controller.Session.HasUnreadEmail;
            unreadBadge.SetActive(unread);
            notificationText.text = unread
                ? "1 new email  -  " + controller.Definition.Email.Sender
                : "Open inbox";
        }
    }
}
