using ScamReady.Responses;
using ScamReady.Scenarios;
using ScamReady.Verification;
using TMPro;
using UnityEngine;

namespace ScamReady.Email
{
    /// <summary>邮件详情展示与操作区域；会话状态由控制器提供。</summary>
    public sealed class EmailDetailView : MonoBehaviour
    {
        [SerializeField] private TMP_Text senderText;
        [SerializeField] private TMP_Text fromText;
        [SerializeField] private TMP_Text replyToText;
        [SerializeField] private TMP_Text subjectText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private TMP_Text responseText;
        [SerializeField] private UnityEngine.UI.Button proceedButton;
        [SerializeField] private UnityEngine.UI.Button stopContactButton;
        [SerializeField] private UnityEngine.UI.Button ignoreButton;
        [SerializeField] private UnityEngine.UI.Button rejectButton;
        [SerializeField] private TMP_Text proceedLabel;
        [SerializeField] private TMP_Text stopContactLabel;
        [SerializeField] private TMP_Text ignoreLabel;
        [SerializeField] private TMP_Text rejectLabel;
        private ScenarioController controller;

        public void Connect(ScenarioController controller)
        {
            this.controller = controller;
            proceedButton.onClick.AddListener(() => controller.ChooseResponse(ContactResponse.Proceed));
            stopContactButton.onClick.AddListener(controller.StopContact);
            ignoreButton.onClick.AddListener(() => controller.ChooseResponse(ContactResponse.Ignore));
            rejectButton.onClick.AddListener(() => controller.ChooseResponse(ContactResponse.Reject));
        }

        public void Bind(EmailData email, ScenarioSession session)
        {
            RefreshEmailContent(email);
            RefreshResponseLabels(email);
            RefreshResponseState(session);
        }

        private void RefreshEmailContent(EmailData email)
        {
            senderText.text = email.Sender;
            fromText.text = "From: " + email.From;
            replyToText.text = "Reply-To: " + email.ReplyTo;
            replyToText.gameObject.SetActive(!string.IsNullOrEmpty(email.ReplyTo));
            subjectText.text = email.Subject;
            BrowserLinkText.SetText(bodyText, email.Body, controller.OpenLink);
        }

        private void RefreshResponseLabels(EmailData email)
        {
            proceedLabel.text = "Proceed\n" + email.ProceedLabel;
            stopContactLabel.text = "Stop Contact\n" + email.StopContactLabel;
            ignoreLabel.text = "Ignore\n" + email.IgnoreLabel;
            rejectLabel.text = "Reject / Report\n" + email.RejectLabel;
        }

        private void RefreshResponseState(ScenarioSession session)
        {
            bool decided = session != null && session.Decision.HasValue;
            proceedButton.interactable = !decided;
            ignoreButton.interactable = !decided;
            rejectButton.interactable = !decided;
            responseText.gameObject.SetActive(decided);
            responseText.text = decided ? "Response recorded: " + session.Decision.Value : string.Empty;
        }
    }
}
