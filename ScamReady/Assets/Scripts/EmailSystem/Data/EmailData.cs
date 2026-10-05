using System;
using UnityEngine;

namespace ScamReady.Email
{
    /// <summary>邮件内容配置；玩家的阅读状态由 ScenarioSession 保存。</summary>
    [Serializable]
    public sealed class EmailData
    {
        [SerializeField] private string id;
        [SerializeField] private string sender;
        [SerializeField] private string from;
        [SerializeField] private string replyTo;
        [SerializeField] private string subject;
        [SerializeField, TextArea(10, 30)] private string body;
        [SerializeField] private string proceedLabel = "Reply and request instructions";
        [SerializeField] private string stopContactLabel = "Close email and verify independently";
        [SerializeField] private string ignoreLabel = "Leave the email unanswered";
        [SerializeField] private string rejectLabel = "Reject / report this email";

        public string Id => id;
        public string Sender => sender;
        public string From => from;
        public string ReplyTo => replyTo;
        public string Subject => subject;
        public string Body => body;
        public string ProceedLabel => proceedLabel;
        public string StopContactLabel => stopContactLabel;
        public string IgnoreLabel => ignoreLabel;
        public string RejectLabel => rejectLabel;
    }
}
