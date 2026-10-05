using ScamReady.Email;
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

        public string Id => id;
        public string Title => title;
        public string Setup => setup;
        public string GameDateText => gameDateText;
        public EmailData Email => email;
    }
}
