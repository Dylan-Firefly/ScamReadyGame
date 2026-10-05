using TMPro;
using UnityEngine;

namespace ScamReady.Email
{
    /// <summary>单封邮件的列表项。以后扩展多邮件时可复用为行 Prefab。</summary>
    public sealed class EmailInboxItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text senderText;
        [SerializeField] private TMP_Text subjectText;

        public void Bind(EmailData email)
        {
            senderText.text = email.Sender;
            subjectText.text = email.Subject;
        }
    }
}
