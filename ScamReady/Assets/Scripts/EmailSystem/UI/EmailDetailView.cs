using ScamReady.Responses;
using ScamReady.Scenarios;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private Button proceedButton;
        [SerializeField] private Button stopContactButton;
        [SerializeField] private Button ignoreButton;
        [SerializeField] private Button rejectButton;
        [SerializeField] private TMP_Text proceedLabel;
        [SerializeField] private TMP_Text stopContactLabel;
        [SerializeField] private TMP_Text ignoreLabel;
        [SerializeField] private TMP_Text rejectLabel;

        public void Connect(ScenarioController controller)
        {
            proceedButton.onClick.AddListener(() => controller.ChooseResponse(ContactResponse.Proceed));
            stopContactButton.onClick.AddListener(controller.StopContact);
            ignoreButton.onClick.AddListener(() => controller.ChooseResponse(ContactResponse.Ignore));
            rejectButton.onClick.AddListener(() => controller.ChooseResponse(ContactResponse.Reject));
        }

        public void Bind(EmailData email, ScenarioSession session)
        {
            senderText.text = email.Sender;
            fromText.text = "From: " + email.From;
            replyToText.text = "Reply-To: " + email.ReplyTo;
            replyToText.gameObject.SetActive(!string.IsNullOrEmpty(email.ReplyTo));
            subjectText.text = email.Subject;
            // 文案视为普通内容，避免邮件里的尖括号被 TMP 当作富文本标签。
            bodyText.richText = false;
            bodyText.text = email.Body;
            proceedLabel.text = "Proceed\n" + email.ProceedLabel;
            stopContactLabel.text = "Stop Contact\n" + email.StopContactLabel;
            ignoreLabel.text = "Ignore\n" + email.IgnoreLabel;
            rejectLabel.text = "Reject / Report\n" + email.RejectLabel;

            bool decided = session != null && session.Decision.HasValue;
            proceedButton.interactable = !decided;
            ignoreButton.interactable = !decided;
            rejectButton.interactable = !decided;
            responseText.gameObject.SetActive(decided);
            responseText.text = decided ? "Response recorded: " + session.Decision.Value : string.Empty;
        }
    }
}
