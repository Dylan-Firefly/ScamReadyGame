using System.Collections.Generic;
using ScamReady.Email;
using ScamReady.Verification;
using UnityEngine;

namespace ScamReady.Scenarios
{
    /// <summary>可复用的邮件情境内容。运行时不修改这个资产。</summary>
    [CreateAssetMenu(menuName = "Scam Ready/Email Scenario", fileName = "EmailScenario")]
    public sealed class EmailScenarioDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField, TextArea(2, 5)] private string setup;
        [SerializeField] private string gameDateText;
        [SerializeField] private EmailData email = new EmailData();
        [SerializeField, Tooltip("拖入本关可访问的官网页面资产。当前浏览器提供两个入口。")]
        private VerificationPageDefinition[] verificationPages = new VerificationPageDefinition[0];

        public string Id => id;
        public string Title => title;
        public string Setup => setup;
        public string GameDateText => gameDateText;
        public EmailData Email => email;
        public IReadOnlyList<VerificationPageDefinition> VerificationPages => verificationPages;
    }
}
