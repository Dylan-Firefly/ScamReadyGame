using ScamReady.Scenarios;
using ScamReady.Verification;
using TMPro;
using UnityEngine;

namespace ScamReady.UI
{
    /// <summary>挂在常驻 ReminderUI 上，仅显示或关闭一次提醒，不自动打开邮件。</summary>
    public sealed class ReminderView : MonoBehaviour
    {
        [SerializeField] private ScenarioController controller;
        [SerializeField] private GameObject panel;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;

        private void Awake() => closeButton.onClick.AddListener(controller.DismissReminder);

        private void OnEnable()
        {
            controller.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => controller.Changed -= Refresh;

        private void Refresh()
        {
            var session = controller.Session;
            bool visible = session != null && session.IsReminderVisible && !session.IsEvidenceSummaryOpen;
            if (visible) RefreshContent();
            SetVisible(visible);
        }

        private void RefreshContent()
        {
            titleText.text = controller.Definition.Reminder.Title;
            BrowserLinkText.SetText(bodyText, controller.Definition.Reminder.Body, controller.OpenLink);
        }

        private void SetVisible(bool visible)
        {
            if (visible) Show();
            else Hide();
        }

        /// <summary>仅显示提醒面板；显示动效在这里接入，不修改提醒触发或已读状态。</summary>
        public void Show()
        {
            if (panel.activeSelf) return;
            panel.SetActive(true);
        }

        /// <summary>仅隐藏提醒面板。玩家关闭提醒仍调用 Controller.DismissReminder。</summary>
        public void Hide()
        {
            if (!panel.activeSelf) return;
            panel.SetActive(false);
        }
    }
}
