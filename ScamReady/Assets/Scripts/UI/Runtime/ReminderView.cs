using ScamReady.Scenarios;
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
            panel.SetActive(visible);
            if (!visible) return;
            titleText.text = controller.Definition.Reminder.Title;
            bodyText.richText = false;
            bodyText.text = controller.Definition.Reminder.Body;
        }
    }
}
