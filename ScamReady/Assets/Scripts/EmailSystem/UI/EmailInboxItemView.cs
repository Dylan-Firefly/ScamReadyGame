using TMPro;
using UnityEngine;

namespace ScamReady.Email
{
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
