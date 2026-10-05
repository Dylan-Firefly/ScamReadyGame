using System;
using UnityEngine;

namespace ScamReady.Feedback
{
    /// <summary>固定分支的可编辑文案；MVP 不配置条件组合或动态规则。</summary>
    [Serializable]
    public sealed class ScenarioFeedbackConfig
    {
        [SerializeField] private FeedbackMessage proceed = new FeedbackMessage(
            "You paid the scammer", "You trusted the sender's claim and paid money to a scammer.");
        [SerializeField] private FeedbackMessage ignore = new FeedbackMessage(
            "You ignored the message", "You did not pay the sender. Was ignoring the message the best response?");
        [SerializeField] private FeedbackMessage reportUnverified = new FeedbackMessage(
            "Safe, but without supporting evidence", "You rejected the suspicious request without collecting evidence.");
        [SerializeField] private FeedbackMessage reportPartial = new FeedbackMessage(
            "Safe, with some verification", "You collected {evidenceCount} evidence item(s), but have not collected all required evidence.");
        [SerializeField] private FeedbackMessage reportVerified = new FeedbackMessage(
            "Safe, with a supported decision", "You collected {evidenceCount} evidence item(s) and all {requiredCount} required items before rejecting the scam.");

        public FeedbackMessage GetMessage(ScenarioOutcome outcome)
        {
            switch (outcome)
            {
                case ScenarioOutcome.Proceed: return proceed;
                case ScenarioOutcome.Ignore: return ignore;
                case ScenarioOutcome.ReportUnverified: return reportUnverified;
                case ScenarioOutcome.ReportPartial: return reportPartial;
                case ScenarioOutcome.ReportVerified: return reportVerified;
                default: throw new ArgumentOutOfRangeException(nameof(outcome));
            }
        }
    }

    [Serializable]
    public sealed class FeedbackMessage
    {
        [SerializeField] private string title;
        [SerializeField, TextArea(3, 10), Tooltip("可使用 {evidenceCount}、{requiredCount}、{collectedRequiredCount} 显示决策时的数量。")]
        private string body;

        public string Title => title;
        public string Body => body;

        public FeedbackMessage(string title, string body)
        {
            this.title = title;
            this.body = body;
        }
    }
}
